using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        context.Database.Migrate();

        if (!context.Posts.Any())
        {
            context.Posts.AddRange(
                new Post { Title = "Hello World", Content = "This is the first blog post!" },
                new Post { Title = "Docker is awesome", Content = "Running fullstack apps with containers is easy." }
            );

            context.SaveChanges();
        }
    }
}
