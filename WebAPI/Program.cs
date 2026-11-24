using EfcRepositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using WebAPI.Middleware;
using WebAPI.Seeding;
using WebAPI.Services;
// System.AppContext would clash with our database context
using AppContext = EfcRepositories.AppContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// The SQLite file lives in the EfcRepositories folder, next to the migrations.
// The path in appsettings.json is relative to the WebAPI project folder, so it works on every machine.
string databasePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["DatabasePath"] ?? "../EfcRepositories/app.db"));
builder.Services.AddDbContext<AppContext>(options => options.UseSqlite($"Data Source={databasePath}"));

builder.Services.AddScoped<ITagRepository, EfcTagRepository>();
builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();
builder.Services.AddScoped<IProfileRepository, EfcProfileRepository>();
builder.Services.AddScoped<INotificationRepository, EfcNotificationRepository>();
builder.Services.AddScoped<IMessageRepository, EfcMessageRepository>();

builder.Services.AddScoped<PostDtoBuilder>();
builder.Services.AddScoped<ProfileCardBuilder>();
builder.Services.AddScoped<Notifier>();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

var app = builder.Build();

// creates the database on first run and applies any new migrations (same as "dotnet ef database update")
using (IServiceScope scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppContext>().Database.MigrateAsync();
}

await DemoDataSeeder.SeedAsync(app.Services);

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// serves uploaded images (wwwroot/uploads) and demo artwork (wwwroot/demo)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
