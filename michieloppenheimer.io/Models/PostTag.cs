using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Blog.Models
{
    public class PostTag
    {
        [Key]
        public int TagId { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        [DataMember]
        public string TagName { get; set; }

        public Post Post { get; set; }

    }
}
