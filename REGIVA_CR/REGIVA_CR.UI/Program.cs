using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using REGIVA_CR.AB.AccesoADatos.Auth;
using REGIVA_CR.AB.AccesoADatos.Blog;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.LogicaDeNegocio.Blog;
using REGIVA_CR.AB.Services;
using REGIVA_CR.AD;
using REGIVA_CR.AD.Auth;
using REGIVA_CR.AD.Blog;
using REGIVA_CR.LN.Auth;
using REGIVA_CR.LN.Blog;
using REGIVA_CR.LN.Services;

Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

string connectionString =
    $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
    "Include Error Detail=true";

builder.Services.AddDbContext<RegivaContext>(options =>
    options.UseNpgsql(connectionString));

builder.Configuration["EmailSettings:Host"] =
    Environment.GetEnvironmentVariable("EMAIL_HOST");

builder.Configuration["EmailSettings:Port"] =
    Environment.GetEnvironmentVariable("EMAIL_PORT");

builder.Configuration["EmailSettings:Email"] =
    Environment.GetEnvironmentVariable("EMAIL_USER");

builder.Configuration["EmailSettings:Password"] =
    Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    
builder.Services.AddScoped<IAccountAD, AccountAD>();
builder.Services.AddScoped<IAccountLN, AccountLN>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDocsService, DocsService>();
builder.Services.AddScoped<IBlogAD, BlogAD>(); 
builder.Services.AddScoped<IBlogLN, BlogLN>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".REGIVA.Session";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";

        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "REGIVA_Auth_Cookie";
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddControllersWithViews()
        .AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
