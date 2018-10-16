using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public class BlogService : IBlogService
    {
        private BlogContext _blogContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public BlogService(BlogContext blogContext, IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _blogContext = blogContext;
        }

        public virtual async Task<IEnumerable<Post>> GetPostsAsync(int count, int skip = 0)
        {
            var posts = await _blogContext.Posts
                .AsNoTracking()
                .Where(x => x.PubDate <= DateTime.UtcNow && (x.IsPublished == true))
                .Skip(skip)
                .Take(count).ToListAsync();

            return posts;
        }

        /// <summary>
        /// Only use in Console controller(returns ALL posts)
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IQueryable<Post>> GetAllPostAsync()
        {

            var posts = await _blogContext.Posts
                .AsNoTracking().ToListAsync();

            return posts.AsQueryable();
        }

        //  Is this really necessay? using GetAllPostAsync() can handle these
        //  types queries.
        /*public virtual Task<IEnumerable<Post>> GetPostsByCategory(string category)
        {
            var posts = from p in _blogContext.Posts
                where p.PubDate <= DateTime.UtcNow && p.IsPublished == true
                where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
                select p;

            return Task.FromResult(posts);
        }*/

        public virtual async Task<Post> GetPostBySlugAsync(string slug)
        {

            var post = await _blogContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

            var isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
            {
                return post;
            }

            return null;
        }

        public virtual async Task<Post> GetPostByIdAsync(int id)
        {
            var post = await _blogContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            var isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
            {
                return post;
            }

            return null;
        }

        public async Task<bool> SavePostAsync(Post post)
        {
            post.LastModified = DateTime.UtcNow;

            try
            {
                _blogContext.Posts.Add(post);
                await _blogContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _blogContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> EditPost(int id, Post post)
        {
            var postToUpdate = await _blogContext.Posts.FindAsync(post.Id);
            

            if (postToUpdate != null)
            {
                postToUpdate.LastModified = DateTime.UtcNow;
                _blogContext.Posts.Update(postToUpdate);
                await _blogContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }


        //  Helper methods
        protected bool IsAdmin()
        {
            return _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
        }

    }
}
