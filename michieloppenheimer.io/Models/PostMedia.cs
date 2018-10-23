using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable once CheckNamespace
namespace Blog.Models
{
    public class PostMedia
    {
        [Key]
        public  int Id { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public string FileName { get; set; }

        public string MedialUrl { get; set; }

        public Post Post { get; set; }
    }
}
