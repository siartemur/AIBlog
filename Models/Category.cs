namespace AIBlog.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }

        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
