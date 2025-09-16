namespace babiltr.Models.Translation
{
    public class TranslationUpdateModel
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string? TranslatedSentence { get; set; }
    }
}
