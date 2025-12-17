using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Repositories;
using GuildManagement.Services;
using GuildManagement.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddControllers();
builder.Services.AddDbContext<GuildManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<GuildManagementContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<DataLoader>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
using (var scope = app.Services.CreateScope())
{
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "USER", "ADMIN", "MODERATOR" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        if (!userManager.Users.Any())
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@library.com",
                EmailConfirmed = true,
                FirstName = "Артас",
                LastName = "Менетил"
            };
            
            var adminResult = await userManager.CreateAsync(admin, "password123");
            
            if (adminResult.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, new[] { "ADMIN", "USER" });
            }
            
            var user = new ApplicationUser
            {
                UserName = "jaina",
                Email = "jaina@library.com", 
                EmailConfirmed = true,
                FirstName = "Джайна",
                LastName = "Праудмур"
            };
            
            var userResult = await userManager.CreateAsync(user, "password123");
            
            if (userResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "USER");
            }
            
            var user2 = new ApplicationUser
            {
                UserName = "valera",
                Email = "valera@library.com",
                EmailConfirmed = true,
                FirstName = "Валера",
                LastName = "Сангуинар"
            };
            
            var user2Result = await userManager.CreateAsync(user2, "password123");
            
            if (user2Result.Succeeded)
            {
                await userManager.AddToRoleAsync(user2, "USER");
            }
        }
        else
        {
            Console.WriteLine("уже существую");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
    }
}

// 8. Загрузка данных гильдии
// using (var scope = app.Services.CreateScope())
// {
//     try
//     {
//         var dataLoader = scope.ServiceProvider.GetRequiredService<DataLoader>();
//         await dataLoader.LoadTestData();
//         Console.WriteLine("данные гильдии загружены");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($" {ex.Message}");
//     }
// }

// 9. Маршрутизация
app.MapControllers();
app.MapGet("/", () => "Guild Management API работает с аутентификацией!");

app.Run();