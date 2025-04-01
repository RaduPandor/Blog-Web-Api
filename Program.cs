using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 41))
    )
);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blogger API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    });
}

app.MapControllers();
app.Run();
