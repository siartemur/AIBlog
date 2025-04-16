using AIBlog.Models;

namespace AIBlog.ViewModels
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public DateTime PublishedOn { get; set; }
        public string AuthorName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public List<string> Tags { get; set; } = new();

        public List<Comment> Comments { get; set; } = new(); 
        public string? NewCommentText { get; set; } 
    }
}
