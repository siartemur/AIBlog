namespace AIBlog.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Image { get; set; }
        public DateTime PublishedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }  // ✅ Nullable yapıldı

        public int CategoryId { get; set; }
        public Category? Category { get; set; } // ✅ Nullable yapıldı

        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
