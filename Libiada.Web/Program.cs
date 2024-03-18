using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;

using System.Data.Common;

using Libiada.Database.Helpers;
using Libiada.Database.Models.Calculators;
using Libiada.Database.Models.Repositories.Catalogs;
using Libiada.Database.Models.Repositories.Sequences;

using Libiada.Web.Helpers;
using Libiada.Web.Tasks;

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

//  reading connection sting from environment variables
builder.Configuration.AddEnvironmentVariables(prefix: "Libiada_");
string environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
string connectionString = builder.Configuration.GetConnectionString($"LibiadaDatabaseEntities_{environment}") ?? throw new Exception($"Connection string 'LibiadaDatabaseEntities_{environment}' is not found.");
builder.Configuration["ConnectionStrings:LibiadaDatabaseEntities"] = connectionString;

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

//Adding db context factory and it automaticly adds db context 
builder.Services.AddDbContextFactory<LibiadaDatabaseEntities>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AspNetUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<LibiadaDatabaseEntities>()
                .AddDefaultTokenProviders();

builder.Services.AddSingleton<Cache>();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPushNotificationHelper, PushNotificationHelper>();
builder.Services.AddSingleton<ITaskManager, TaskManager>();

builder.Services.AddSingleton<INcbiHelper, NcbiHelper>();

builder.Services.AddSingleton<IAccordanceCharacteristicRepository, AccordanceCharacteristicRepository>();
builder.Services.AddSingleton<IBinaryCharacteristicRepository, BinaryCharacteristicRepository>();
builder.Services.AddSingleton<ICongenericCharacteristicRepository, CongenericCharacteristicRepository>();
builder.Services.AddSingleton<IFullCharacteristicRepository, FullCharacteristicRepository>();
builder.Services.AddSingleton<ICommonSequenceRepositoryFactory, CommonSequenceRepositoryFactory>();

builder.Services.AddScoped<ISequencesCharacteristicsCalculator, SequencesCharacteristicsCalculator>();
builder.Services.AddScoped<ICongenericSequencesCharacteristicsCalculator, CongenericSequencesCharacteristicsCalculator>();
builder.Services.AddScoped<ISubsequencesCharacteristicsCalculator, SubsequencesCharacteristicsCalculator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient(provider => ((provider.GetService<IHttpContextAccessor>() ?? throw new Exception($"IHttpContextAccessor is not found."))
                                                    .HttpContext ?? throw new Exception($"HttpContext is not found."))
                                                    .User);

builder.Services.AddTransient<IViewDataHelper, ViewDataHelper>();

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

// TODO: fix naming
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
