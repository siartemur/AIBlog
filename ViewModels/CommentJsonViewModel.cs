namespace AIBlog.ViewModels
{
    public class CommentJsonViewModel
    {
        public string text { get; set; } = null!;
        public DateTime publishedOn { get; set; }
        public string userName { get; set; } = null!;
        public string userImage { get; set; } = "default.jpg";
    }
}
