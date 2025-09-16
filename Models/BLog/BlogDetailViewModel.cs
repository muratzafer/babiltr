using babiltr.EntityLayer;
namespace babiltr.Models.BLog
{
    public class BlogDetailViewModel
    {
        public Blog Blog { get; set; }
        public List<Blog> LatestBlogs { get; set; }
        public List<string> Tags { get; set; }
    }

}
