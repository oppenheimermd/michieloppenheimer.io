using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Blog.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Author { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(400)]
        public string Content { get; set; }

        [Required]
        public DateTime PubDate { get; set; } = DateTime.UtcNow;

        public bool CommentApproved { get; set; } = false;

        public bool IsAdmin { get; set; }

        public int PostId { get; set; }
    }
}
