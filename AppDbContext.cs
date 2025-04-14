using BloggerWebApi.Dto;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts { get; set; }

    public DbSet<PostPreviewDto> PostPreviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PostPreviewDto>().HasNoKey();
    }
}
