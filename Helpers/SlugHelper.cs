namespace AIBlog.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "";

            var slug = title.ToLowerInvariant()
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u")
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("\"", "")
                .Replace("'", "");

            return slug;
        }
    }
}
