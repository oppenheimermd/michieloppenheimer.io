using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Blog.Models.Helpers
{
    public static class TagExtensions
    {
        public static Task<List<PostTag>> ProcessTagsAsync(this List<string> tags, int postId)
        {
            /*  //  Use lambda expression
                return Task.Run<List<PostTag>>(() =>
                {
                    return ProcessTags(tags, postId);
                });
             */

            return Task.Run<List<PostTag>>(() => ProcessTags(tags, postId));
        }

        private static List<PostTag> ProcessTags(this List<string> tags, int postId)
        {
            //  list of tags to return
            var newTags = new List<string>();

            tags.ForEach(tag =>
            {
                if (tag != null && tag.Trim() != string.Empty && newTags.SingleOrDefault(newtag => newtag.ToLower() == tag.ToLower()) == null)
                    newTags.Add(tag.Trim().ToLower());
            });

            var newTagsList = new List<PostTag>();
            newTags.ForEach(tag =>
            {
                var slug = tag;
                newTagsList.Add(new PostTag { TagName = tag.ToLowerInvariant(), PostId = postId });
            });

            return newTagsList;
        }
    }
}
