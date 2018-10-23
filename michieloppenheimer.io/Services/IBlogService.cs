using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public interface IBlogService
    {
        //  Queries

        Task<IEnumerable<Post>> GetPostsAsync(int count, int skip = 0);

        Task<IQueryable<Post>> GetAllPostAsync();

        Task<Post> GetPostBySlugAsync(string slug);

        Task<Post> GetPostByIdAsync(int id);

        Task<IQueryable<PostMedia>> GetPostMediaByPostAsync(int postId);

        Task<List<PostTag>> GetPostTags(int postId);

        //  Create / Update

        /// <summary>
        /// Persistence, save blog to data store
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        Task SavePostAsync(Post post);

        Task UpdatePostAsync(Post post);

        Task<Post> SaveFilesToDiskAsync(Post post);

        Task<(List<PostMedia> newMedia, List<PostMedia> oldMedia)> UpdatePostMediaAsync(Post post, IReadOnlyCollection<PostMedia> oldMedia);

        Task SavePostTagAsync(PostTag postTag);

        Task EditPost(int id, Post post);

        //  Not tested yet
        //Task DeletePostAsync(int id);

        //  Delete

        Task DeletePostTagAsync(int tagId);

    }
}
