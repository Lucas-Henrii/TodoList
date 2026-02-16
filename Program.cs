using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql("Host=localhost;Database=todo_db;Username=postgres;Password=root"));

var app = builder.Build();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseDefaultFiles(); // Procura automaticamente por index.html
app.UseStaticFiles();  // Permite carregar CSS, JS e Imagens

//GET
app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

//POST
app.MapPost("/tarefas", async (Tarefa novaTarefa, AppDbContext db) =>
{
    db.Tarefas.Add(novaTarefa);
    await db.SaveChangesAsync();
    return Results.Created("/tarefas/{novaTarefa.Id}", novaTarefa);
});

//ATUALIZAR
app.MapPut("/tarefas/{id}", (int id, Tarefa tarefaAlterada, AppDbContext db) =>
{
    var tarefa = db.Tarefas.Find(id);
    if (tarefa == null) return Results.NotFound();

    // Atualiza os campos
    tarefa.Titulo = tarefaAlterada.Titulo;
    tarefa.Concluida = tarefaAlterada.Concluida;

    db.SaveChanges();
    return Results.Ok();
});

//DELETE
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound();

    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Esse bloco garante que o banco de dados seja criado/atualizado no Render
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>(); // Troque 'SeuDbContext' pelo nome do seu Context
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao rodar as migrations.");
    }
}

app.Run();

public class Tarefa
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public bool Concluida { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tarefa> Tarefas { get; set; }
}