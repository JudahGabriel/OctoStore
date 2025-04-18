using Raven.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Configure app configuration to include environment variables and appsettings.production.json
builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true, reloadOnChange: true) // load environment-specific settings
    .AddEnvironmentVariables(); // Load settings from env vars

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRavenDbDocStore(o =>
{
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
});
builder.Services.AddRavenDbAsyncSession();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
