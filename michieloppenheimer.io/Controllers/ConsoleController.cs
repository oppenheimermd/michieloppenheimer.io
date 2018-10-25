using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Data;
using Blog.Models;
using Blog.Models.Helpers;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Web.Controllers
{
    public class ConsoleController : Controller
    {
        private readonly IBlogService _blog;
        private readonly BlogContext _blogContext;
        private readonly int _excerptMaxLength;

        public ConsoleController(IBlogService blog, BlogContext blogContext)
        {
            _blog = blog;
            _blogContext = blogContext;
            _excerptMaxLength = 280;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var posts = await _blog.GetAllPostAsync();

            var pager = new PaginatedList(posts.Count(), page);
            var pagedViewModel = new PagedViewModel<Post>(posts)
            {
                Items = posts.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };

            return View(pagedViewModel);
        }

        /*[HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id.HasValue)
            {
                var post = await _blog.GetPostByIdAsync(id.Value);
                if (post == null) return NotFound();
                return View(post);
            }

            return View(new Post());
        }*/

        [HttpGet]
        public IActionResult CreatePost()
        {
            var post = new Post();
            return View(post);

        }

        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CreatePost(Post post)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", post);
            }

            var newPost = new Post();
            string formTags = Request.Form["categories"];
            newPost.Title = post.Title.Trim();
            if (!string.IsNullOrWhiteSpace(post.Slug))
            {
                newPost.Slug = PostHelpers.CreateSlug(post.Slug);
            }

            newPost.PubDate = DateTime.UtcNow;
            newPost.IsPublished = post.IsPublished;
            newPost.Content = post.Content.Trim();
            newPost.Excerpt = PostHelpers.ShortenAndFormatText(post.Excerpt.Trim(), _excerptMaxLength);
            newPost.LastModified = DateTime.UtcNow;
            //  Do we have a cover photo?
            //  Check the file length and don't bother attempting to read it if the file contains no content.
            if (HttpContext.Request.Form.Files.Count != 0)
            {
                var newCoverPhoto = HttpContext.Request.Form.Files[0];
                //  yes
                if (newCoverPhoto.Length > 0)
                {
                    newPost.PostCoverPhoto = await _blog.SaveCoverPhotoAsync(newPost.Slug, newCoverPhoto);
                }
            }


            //  1.  We need to save post to db first to get id
            await _blog.SavePostAsync(newPost);
            //  2.  Save files to disk
            await _blog.SaveFilesToDiskAsync(newPost);
            //  3   Update any changes from SaveFileToDiskAsync()
            await _blog.UpdatePostAsync(newPost);



            //  Update post media
            var oldMedia = await _blogContext.PostMedias
                .AsNoTracking()
                .Where(x => x.PostId == newPost.Id)
                .ToListAsync();

            var postMedia = await _blog.UpdatePostMediaAsync(newPost, oldMedia);
            //  new media?
            if (postMedia.newMedia.Any())
            {
                foreach (var media in postMedia.newMedia)
                {
                    await _blogContext.PostMedias.AddAsync(media);
                    await _blogContext.SaveChangesAsync();
                }
            }
            //  oldMedia
            if (postMedia.oldMedia.Any())
            {
                foreach (var media in postMedia.oldMedia)
                {
                    //await _blogContext.PostMedias.r(media)
                    var old = await _blogContext.PostMedias
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == media.Id);
                    if (old == null) continue;
                    _blogContext.PostMedias.Remove(old);
                    await _blogContext.SaveChangesAsync();
                }
            }

            //  Tags
            //  has tags, add them
            if (!string.IsNullOrEmpty(formTags))
            {
                var tags = new List<PostTag>();
                if (!string.IsNullOrEmpty(formTags))
                {
                    tags = await formTags.Split(',').ToList().ProcessTagsAsync(newPost.Id);
                    if (tags.Any())
                    {
                        foreach (var t in tags)
                        {
                            await _blog.SavePostTagAsync(t);
                        }
                    }
                }
            }

            

            var redirectLink = $"/Console/BlogPreview/{newPost.Id}";
            return Redirect(redirectLink);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();
            var post = await _blog.GetPostByIdAsync(id.Value);
            if (post == null) return NotFound();
            return View(post);

        }

        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(Post post)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", post);
            }

            var existing = await _blog.GetPostByIdAsync(post.Id);
            if(existing == null)
                return NotFound();

            string formTags = Request.Form["categories"];

            existing.IsPublished = post.IsPublished;
            existing.Content = post.Content.Trim();
            existing.Excerpt = PostHelpers.ShortenAndFormatText(post.Excerpt.Trim(), _excerptMaxLength);
            existing.LastModified = DateTime.UtcNow;

            var oldMedia = await _blogContext.PostMedias
                .AsNoTracking()
                .Where(x => x.PostId == existing.Id)
                .ToListAsync();

            //  1.  Save Files to disk
            await _blog.SaveFilesToDiskAsync(existing);
            await _blog.UpdatePostAsync(existing);
            //  Update post media
            var postMedia = await _blog.UpdatePostMediaAsync(existing, oldMedia);
            //  new media?
            if (postMedia.newMedia.Any())
            {
                foreach (var media in postMedia.newMedia)
                {
                    await _blogContext.PostMedias.AddAsync(media);
                    await _blogContext.SaveChangesAsync();
                }
            }
            //  oldMedia
            if (postMedia.oldMedia.Any())
            {
                foreach (var media in postMedia.oldMedia)
                {
                    // remove from db
                    var old = await _blogContext.PostMedias
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == media.Id);
                    if (old == null) continue;
                    _blogContext.PostMedias.Remove(old);
                    await _blogContext.SaveChangesAsync();
                    //  Remove from file store
                    //await _blog.DeletePostFileAsync()
                }
            }

            var newTags = await formTags.Split(',').ToList().ProcessTagsAsync(existing.Id);
            var oldTags = await _blog.GetPostTags(post.Id);
            // tags equal? take no action
            if (!newTags.SequenceEqual(oldTags, new DefaultPostTagComparer()))
            {
                // delete old tags
                foreach (var t in oldTags)
                {
                    await _blog.DeletePostTagAsync(t.Id);
                }

                //  add new tags
                if (newTags.Any())
                {
                    foreach (var t in newTags)
                    {
                        await _blog.SavePostTagAsync(t);
                    }
                }
            }


            var redirectLink = $"/Console/BlogPreview/{post.Id}";
            return Redirect(redirectLink);
        }

        [HttpGet]
        public async Task<IActionResult> BlogPreview(int? id)
        {
            if (!id.HasValue) return NotFound();

            var post = await _blog.GetPostByIdAsync(id.Value);
            if (post != null)
            {
                return View(post);
            }

            return NotFound();

        }

    }
}
