namespace AIBlog.ViewModels
{
    public class PostListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? CategoryName { get; set; }
        public string? AuthorName { get; set; }
        public string? Url { get; set; }
        public DateTime PublishedOn { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
