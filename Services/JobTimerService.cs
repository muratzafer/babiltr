using Hangfire;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

public class JobTimerService
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IConfiguration _configuration;

    public JobTimerService(ApplicationDbContext context, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager, IConfiguration configuration = null)
    {
        _context = context;
        _backgroundJobs = backgroundJobs;
        _recurringJobManager = recurringJobManager;
        _configuration = configuration;
    }

    public void ScheduleTransferPay(int jobId)
    {
        _backgroundJobs.Schedule(() => TransferPay(jobId), TimeSpan.FromDays(7));
    }

    public async Task TransferPay(int jobId)
    {
        Log.Information("TransferPay method started for Job ID: {JobId}", jobId);

        var job = _context.Jobs.FirstOrDefault(j => j.JobID == jobId);

        if (job == null)
        {
            Log.Warning("Job not found for Job ID: {JobId}", jobId);
            return;
        }

        var application = _context.Applications.FirstOrDefault(a => a.JobID == jobId && a.ApplicationStatus == "Approved");

        if (application == null)
        {
            Log.Warning("No approved application found for Job ID: {JobId}", jobId);
            return;
        }

        var approvedUser = _context.Users.FirstOrDefault(u => u.Id == application.UserID);

        if (approvedUser == null)
        {
            Log.Warning("Approved user not found for Application ID: {ApplicationId}", application.ApplicationID);
            return;
        }

        // Retrieve PayTR settings from configuration
        string merchant_id = _configuration["PayTR:MerchantId"];
        string merchant_key = _configuration["PayTR:MerchantKey"];
        string merchant_salt = _configuration["PayTR:MerchantSalt"];

        // Order and transaction details
        string merchant_oid = job.MmerchantOid;

        int uniqueCounter = new Random().Next(100, 999);
        string trans_id = "2" + job.JobID.ToString() + approvedUser.Id.ToString() + uniqueCounter.ToString();

        // Commission details
        var commission = _context.Commissions.FirstOrDefault();
        if (commission == null)
        {
            Log.Error("Commission settings not found in the database.");
            return;
        }

        // Calculate amounts in cents (integer)
        int submerchant_amount = (int)((job.Price - (job.Price * commission.TranslatorPercentage / 100)) * 100);
        int total_amount = (int)(job.Price * 100);

        // Transfer recipient details
        string transfer_name = approvedUser.FirstName + " " + approvedUser.LastName;
        string transfer_iban = approvedUser.Iban?.Replace(" ", "");

        if (string.IsNullOrEmpty(transfer_iban))
        {
            Log.Error("IBAN is missing for User ID: {UserId}", approvedUser.Id);
            return;
        }

        // Token generation
        string Birlestir = string.Concat(
            merchant_id,
            merchant_oid,
            trans_id,
            submerchant_amount.ToString(),
            total_amount.ToString(),
            transfer_name,
            transfer_iban,
            merchant_salt
        );

        try
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key)))
            {
                byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
                string token = Convert.ToBase64String(b);

                // Prepare data for API request
                NameValueCollection data = new NameValueCollection
            {
                { "merchant_id", merchant_id },
                { "merchant_oid", merchant_oid },
                { "trans_id", trans_id },
                { "submerchant_amount", submerchant_amount.ToString() },
                { "total_amount", total_amount.ToString() },
                { "transfer_name", transfer_name },
                { "transfer_iban", transfer_iban },
                { "paytr_token", token }
            };

                Log.Information("Initiating transfer for Transaction ID: {TransactionId}", trans_id);

                // Send the data to PayTR
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    byte[] response = client.UploadValues("https://www.paytr.com/odeme/platform/transfer", "POST", data);
                    string responseString = Encoding.UTF8.GetString(response);
                    dynamic jsonResponse = JObject.Parse(responseString);

                    if (jsonResponse.status == "success")
                    {
                        Log.Information("Transfer successful for Transaction ID: {TransactionId}", trans_id);
                    }
                    else
                    {
                        Log.Warning("Transfer failed for Transaction ID: {TransactionId}. Response: {Response}", trans_id, responseString);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during transfer for Transaction ID: {TransactionId}", trans_id);
        }
    }

    // Timer başlatma işlemi
    public void StartJobTimer(int jobId)
    {
        var startTime = GetTurkeyTime();
        var job = _context.Jobs.Find(jobId);

        if (job != null)
        {
            job.TimerStartTime = startTime;
            _context.SaveChanges();
        }

        _recurringJobManager.AddOrUpdate(
            $"JobTimer_{jobId}",
            () => MonitorJobDuration(jobId),
            "0 * * * *");
    }

    // Timer'ı iptal etme işlemi
    public void CancelJobTimer(int jobId)
    {
        _recurringJobManager.RemoveIfExists($"JobTimer_{jobId}");
    }

    // Zamanlayıcıyı kontrol eden method
    public async Task MonitorJobDuration(int jobId)
    {
        var job = await _context.Jobs.FindAsync(jobId);

        if (job == null || job.Status == "Completed" || job.Status == "Revision")
        {
            CancelJobTimer(jobId);
            return;
        }

        var startTime = job.TimerStartTime;

        if (!startTime.HasValue)
        {
            return;
        }

        var elapsedTime = GetTurkeyTime() - startTime.Value;

        if (elapsedTime >= TimeSpan.FromDays(3))
        {
            job.Status = "Completed";
            await _context.SaveChangesAsync();
        }
    }

    private DateTime GetTurkeyTime()
    {
        var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
    }
}
