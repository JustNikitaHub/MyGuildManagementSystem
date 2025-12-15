using Microsoft.EntityFrameworkCore;
using GuildManagement.Data;
using GuildManagement.Entities;
using GuildManagement.Interfaces;
using GuildManagement.Repositories;
using GuildManagement.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<GuildManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<DataLoader>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GuildManagementContext>();
    context.Database.EnsureCreated();
}

using (var scope = app.Services.CreateScope())
{
    var dataLoader = scope.ServiceProvider.GetRequiredService<DataLoader>();
    await dataLoader.LoadTestData();
}

app.MapGet("/", () => "Guild Management API is running with database!");

app.MapPost("/api/init", async (DataLoader dataLoader) =>
{
    await dataLoader.LoadTestData();
    return Results.Ok("Test data reinitialized");
});

app.MapControllers();

app.Run();