using OctoStore.Models;
using OctoStore.Services;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Raven.StructuredLogger;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Configure app configuration to include environment variables and appsettings.production.json
builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true, reloadOnChange: true) // load environment-specific settings
    .AddEnvironmentVariables(); // Load settings from envs

// Add services to the container.
builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddControllersWithViews();
builder.Services.AddRavenDbDocStore(o =>
{
    // Grab the database connection info from configuration.
    var certBase64 = builder.Configuration["RavenSettings:CertBase64"];
    if (string.IsNullOrEmpty(certBase64))
    {
        throw new InvalidOperationException("Unable to find Raven cert base64 string in configuration. This is stored in VS user secrets, Azure web app env vars, and LP.");
    }
    var certPassword = builder.Configuration["RavenSettings:CertPassword"];
    if (string.IsNullOrEmpty(certPassword))
    {
        throw new InvalidOperationException("Unable to find Raven cert password in configuration. This is stored in VS user secrets, Azure web app env vars, and LP.");
    }
    var certBytes = Convert.FromBase64String(certBase64);
    o.Certificate = X509CertificateLoader.LoadPkcs12(certBytes, certPassword);

    // Also, ignore self-referencing loops for Raven logging
    o.BeforeInitializeDocStore = docStore => docStore.IgnoreSelfReferencingLoops();
});
builder.Services.AddRavenDbAsyncSession();
builder.Services.AddRavenStructuredLogger();
builder.Services.AddSingleton<GitHubService>();
builder.Services.AddSingleton<AppSubmissionService>();
builder.Services.AddHostedService<GitHubPublishManifestFinder>();
builder.Services.AddHostedService<RepoScanner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
