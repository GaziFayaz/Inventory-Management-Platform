using Inventory_Management_Platform.Common.Authorization;
using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Data;
using Inventory_Management_Platform.Data.Seeder;
using Inventory_Management_Platform.Features.Admin;
using Inventory_Management_Platform.Features.Auth;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = new List<string>();
        var frontendUrl = builder.Configuration["FrontendUrl"];

        if (!string.IsNullOrWhiteSpace(frontendUrl))
            origins.Add(frontendUrl);

        if (builder.Environment.IsDevelopment())
        {
            origins.Add("http://localhost:5173");
            origins.Add("https://localhost:5173");
        }

        if (origins.Count == 0)
            origins.Add("http://localhost:3000");

        policy
            .WithOrigins(origins.Distinct().ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Return JSON 401/403 for API clients instead of browser redirects.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/auth/callback/google";
    });
    // .AddFacebook(options =>
    // {
    //     options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    //     options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
    //     options.CallbackPath = "/auth/callback/facebook";
    // });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p
        .RequireRole("Admin"))
    .AddPolicy("Authenticated", p => p
        .RequireAuthenticatedUser()
        .AddRequirements(new NotBlockedRequirement()))
    .AddPolicy("OwnerOrAdmin", p => p
        .RequireAuthenticatedUser()
        .AddRequirements(
            new NotBlockedRequirement(),
            new InventoryOwnerOrAdminRequirement()))
    .AddPolicy("InventoryWrite", p => p
        .RequireAuthenticatedUser()
        .AddRequirements(
            new NotBlockedRequirement(),
            new InventoryWriteRequirement()));

builder.Services.AddScoped<IAuthorizationHandler, NotBlockedHandler>();
builder.Services.AddScoped<IAuthorizationHandler, InventoryOwnerOrAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, InventoryWriteHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Seed admin role and user on every startup (idempotent — skips if already exists).
using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();