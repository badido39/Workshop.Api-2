using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


// Port forcé : 5000 partout (local, conteneur), écoute sur toutes les interfaces (0.0.0.0)
// Prioritaire sur launchSettings.json, ASPNETCORE_URLS et ASPNETCORE_HTTP_PORTS
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5000));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Checkpoint 02 : toujours en mémoire, mais l'API est maintenant conteneurisable
var todos = new ConcurrentDictionary<int, Todo>();
var lastId = 0;

foreach (var title in new[] { "Installer Docker Desktop", "Lancer le workshop", "Ne pas paniquer" })
{
    var id = Interlocked.Increment(ref lastId);
    todos[id] = new Todo(id, title, false, DateTime.Now);
}

app.MapGet("/", () => Results.Redirect("/scalar"));

app.MapGet("/info", () => new
{
    machine = Environment.MachineName,
    os = RuntimeInformation.OSDescription,
    runtime = RuntimeInformation.FrameworkDescription,
    inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true",
    now = DateTime.Now,
    timeZone = TimeZoneInfo.Local.Id
});

app.MapGet("/todos", () => todos.Values.OrderBy(t => t.Id));

app.MapPost("/todos", (NewTodo input) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { error = "Le titre est obligatoire." });

    var id = Interlocked.Increment(ref lastId);
    var todo = new Todo(id, input.Title.Trim(), false, DateTime.Now);
    todos[id] = todo;
    return Results.Created($"/todos/{id}", todo);
});

app.MapPatch("/todos/{id:int}/toggle", (int id) =>
{
    if (!todos.TryGetValue(id, out var todo))
        return Results.NotFound();

    var updated = todo with { Done = !todo.Done };
    todos[id] = updated;
    return Results.Ok(updated);
});

app.MapDelete("/todos/{id:int}", (int id) =>
    todos.TryRemove(id, out _) ? Results.NoContent() : Results.NotFound());

app.Run();

record Todo(int Id, string Title, bool Done, DateTime CreatedAt);

record NewTodo(string Title);
