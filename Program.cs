using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAÇÃO DOS SERVIÇOS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Configuração do Banco de Dados PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql("Host=localhost;Database=todo_db;Username=postgres;Password=root"));

var app = builder.Build();

// 2. CONFIGURAÇÃO DO PIPELINE (Middleware)
app.UseCors("PermitirTudo");

// --- ROTAS DA API ---

// Listar todas as tarefas
app.MapGet("/tarefas", async (AppDbContext db) =>
    await db.Tarefas.ToListAsync());

// Criar nova tarefa
app.MapPost("/tarefas", async (Tarefa novaTarefa, AppDbContext db) =>
{
    db.Tarefas.Add(novaTarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{novaTarefa.Id}", novaTarefa);
});

// Atualizar tarefa (Título, Concluída ou Em Andamento)
app.MapPut("/tarefas/{id}", async (int id, Tarefa tarefaAlterada, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa == null) return Results.NotFound();

    tarefa.Titulo = tarefaAlterada.Titulo;
    tarefa.Concluida = tarefaAlterada.Concluida;
    tarefa.EmAndamento = tarefaAlterada.EmAndamento;

    await db.SaveChangesAsync();
    return Results.Ok();
});

// Excluir tarefa
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound();

    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// --- MIGRATIONS AUTOMÁTICAS (Executa ao iniciar) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao rodar as migrations.");
    }
}

app.Run(); // Inicia o servidor

// --- MODELOS E CONTEXTO ---

public class Tarefa
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public bool Concluida { get; set; }
    public bool EmAndamento { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Tarefa> Tarefas { get; set; }
}