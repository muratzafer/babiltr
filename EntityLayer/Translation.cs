using System.ComponentModel.DataAnnotations;

namespace babiltr.EntityLayer
{
    public class Translation
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string OriginalSentence { get; set; }
        public string? TranslatedSentence { get; set; }
        public bool IsHeading { get; set; }
        public virtual Jobs Job { get; set; }
    }
}
