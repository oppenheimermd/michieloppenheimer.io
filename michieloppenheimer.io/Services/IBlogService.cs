using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public interface IBlogService
    {
        Task<IEnumerable<Post>> GetPostsAsync(int count, int skip = 0);

        Task<IQueryable<Post>> GetAllPostAsync();

        Task<Post> GetPostBySlugAsync(string slug);

        Task<Post> GetPostByIdAsync(int id);

        Task<bool> SavePostAsync(Post post);

        Task DeletePostAsync(int id);

        Task<bool> EditPost(int id, Post post);
    }
}
