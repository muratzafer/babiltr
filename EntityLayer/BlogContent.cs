namespace babiltr.EntityLayer
{
    public class BlogContent
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Subheading { get; set; }
        public string Paragraph { get; set; }
        public Blog Blog { get; set; }
    }
}
