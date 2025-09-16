using babiltr.EntityLayer;
using babiltr.Models.Translation;
using babiltr.Services;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    public class TranslationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TranslationController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment = null)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Segmentlere Ayrılmış Dosyanın gösterilmesi (İşveren)
        [HttpGet]
        [CheckAccess("ShowSegments")]
        public IActionResult ShowSegments(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);
            if (job == null)
            {
                return NotFound();
            }

            ViewBag.JobId = id;
            ViewBag.JobTitle = job.Title;
            ViewBag.OriginalLanguage = job.OriginalLanguage;
            ViewBag.TargetLanguage = job.TargetLanguage;
            ViewBag.CompletionPercentage = job.CompletionPercentage;

            return View();
        }

        //İlan Sahibine Çevrilecek içeriğin cümleler ile listelenmesi (datatable)
        [HttpGet]
        [CheckAccess("ShowSegments")]
        public IActionResult ShowSegmentsTable(int id)
        {
            var translations = _context.Translations
                .Where(t => t.JobId == id)
                .Select(t => new
                {
                    t.Id,
                    originalSentence = t.OriginalSentence,
                })
                .ToList();

            return Json(new { data = translations });
        }

        // Segmentlere Ayrılmış Dosyanın gösterilmesi ve çevrilmesi (çevirmen)
        [HttpGet]
        [CheckAccess("Translation")]
        public IActionResult Translation(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);
            if (job == null)
            {
                return NotFound();
            }

            ViewBag.JobId = id;
            ViewBag.JobTitle = job.Title;
            ViewBag.OriginalLanguage = job.OriginalLanguage;
            ViewBag.TargetLanguage = job.TargetLanguage;
            ViewBag.CompletionPercentage = job.CompletionPercentage;

            return View();
        }

        //Çevirmene çevrilecek içeriğin cümleler ile listelenmesi (datatable)
        [HttpGet]
        [CheckAccess("Translation")]
        public IActionResult TranslationTable(int id)
        {
            var translations = _context.Translations
                .Where(t => t.JobId == id)
                .Select(t => new
                {
                    id = t.Id,
                    translatedSentence = t.TranslatedSentence,
                    originalSentence = t.OriginalSentence,
                })
                .ToList();

            return Json(new { data = translations });
        }

        // Segmentlere ayırma kodu
        public IActionResult SeparateSegments(string filename, int jobID)
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", "OriginalFiles", filename);

            if (System.IO.File.Exists(filePath))
            {
                StringBuilder textBuilder = new StringBuilder();
                string currentHeading = null;

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;

                    foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                    {
                        // Paragrafın stil bilgisini al
                        var paragraphStyle = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

                        // Eğer stil adı "balk1", "balk2" gibi başlık stilini ifade ediyorsa
                        if (!string.IsNullOrEmpty(paragraphStyle) && paragraphStyle.StartsWith("balk", StringComparison.OrdinalIgnoreCase))
                        {
                            // Eğer mevcut başlık varsa, başlık kaydedilecek
                            if (!string.IsNullOrEmpty(currentHeading))
                            {
                                // Başlık kaydedilecek, önceki başlık kaydedildi
                                SaveSegmentToDatabase(jobID, currentHeading, isHeading: true);
                            }

                            // Yeni başlık belirleniyor
                            currentHeading = CleanUpSentence(paragraph.InnerText);
                        }
                        else
                        {
                            // Metin paragrafı ise, başlığı hemen kaydediyoruz
                            // Başlık varsa hemen kaydediyoruz
                            if (!string.IsNullOrEmpty(currentHeading))
                            {
                                // Başlık kaydediliyor
                                SaveSegmentToDatabase(jobID, currentHeading, isHeading: true);
                                // Başlık kaydedildikten sonra, başlığı sıfırlıyoruz
                                currentHeading = null;
                            }

                            // Paragraf bir metin paragrafı ise, metni biriktiriyoruz
                            foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                            {
                                textBuilder.Append(run.InnerText + " ");
                            }

                            // Paragraf tamamlandığında, metni cümlelere ayırıyoruz
                            var paragraphText = textBuilder.ToString().Trim();
                            if (!string.IsNullOrEmpty(paragraphText))
                            {
                                var sentences = SplitTextIntoSentences(paragraphText);

                                // Cümleleri tek tek kaydediyoruz, başlık burada yer almayacak
                                foreach (var sentence in sentences)
                                {
                                    SaveSegmentToDatabase(jobID, CleanUpSentence(sentence), isHeading: false);
                                }

                                // TextBuilder'ı sıfırlıyoruz, çünkü her paragraf sonrası temizlemeliyiz
                                textBuilder.Clear();
                            }
                        }
                    }

                    // Son başlık kaydedildikten sonra iş tamamlanır
                    if (!string.IsNullOrEmpty(currentHeading))
                    {
                        // Son başlık kaydedilecek
                        SaveSegmentToDatabase(jobID, currentHeading, isHeading: true);
                    }
                }

                // İşin tamamlanma yüzdesini güncelle
                UpdateJobCompletionPercentage(jobID);
                return RedirectToAction("MyCreatedJobs", "Jobs");
            }

            return BadRequest("File not found.");
        }

        // Metni cümlelere ayırma
        private List<string> SplitTextIntoSentences(string text)
        {
            var sentenceEndings = new[] { '.', '?', '!' };
            var sentences = new List<string>();
            var currentSentence = new StringBuilder();

            foreach (var ch in text)
            {
                currentSentence.Append(ch);

                // Cümle sonu noktalama işaretlerini kontrol et
                if (sentenceEndings.Contains(ch))
                {
                    sentences.Add(currentSentence.ToString().Trim());
                    currentSentence.Clear();
                }
            }

            // Son cümleyi ekle
            if (currentSentence.Length > 0)
            {
                sentences.Add(currentSentence.ToString().Trim());
            }

            return sentences;
        }

        // Segmenti veritabanına kaydet
        private void SaveSegmentToDatabase(int jobID, string segment, bool isHeading)
        {
            var translation = new Translation
            {
                JobId = jobID,
                OriginalSentence = segment,
                IsHeading = isHeading
            };

            _context.Translations.Add(translation);
            _context.SaveChanges();
        }

        // Fazla boşlukları temizle
        private string CleanUpSentence(string sentence)
        {
            return string.Join(" ", sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        // İş Tamamlanma Yüzdesinin Hesaplanması
        [HttpGet]
        private void UpdateJobCompletionPercentage(int jobID)
        {
            var totalSegments = _context.Translations.Count(t => t.JobId == jobID);
            var translatedSegments = _context.Translations.Count(t => t.JobId == jobID && !string.IsNullOrEmpty(t.TranslatedSentence));

            if (totalSegments > 0)
            {
                var completionPercentage = (double)translatedSegments / totalSegments * 100;

                var job = _context.Jobs.FirstOrDefault(j => j.JobID == jobID);
                if (job != null)
                {
                    job.CompletionPercentage = (int)Math.Round(completionPercentage);
                    _context.SaveChanges();
                }
            }
        }

        // Bitmiş İş İçin Dosya Oluşturma(word)
        [HttpGet]
        public IActionResult GenerateFile(int jobID)
        {
            var segments = _context.Translations
                .Where(t => t.JobId == jobID)
                .OrderBy(t => t.Id)
                .Select(t => new { t.TranslatedSentence, t.IsHeading })
                .ToList();

            if (!segments.Any())
            {
                return BadRequest("No segments found.");
            }

            var outputPath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", "TranslatedFiles");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var wordFileName = $"TranslatedJob_{jobID}.docx";
            var wordFilePath = Path.Combine(outputPath, wordFileName);

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(wordFilePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body());

                var body = mainPart.Document.Body;

                DocumentFormat.OpenXml.Wordprocessing.Paragraph currentParagraph = null;

                foreach (var segment in segments)
                {
                    if (segment.IsHeading)
                    {
                        if (currentParagraph != null)
                        {
                            body.AppendChild(currentParagraph);
                        }

                        var heading = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                                new DocumentFormat.OpenXml.Wordprocessing.ParagraphStyleId() { Val = "Heading1" }
                            ),
                            new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(segment.TranslatedSentence))
                        );
                        body.AppendChild(heading);

                        currentParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    }
                    else
                    {
                        if (currentParagraph == null)
                        {
                            currentParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                        }

                        var textRun = new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(segment.TranslatedSentence));
                        currentParagraph.AppendChild(textRun);

                        var spaceRun = new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(" "));
                        currentParagraph.AppendChild(spaceRun);
                    }
                }

                if (currentParagraph != null)
                {
                    body.AppendChild(currentParagraph);
                }
            }

            // Dosya adını veritabanına kaydet
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == jobID);
            if (job != null)
            {
                job.TranslatedFileName = wordFileName;
                _context.SaveChanges();
            }
            return RedirectToAction("UserApplications", "Applications");
        }

        // Çevirmenin Çeviri Metinlerini Kaydedilmesi
        [HttpPost]
        public IActionResult UpdateSegment([FromBody] List<TranslationUpdateModel> translations)
        {
            if (translations == null || !translations.Any())
            {
                return BadRequest("No translations provided.");
            }

            foreach (var translation in translations)
            {
                var existingTranslation = _context.Translations.FirstOrDefault(t => t.Id == translation.Id);

                if (existingTranslation != null)
                {
                    existingTranslation.TranslatedSentence = translation.TranslatedSentence ?? "";
                }
                else
                {
                    var newTranslation = new Translation
                    {
                        Id = translation.Id,
                        TranslatedSentence = translation.TranslatedSentence ?? ""
                    };

                    _context.Translations.Add(newTranslation);
                }
            }
            _context.SaveChanges();

            var jobID = translations.FirstOrDefault()?.JobId ?? 0;
            if (jobID > 0)
            {
                UpdateJobCompletionPercentage(jobID);
            }

            return Ok("Segments updated successfully.");
        }
    }
}
