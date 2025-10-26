using GuildManagement.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var members = new List<Member>
{
    new Member("Test Warrior", 25, MemberClass.Warrior) { Id = 1 },
    new Member("Test Mage", 30, MemberClass.Mage) { Id = 2 }
};
var events = new List<Event>
{
    new Event("Test Raid", EventType.Raid, DateTime.Now, 1) { Id = 1 },
    new Event("Test Quest", EventType.Social, DateTime.Now.AddDays(1), 2) { Id = 2 }
};
app.MapGet("/api/members", () => members);
app.MapGet("/api/events", () => events);
app.MapGet("/api/members/{id}", (int id) =>
{
    var member = members.FirstOrDefault(m => m.Id == id);
    return member != null ? Results.Ok(member) : Results.NotFound();
});
app.MapGet("/api/events/{id}", (int id) =>
{
    var eventItem = events.FirstOrDefault(e => e.Id == id);
    return eventItem != null ? Results.Ok(eventItem) : Results.NotFound();
});
app.MapPost("/api/init", () =>
{
    members.Clear();
    events.Clear();
    
    var member = new Member("Test Warrior", 25, MemberClass.Warrior) { Id = 1 };
    var eventItem = new Event("Test Raid", EventType.Raid, DateTime.Now, 1) { Id = 1 };
    
    members.Add(member);
    events.Add(eventItem);
    
    return Results.Ok(new
    {
        Message = "Test data initialized",
        MemberId = member.Id,
        EventId = eventItem.Id
    });
});

app.MapGet("/", () => "Guild Management API is running!");

app.Run();