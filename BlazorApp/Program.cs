using BlazorApp.Auth;
using BlazorApp.Components;
using BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

ApiConfig apiConfig = new(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7199");
builder.Services.AddSingleton(apiConfig);
builder.Services.AddScoped(sp => new HttpClient
    {
        BaseAddress = new Uri(apiConfig.BaseUrl)
    }
);

builder.Services.AddScoped<IUserService, HttpUserService>();
builder.Services.AddScoped<IPostService, HttpPostService>();
builder.Services.AddScoped<ICommentService, HttpCommentService>();
builder.Services.AddScoped<ITagService, HttpTagService>();
builder.Services.AddScoped<IProfileService, HttpProfileService>();
builder.Services.AddScoped<IUploadService, HttpUploadService>();
builder.Services.AddScoped<INotificationService, HttpNotificationService>();
builder.Services.AddScoped<IMessageService, HttpMessageService>();
builder.Services.AddScoped<ISearchService, HttpSearchService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<ProfileCardCache>();
builder.Services.AddScoped<ShellThemeService>();

builder.Services.AddScoped<AuthenticationStateProvider, SimpleAuthProvider>();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(ClaimsExtensions.ModeratorPolicy, policy => policy.RequireClaim("IsModerator", "True"));
});
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, PassThroughAuthorizationResultHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
