using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace BloggerWebApi.Services
{
    public class PostServiceRawSql(AppDbContext context) : IPostService
    {
        private readonly AppDbContext context = context;

        public async Task<IEnumerable<PostPreviewDto>> GetAllAsync()
        {
            var sql = "SELECT Id, Title, Author, SUBSTRING(Content, 1, 20) AS ContentPreview, CreatedDate, LastModifiedDate FROM Posts ORDER BY CreatedDate DESC";
            await using var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();

            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            var list = new List<PostPreviewDto>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new PostPreviewDto
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    ContentPreview = reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4),
                    LastModifiedDate = reader.GetDateTime(5)
                });
            }

            return list;
        }


        public async Task<Post?> GetByIdAsync(int id)
        {
            var sql = "SELECT Id, Title, Author, Content, CreatedDate, LastModifiedDate FROM Posts WHERE Id = @id";
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            var idParam = new SqlParameter("@id", id);
            command.Parameters.Add(idParam);
            await context.Database.OpenConnectionAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Post
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Content = reader.GetString(3),
                        CreatedDate = reader.GetDateTime(4),
                        LastModifiedDate = reader.GetDateTime(5)
                    };
                }
            }

            return null;
        }

        public async Task<Post> CreateAsync(Post post)
        {
            var sql = "INSERT INTO Posts (Title, Author, Content, CreatedDate, LastModifiedDate) VALUES (@Title, @Author, @Content, @CreatedDate, @LastModifiedDate)";
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new SqlParameter("@Title", post.Title));
            command.Parameters.Add(new SqlParameter("@Author", post.Author));
            command.Parameters.Add(new SqlParameter("@Content", post.Content));
            command.Parameters.Add(new SqlParameter("@CreatedDate", post.CreatedDate));
            command.Parameters.Add(new SqlParameter("@LastModifiedDate", post.LastModifiedDate));
            await context.Database.OpenConnectionAsync();
            await command.ExecuteNonQueryAsync();
            return post;
        }

        public async Task<Post?> UpdateAsync(int id, Post updatedPost)
        {
            var query = "UPDATE Posts SET Title = @Title, Author = @Author, Content = @Content, LastModifiedDate = @LastModifiedDate WHERE Id = @Id";

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(query,
                new MySqlParameter("@Title", updatedPost.Title),
                new MySqlParameter("@Author", updatedPost.Author),
                new MySqlParameter("@Content", updatedPost.Content),
                new MySqlParameter("@LastModifiedDate", DateTime.UtcNow),
                new MySqlParameter("@Id", id));

            if (rowsAffected == 0)
            {
                return null;
            }

            var updatedPostFromDb = await context.Posts
                .FromSqlRaw("SELECT * FROM Posts WHERE Id = @Id", new MySqlParameter("@Id", id))
                .FirstOrDefaultAsync();

            return updatedPostFromDb;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM Posts WHERE Id = @Id";
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            var idParam = new SqlParameter("@Id", id);
            command.Parameters.Add(idParam);
            await context.Database.OpenConnectionAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
