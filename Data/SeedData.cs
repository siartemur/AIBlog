using AIBlog.Models;
using AIBlog.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AIBlog.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Users.Any() || context.Posts.Any()) return;

            // Kullanıcılar
            var admin = new User { UserName = "admin", Name = "Admin", Email = "admin@aiblog.com", Password = "123456", Role = UserRole.Admin, Image = "admin.png" };
            var editor = new User { UserName = "editor", Name = "Editor", Email = "editor@aiblog.com", Password = "123456", Role = UserRole.Editor, Image = "editor.png" };
            var user = new User { UserName = "john", Name = "John Doe", Email = "john@aiblog.com", Password = "123456", Role = UserRole.User, Image = "john.png" };

            context.Users.AddRange(admin, editor, user);
            context.SaveChanges();

            // Kategoriler
            var categories = new List<Category>
            {
                new Category { Name = "Yapay Zeka", Url = "yapay-zeka" },
                new Category { Name = "Makine Öğrenimi", Url = "makine-ogrenimi" },
                new Category { Name = "Derin Öğrenme", Url = "derin-ogrenme" },
                new Category { Name = "Veri Bilimi", Url = "veri-bilimi" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Tag'ler
            var tags = new List<Tag>
            {
                new Tag { Text = "AI", Url = "ai", Color = TagColors.primary },
                new Tag { Text = "ML", Url = "ml", Color = TagColors.success },
                new Tag { Text = "ChatGPT", Url = "chatgpt", Color = TagColors.info },
                new Tag { Text = "DeepLearning", Url = "deep-learning", Color = TagColors.warning }
            };
            context.Tags.AddRange(tags);
            context.SaveChanges();

            // Postlar
            var post1 = new Post
            {
                Title = "ChatGPT ile Proje Geliştirme",
                Content = "ChatGPT artık kod da yazıyor!",
                Description = "Yapay zeka destekli üretkenlik",
                Url = SlugHelper.GenerateSlug("ChatGPT ile Proje Geliştirme"),
                Image = "~/images/post1.jpg",
                PublishedOn = DateTime.UtcNow.AddDays(-10),
                IsActive = true,
                User = admin,
                Category = categories[0],
                Tags = new List<Tag> { tags[0], tags[2] }
            };

            var post2 = new Post
            {
                Title = "Makine Öğrenimine Giriş",
                Content = "ML temel kavramlar...",
                Description = "Yeni başlayanlar için ML",
                Url = SlugHelper.GenerateSlug("Makine Öğrenimine Giriş"),
                Image = "~/images/post2.jpg",
                PublishedOn = DateTime.UtcNow.AddDays(-5),
                IsActive = true,
                User = user,
                Category = categories[1],
                Tags = new List<Tag> { tags[1] }
            };

            var post3 = new Post
            {
                Title = "Deep Learning ile Görüntü İşleme",
                Content = "Convolutional Neural Networks...",
                Description = "Görüntü işleme alanında derin öğrenme",
                Url = SlugHelper.GenerateSlug("Deep Learning ile Görüntü İşleme"),
                Image = "~/images/post3.jpg",
                PublishedOn = DateTime.UtcNow.AddDays(-2),
                IsActive = false, // Pasif
                User = editor,
                Category = categories[2],
                Tags = new List<Tag> { tags[0], tags[3] }
            };

            context.Posts.AddRange(post1, post2, post3);
            context.SaveChanges();

            // Yorumlar
            var comment1 = new Comment
            {
                Text = "Çok faydalı içerik!",
                PublishedOn = DateTime.UtcNow.AddDays(-8),
                Post = post1,
                User = user
            };

            var comment2 = new Comment
            {
                Text = "ML konusunu sade ve anlaşılır anlatmışsınız.",
                PublishedOn = DateTime.UtcNow.AddDays(-4),
                Post = post2,
                User = editor
            };

            context.Comments.AddRange(comment1, comment2);
            context.SaveChanges();
        }
    }
}
