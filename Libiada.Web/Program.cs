global using Libiada.Database;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc.Rendering;

using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);
DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("LibiadaDatabaseEntities") ?? throw new InvalidOperationException("Connection string 'LibiadaDatabaseEntities' not found.");
builder.Services.AddDbContext<LibiadaDatabaseEntities>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<AspNetRole>()
                .AddEntityFrameworkStores<LibiadaDatabaseEntities>()
                .AddDefaultTokenProviders();

builder.Services.AddScoped(l => new LibiadaDatabaseEntities(new DbContextOptions<LibiadaDatabaseEntities>()));
builder.Services.AddSingleton<ILibiadaDatabaseEntitiesFactory, LibiadaDatabaseEntitiesFactory>();

builder.Services.AddSingleton<Cache>();

builder.Services.AddSingleton<IAccordanceCharacteristicRepository, AccordanceCharacteristicRepository>();
builder.Services.AddSingleton<IBinaryCharacteristicRepository, BinaryCharacteristicRepository>();
builder.Services.AddSingleton<ICongenericCharacteristicRepository, CongenericCharacteristicRepository>();
builder.Services.AddSingleton<IFullCharacteristicRepository, FullCharacteristicRepository>();

builder.Services.AddTransient<ICommonSequenceRepository, CommonSequenceRepository>();

builder.Services.AddScoped<ISequencesCharacteristicsCalculator, SequencesCharacteristicsCalculator>();
builder.Services.AddScoped<ISubsequencesCharacteristicsCalculator, SubsequencesCharacteristicsCalculator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

builder.Services.AddTransient<IViewDataHelper, ViewDataHelper>();
builder.Services.AddSingleton<ITaskManagerHubFactory, TaskManagerHubFactory>();
builder.Services.AddSingleton<ITaskManager, TaskManager>();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    // {0} - Action Name
    // {1} - Controller Name
    // {2} - Area Name
    options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Sequences/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Calculators/{1}/{0}.cshtml");
    // options.ViewLocationFormats.Add("/Views/AngularTemplates/{0}.cshtml");


});

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

app.MapHub<TaskManagerHub>("/TaskManagerHub");

app.Run();