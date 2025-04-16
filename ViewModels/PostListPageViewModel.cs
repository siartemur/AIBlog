namespace AIBlog.ViewModels
{
    public class PostListPageViewModel
    {
        public List<PostListViewModel> Posts { get; set; } = new List<PostListViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string? CurrentTag { get; set; }
        public string? CurrentCategory { get; set; }
    }
}
