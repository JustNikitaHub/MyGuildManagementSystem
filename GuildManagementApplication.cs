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
});
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
    Console.WriteLine("таблицы созданы!");
}


using (var scope = app.Services.CreateScope())
{
    var dataLoader = scope.ServiceProvider.GetRequiredService<DataLoader>();
    await dataLoader.LoadTestData();
}


app.MapGet("/", () => "Guild Management API is running with database!");


app.MapGet("/api/members", async (IMemberService service) => 
    await service.GetAllMembers());

app.MapGet("/api/members/{id}", async (int id, IMemberService service) =>
{
    var member = await service.GetMemberById(id);
    return member != null ? Results.Ok(member) : Results.NotFound();
});

app.MapPost("/api/members", async (Member member, IMemberService service) =>
{
    try
    {
        var createdMember = await service.CreateMember(member);
        return Results.Created($"/api/members/{createdMember.Id}", createdMember);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/api/events", async (IEventService service) => 
    await service.GetAllEvents());

app.MapGet("/api/events/{id}", async (int id, IEventService service) =>
{
    var eventItem = await service.GetEventById(id);
    return eventItem != null ? Results.Ok(eventItem) : Results.NotFound();
});

app.MapPost("/api/events", async (Event eventItem, IEventService service) =>
{
    try
    {
        var createdEvent = await service.CreateEvent(eventItem);
        return Results.Created($"/api/events/{createdEvent.Id}", createdEvent);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/api/events/upcoming", async (IEventService service) =>
    await service.GetUpcomingEvents());

app.MapGet("/api/members/class/{memberClass}", async (MemberClass memberClass, IMemberService service) =>
    await service.GetMembersByClass(memberClass));

app.MapPut("/api/members/{id}/level/{newLevel}", async (int id, int newLevel, IMemberService service) =>
{
    var success = await service.ChangeMemberLevel(id, newLevel);
    return success ? Results.Ok($"Level changed to {newLevel}") : Results.NotFound();
});


app.MapPost("/api/init", async (DataLoader dataLoader) =>
{
    await dataLoader.LoadTestData();
    return Results.Ok("Test data reinitialized");
});

app.Run();