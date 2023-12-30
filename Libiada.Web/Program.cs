global using Libiada.Database;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc.Rendering;
using Libiada.Database.Helpers;
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
//builder.Configuration.AddEnvironmentVariables(prefix: "Libiada_");
builder.WebHost.UseKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = null;
        options.Limits.MaxRequestBufferSize = null;
        options.Limits.MaxRequestLineSize = 0x1000000;
        //options.Limits.MaxRequestHeadersTotalSize = 0x1000000;
        options.Limits.MaxResponseBufferSize = null;
        //options.Limits.Http2.MaxStreamsPerConnection = 1000; // 100 by default
        options.Limits.Http2.MaxFrameSize = 16777215; // 2^14 by default
        options.Limits.Http2.InitialConnectionWindowSize = 0x79000000; // 0x100000 by default
        options.Limits.Http2.InitialStreamWindowSize = 0x79000000; // 0xBB800 by default
    });

builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });
DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
// Add services to the container.
builder.Services.AddDbContext<LibiadaDatabaseEntities>(options => options.UseNpgsql());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<AspNetRole>()
                .AddEntityFrameworkStores<LibiadaDatabaseEntities>()
                .AddDefaultTokenProviders();

builder.Services.AddScoped(l => new LibiadaDatabaseEntities(new DbContextOptions<LibiadaDatabaseEntities>(), builder.Configuration));
builder.Services.AddSingleton<ILibiadaDatabaseEntitiesFactory, LibiadaDatabaseEntitiesFactory>();

builder.Services.AddSingleton<INcbiHelper, NcbiHelper>();

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
    options.ViewLocationFormats.Add("/Views/AngularTemplates/{0}.cshtml");
});

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR().AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; });

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
app.UseResponseCompression();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHub<TaskManagerHub>("/TaskManagerHub");

app.Run();
