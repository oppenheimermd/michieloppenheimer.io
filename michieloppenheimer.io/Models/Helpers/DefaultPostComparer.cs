using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Blog.Models.Helpers
{
    /// <summary>
    /// .NET doesn’t automatically know how to compare the <see cref="Post"/> objects in a way that is useful
    /// for our program. Instead, the comparison will be based on reference equality.  As such we need to specify own
    /// equality comparer.
    /// </summary>
    public class DefaultPostComparer : IEqualityComparer<Post>
    {
        public bool Equals(Post x, Post y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Post obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
