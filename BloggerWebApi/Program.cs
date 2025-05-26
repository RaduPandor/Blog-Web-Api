using BloggerWebApi.Entities;
using BloggerWebApi.Interfaces;
using BloggerWebApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins?.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 42)),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 1,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AdminUser>(
    builder.Configuration.GetSection("AdminUser"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var maxRetries = 2;
    var delay = TimeSpan.FromSeconds(10);
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            break;
        }
        catch (MySqlConnector.MySqlException ex)
        {
            if (i == maxRetries - 1)
            {
                throw;
            }
            Console.WriteLine($"Failed to connect to MySQL: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");
            await Task.Delay(delay);
        }
    }

    await SeedAdminUser(scope.ServiceProvider);
}

async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var adminSection = config.GetSection("AdminUser");
    var adminUserSettings = adminSection.Get<AdminUser>();

    if (adminUserSettings == null ||
        string.IsNullOrWhiteSpace(adminUserSettings.UserName) ||
        string.IsNullOrWhiteSpace(adminUserSettings.Email) ||
        string.IsNullOrWhiteSpace(adminUserSettings.Password))
    {
        throw new Exception("Admin user settings are incomplete.");
    }

    var roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminUser = await userManager.FindByEmailAsync(adminUserSettings.Email);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminUserSettings.UserName,
            Email = adminUserSettings.Email,
            EmailConfirmed = true,
            DisplayName = adminUserSettings.UserName
        };

        var result = await userManager.CreateAsync(adminUser, adminUserSettings.Password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();