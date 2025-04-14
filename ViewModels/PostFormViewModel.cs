using System.ComponentModel.DataAnnotations;

namespace AIBlog.ViewModels
{
    public class PostFormViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(30, ErrorMessage = "Başlık en fazla 30 karakter olabilir.")]
        public string Title { get; set; } = null!;

        [StringLength(120, ErrorMessage = "Açıklama en fazla 120 karakter olabilir.")]
        public string? Description { get; set; }

        [StringLength(1500, ErrorMessage = "İçerik en fazla 1500 karakter olabilir.")]
        public string? Content { get; set; }

        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; } 
        public string? TagNames { get; set; } // etiketleri virgülle alacağız
    }
}
