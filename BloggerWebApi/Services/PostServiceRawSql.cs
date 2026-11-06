using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace BloggerWebApi.Services
{
    // Same behaviour as PostService, but with hand-written SQL instead of LINQ.
    // To use it, register it in Program.cs instead of PostService:
    // builder.Services.AddScoped<IPostService, PostServiceRawSql>();
    public class PostServiceRawSql(AppDbContext context, IUserService userService) : IPostService
    {
        private readonly AppDbContext context = context;
        private readonly IUserService userService = userService;

        public async Task<IEnumerable<PostPreviewDto>> GetAllAsync()
        {
            const string sql = @"
                SELECT p.Id,
                       p.Title,
                       COALESCE(u.DisplayName, p.Author) AS Author,
                       CASE WHEN CHAR_LENGTH(p.Content) > 150
                            THEN CONCAT(SUBSTRING(p.Content, 1, 150), '...')
                            ELSE p.Content
                       END AS ContentPreview,
                       p.CreatedDate,
                       p.LastModifiedDate
                FROM Posts p
                LEFT JOIN AspNetUsers u ON u.Id = p.Author
                ORDER BY p.CreatedDate DESC";

            return await ExecuteAsync(sql, async command =>
            {
                var posts = new List<PostPreviewDto>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    posts.Add(new PostPreviewDto
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        ContentPreview = reader.GetString(3),
                        CreatedDate = reader.GetDateTime(4),
                        LastModifiedDate = reader.GetDateTime(5)
                    });
                }

                return posts;
            });
        }

        public async Task<FullPostDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.Id, p.Title, p.Content, u.DisplayName, p.Author, p.CreatedDate, p.LastModifiedDate
                FROM Posts p
                INNER JOIN AspNetUsers u ON u.Id = p.Author
                WHERE p.Id = @Id";

            return await ExecuteAsync(sql, async command =>
            {
                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return new FullPostDto
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    Author = reader.GetString(3),
                    AuthorId = reader.GetString(4),
                    CreatedDate = reader.GetDateTime(5),
                    LastModifiedDate = reader.GetDateTime(6)
                };
            }, ("@Id", id));
        }

        public async Task<Post> CreateAsync(Post post, string userId)
        {
            const string sql = @"
                INSERT INTO Posts (Title, Author, Content, CreatedDate, LastModifiedDate)
                VALUES (@Title, @Author, @Content, @CreatedDate, @LastModifiedDate);
                SELECT LAST_INSERT_ID();";

            var now = DateTime.UtcNow;
            post.Author = userId;
            post.CreatedDate = now;
            post.LastModifiedDate = now;

            post.Id = await ExecuteAsync(sql, async command =>
                Convert.ToInt32(await command.ExecuteScalarAsync()),
                ("@Title", post.Title),
                ("@Author", post.Author),
                ("@Content", post.Content),
                ("@CreatedDate", post.CreatedDate),
                ("@LastModifiedDate", post.LastModifiedDate));

            return post;
        }

        public async Task<Post?> UpdateAsync(int id, Post updatedPost)
        {
            const string updateSql = @"
                UPDATE Posts
                SET Title = @Title, Content = @Content, LastModifiedDate = @LastModifiedDate
                WHERE Id = @Id";

            var rowsAffected = await ExecuteAsync(updateSql, command => command.ExecuteNonQueryAsync(),
                ("@Title", updatedPost.Title),
                ("@Content", updatedPost.Content),
                ("@LastModifiedDate", DateTime.UtcNow),
                ("@Id", id));

            if (rowsAffected == 0)
            {
                return null;
            }

            const string selectSql = @"
                SELECT Id, Title, Author, Content, CreatedDate, LastModifiedDate
                FROM Posts
                WHERE Id = @Id";

            return await ExecuteAsync(selectSql, async command =>
            {
                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    Content = reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4),
                    LastModifiedDate = reader.GetDateTime(5)
                };
            }, ("@Id", id));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Permission is checked in PostsController via IsOwnerOrAdminAsync before this runs.
            const string sql = "DELETE FROM Posts WHERE Id = @Id";

            var rowsAffected = await ExecuteAsync(sql, command => command.ExecuteNonQueryAsync(), ("@Id", id));
            return rowsAffected > 0;
        }

        public async Task<bool> IsOwnerOrAdminAsync(int postId, string userId)
        {
            const string sql = "SELECT Author FROM Posts WHERE Id = @Id";

            var authorId = await ExecuteAsync(sql, async command =>
                await command.ExecuteScalarAsync() as string,
                ("@Id", postId));

            if (authorId == null)
            {
                return false;
            }

            return authorId == userId || await userService.IsUserAdminAsync(userId);
        }

        // Runs one command on the DbContext's own connection.
        // EF owns that connection, so we open and close it through EF and never dispose it ourselves.
        private async Task<T> ExecuteAsync<T>(string sql, Func<DbCommand, Task<T>> run, params (string Name, object? Value)[] parameters)
        {
            await context.Database.OpenConnectionAsync();
            try
            {
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;

                foreach (var (name, value) in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = name;
                    parameter.Value = value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }

                return await run(command);
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
