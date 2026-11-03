using FileRepositories;
using RepositoryContracts;
using WebAPI.Middleware;
using WebAPI.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ITagRepository, TagFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();
builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileFileRepository>();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

var app = builder.Build();

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
