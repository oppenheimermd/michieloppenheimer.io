using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public class BlogService : IBlogService
    {
        private BlogContext _blogContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _folder;
        private readonly string _filesFolder;
        /// <summary>
        /// Base directory location for medai
        /// </summary>
        private readonly string _fileLocationPrefix;

        public BlogService(BlogContext blogContext, IHttpContextAccessor contextAccessor, IHostingEnvironment env)
        {
            _contextAccessor = contextAccessor;
            _blogContext = blogContext;
            _folder = Path.Combine(env.WebRootPath, "PostMedia");
            _filesFolder = Path.Combine(env.WebRootPath, "PostMedia");
            _fileLocationPrefix = "/PostMedia/";
        }

        //  Queries

        public virtual async Task<IEnumerable<Post>> GetPostsAsync(int count, int skip = 0)
        {
            var posts = await _blogContext.Posts
                .AsNoTracking()
                .Where(x => x.PubDate <= DateTime.UtcNow && (x.IsPublished == true))
                .Include(x => x.Tags)
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
                .AsNoTracking()
                .Include(x => x.Tags)
                .ToListAsync();

            return posts.AsQueryable();
        }

        public virtual async Task<Post> GetPostBySlugAsync(string slug)
        {

            var post = await _blogContext.Posts
                .AsNoTracking()
                .Include(x => x.Tags)
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
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            var isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
            {
                return post;
            }

            return null;
        }

        public virtual async Task<IQueryable<PostMedia>> GetPostMediaByPostAsync(int postId)
        {
            var postMedia = await _blogContext.PostMedias
                .AsNoTracking()
                .Where(x => x.PostId == postId)
                .ToListAsync();

            return postMedia.AsQueryable();
        }

        public virtual async Task<List<PostTag>> GetPostTags(int postId)
        {
            var postTags = await _blogContext.PostTags
                .AsNoTracking()
                .Where(x => x.PostId == postId)
                .ToListAsync();

            return postTags;
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


        //  Create / Update

        /// <summary>
        /// Persistence, save blog to data store
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task SavePostAsync(Post post)
        {
            post.LastModified = DateTime.UtcNow;
            _blogContext.Posts.Add(post);
            await _blogContext.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            //var postToUpdate = await _blogContext.Posts.FindAsync(post.Id);

            _blogContext.Posts.Update(post);
            await _blogContext.SaveChangesAsync();
        }

        /// <summary>
        /// Save a physical image file.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="fileName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private async Task<string> SaveFileAsync(byte[] bytes, string fileName, string suffix = null)
        {
            suffix = suffix ?? DateTime.UtcNow.Ticks.ToString();

            var ext = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            var relative = $"{name}_{suffix}{ext}";
            var absolute = Path.Combine(_folder, relative);
            var dir = Path.GetDirectoryName(absolute);

            Directory.CreateDirectory(dir);
            using (var writer = new FileStream(absolute, FileMode.CreateNew))
            {
                await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }

            return _fileLocationPrefix + relative;
        }

        private async Task<string> SaveCoverPhotoFileAsync(byte[] bytes, string fileName)
        {
            var ext = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            var relative = $"{name}{ext}";
            var absolute = Path.Combine(_folder, relative);
            var dir = Path.GetDirectoryName(absolute);

            Directory.CreateDirectory(dir);
            using (var writer = new FileStream(absolute, FileMode.CreateNew))
            {
                await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }

            return _fileLocationPrefix + relative;
        }

        /// <summary>
        /// Parse <see cref="Post.Content"/> for media files to save to drive.  Used with <see cref="SaveFileAsync"/>
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task<Post> SaveFilesToDiskAsync(Post post)
        {
            var imgRegex = new Regex("<img[^>].+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var base64Regex = new Regex("data:[^/]+/(?<ext>[a-z]+);base64,(?<base64>.+)", RegexOptions.IgnoreCase);

            foreach (Match match in imgRegex.Matches(post.Content))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<root>" + match.Value + "</root>");

                var img = doc.FirstChild.FirstChild;
                var srcNode = img.Attributes["src"];
                var fileNameNode = img.Attributes["data-filename"];

                // The HTML editor creates base64 DataURIs which we'll have to convert to image files on disk
                if (srcNode != null && fileNameNode != null)
                {
                    var base64Match = base64Regex.Match(srcNode.Value);
                    if (base64Match.Success)
                    {
                        byte[] bytes = Convert.FromBase64String(base64Match.Groups["base64"].Value);
                        srcNode.Value = await SaveFileAsync(bytes, fileNameNode.Value).ConfigureAwait(false);

                        img.Attributes.Remove(fileNameNode);
                        img.Attributes["alt"].Value = srcNode.Value;
                        
                        post.Content = post.Content.Replace(match.Value, img.OuterXml);
                    }
                }
            }

            return post;
        }


        /// <summary>
        /// Updates post media files <see cref="PostMedia"/>
        /// </summary>
        /// <param name="post"></param>
        /// <param name="oldMedia"></param>
        /// <returns></returns>
        public async Task<(List<PostMedia> newMedia, List<PostMedia> oldMedia)>UpdatePostMediaAsync(Post post, IReadOnlyCollection<PostMedia> oldMedia)
        {
            var deleteList = new List<PostMedia>();
            var imgRegex = new Regex("<img[^>].+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var newPostMedia = new List<PostMedia>();

            var oldMediaFiles = new List<PostMedia>();
            if (oldMedia != null)
            {
                oldMediaFiles.AddRange(oldMedia);
            }

            post.Media.Clear();

            foreach (Match match in imgRegex.Matches(post.Content))
            {
                var doc = new XmlDocument();
                doc.LoadXml("<root>" + match.Value + "</root>");

                var assetArray = doc.FirstChild.FirstChild.Attributes;
                var imageUrl = assetArray[0].InnerText;
                var imageFilename = assetArray[1].InnerText;


                if (string.IsNullOrEmpty(imageFilename) || string.IsNullOrEmpty(imageUrl)) continue;
                var media = new PostMedia
                {
                    PostId = post.Id,
                    FileName = imageFilename,
                    MedialUrl = imageUrl
                };
                //  set fileName
                media.FileName = SetPostMediaName(media);
                newPostMedia.Add(media);
            }

            //  has old photos? delete them 
            var anyOldPhotos = oldMediaFiles.Any();
            if (anyOldPhotos)
            {
                //deleteList.AddRange(from itm in oldMedia let itemFound = post.Media.Any(x => x.FileName == itm.FileName) where itemFound == false select itm);
                foreach (var itm in oldMediaFiles)
                {
                    // item in post.Media? true else add for delete(deleteList)
                    var found = post.Media.Any(x => x.FileName == itm.FileName); // ok
                    if (!found)
                    {
                        deleteList.Add(itm);// not found so add it to removal list for deletion
                    }
                }
            }

            if (!deleteList.Any()) return (newPostMedia, oldMediaFiles);
            {
                //  delete each item from serve
                foreach (var itm in deleteList)
                {
                    await DeletePostFileAsync(itm);
                }
            }

            return (newPostMedia, oldMediaFiles);
        }

        public async Task SavePostTagAsync(PostTag postTag)
        {
            _blogContext.PostTags.Add(postTag);
            await _blogContext.SaveChangesAsync();
        }

        public async Task EditPost(int id, Post post)
        {
            var postToUpdate = await _blogContext.Posts.FindAsync(post.Id);

            if (postToUpdate != null)
            {
                postToUpdate.LastModified = DateTime.UtcNow;
                _blogContext.Posts.Update(postToUpdate);
                await _blogContext.SaveChangesAsync();

            }

        }

        public async Task<string> SaveCoverPhotoAsync(string escapedTitle, IFormFile coverPhoto)
        {
            var newFileName = SetCoverPhotoNames(escapedTitle);
            using (var ms = new MemoryStream())
            {
                coverPhoto.CopyTo(ms);
                var fileBytes = ms.ToArray();
                newFileName = await SaveCoverPhotoFileAsync(fileBytes, newFileName).ConfigureAwait(false);
            }

            return newFileName;
        }

        //  Delete

        public async Task DeletePostTagAsync(int tagId)
        {
            var postTag = await _blogContext.PostTags
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tagId);

            if (postTag != null)
            {
                try
                {
                    _blogContext.PostTags.Remove(postTag);
                    await _blogContext.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                }
            }
        }

        private Task DeletePostFileAsync(PostMedia media)
        {
            var fileName = SetPostMediaName(media);
            var filePath = GetPostMediaFilePath(fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        /* NOT TESTED YET
         public async Task DeletePostAsync(int id)
        {
            var post = await _blogContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }*/


        //  Helper methods

        private string SetPostMediaName(PostMedia media)
        {
            var ext = Path.GetExtension(media.MedialUrl);
            var name = Path.GetFileNameWithoutExtension(media.MedialUrl);
            var fileName = name + ext;
            return fileName;
        }

        //  Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="escapedTitle"></param>
        /// <returns></returns>
        private string SetCoverPhotoNames(string escapedTitle)
        {
            var fileName = "cover-photo-" + escapedTitle + ".jpg";
            return fileName;
        }

        private string GetPostMediaFilePath(string fileName)
        {
            return Path.Combine(_filesFolder, fileName);
        }

        private bool IsAdmin()
        {
            return _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
        }

    }
}
