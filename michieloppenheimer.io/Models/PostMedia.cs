using System.ComponentModel.DataAnnotations;

// ReSharper disable once CheckNamespace
namespace Blog.Models
{
    public class PostMedia
    {
        [Key]
        public  int Id { get; set; }
        public int PostId { get; set; }
        public string FileName { get; set; }
        public string MedialUrl { get; set; }
    }
}
