global using Libiada.Database;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc.Rendering;
using LibiadaWeb.Data;
using LibiadaWeb.Helpers;
using LibiadaWeb.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString))
                .AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser<int>>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<LibiadaDatabaseEntities>();
builder.Services.AddSingleton<ILibiadaDatabaseEntitiesFactory, LibiadaDatabaseEntitiesFactory>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
builder.Services.AddTransient<IViewDataHelper, ViewDataHelper>();
builder.Services.AddSingleton<ITaskManagerHubFactory, TaskManagerHubFactory>();
builder.Services.AddSingleton<ITaskManager, TaskManager>();

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
