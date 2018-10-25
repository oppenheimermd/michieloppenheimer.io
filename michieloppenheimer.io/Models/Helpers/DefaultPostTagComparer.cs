using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Blog.Models.Helpers
{
    /// <summary>
    /// .NET doesn’t automatically know how to compare the <see cref="PostTag"/> objects in a way that is useful
    /// for our program. Instead, the comparison will be based on reference equality.  As such we need to specify own
    /// equality comparer.
    /// </summary>
    public class DefaultPostTagComparer : IEqualityComparer<PostTag>
    {
        public bool Equals(PostTag x, PostTag y)
        {
            return x.TagName.ToLowerInvariant() == y.TagName.ToLowerInvariant();
        }

        public int GetHashCode(PostTag obj)
        {
            return obj.TagName.GetHashCode();
        }
    }
}
