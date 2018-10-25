using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Blog.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        [Required]
        public string Excerpt { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime PubDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public bool IsPublished { get; set; } = false;

        public string PostCoverPhoto { get; set; } = String.Empty;

        public bool PostHasCoverPhoto()
        {
            return (!string.IsNullOrEmpty(PostCoverPhoto));
        }

        public IList<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();

        public List<PostMedia> Media { get; set; } = new List<PostMedia>();

        public string CoverPhotoUrl()
        {
            return $"/PostMedia/{PostCoverPhoto}";
        }

        public string GetLink()
        {
            return $"/blog/{Slug}/";
        }

        public bool AreCommentsOpen(int commentsCloseAfterDays)
        {
            return PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;
        }

        public string RenderContent()
        {
            var result = Content;

            // Set up lazy loading of images/iframes
            result = result.Replace(" src=\"", " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"");

            // Youtube content embedded using this syntax: [youtube:xyzAbc123]
            var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
            result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => string.Format(video, m.Groups[1].Value));

            return result;
        }
    }
}
