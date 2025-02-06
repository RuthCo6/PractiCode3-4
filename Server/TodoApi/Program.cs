using Microsoft.EntityFrameworkCore;
using TodoApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToDo API",
        Version = "v1",
        Description = "A simple ToDo API to manage tasks."
    });
});

// הוספת MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql("server=localhost;user=root;password=Diti327770038!;database=ToDoDB",
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql")));

// הוספת Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// הוספת טיפול בשגיאות (developer exception page)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // רק בסביבת פיתוח
}
else
{
    app.UseExceptionHandler("/Home/Error"); // טיפול בשגיאות בסביבה שאינה פיתוח
}

app.UseCors("AllowSpecificOrigins");

// הגדרת Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    options.RoutePrefix = string.Empty;
});

// הגדרת המפות (routes)
app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    var tasks = await context.Items.ToListAsync();
    return tasks.Any() ? Results.Ok(tasks) : Results.NoContent();
})
.WithName("GetTasks");

app.MapPost("/tasks", async (ToDoDbContext context, Item newTask) =>
{
    context.Items.Add(newTask);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{newTask.Id}", newTask);
})
.WithName("CreateTask");

app.MapPut("/tasks/{id}", async (int id, ToDoDbContext context, Item updatedTask) =>
{
    var task = await context.Items.FindAsync(id);
    if (task == null) return Results.NotFound();

    task.Name = updatedTask.Name;
    task.IsComplete = updatedTask.IsComplete;
    await context.SaveChangesAsync();
    return Results.Ok(task);
})
.WithName("UpdateTask");

app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext context) =>
{
    var task = await context.Items.FindAsync(id);
    if (task == null) return Results.NotFound();

    context.Items.Remove(task);
    await context.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteTask");

app.MapGet("/", () => "Hello World!");

// הפעלת האפליקציה
app.Run();
