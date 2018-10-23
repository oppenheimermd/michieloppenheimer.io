using Microsoft.EntityFrameworkCore;
using Blog.Models;

// ReSharper disable once CheckNamespace
namespace Blog.Data
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        {
        }

        /// <summary>
        /// When the database is created, EF creates tables that have names the same as the DbSet
        /// property names. Property names for collections are typically plural.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().ToTable("Post");
            modelBuilder.Entity<Comment>().ToTable("Comment");
            modelBuilder.Entity<PostMedia>().ToTable("PostMedia");
            modelBuilder.Entity<PostTag>().ToTable("PostTag");
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostMedia> PostMedias { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
    }
}
