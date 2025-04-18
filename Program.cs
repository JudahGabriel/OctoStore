using Raven.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRavenDbDocStore(o =>
{
    var certBase64 = builder.Configuration["RavenSettings:CertBase64"];
    if (string.IsNullOrEmpty(certBase64))
    {
        throw new InvalidOperationException("Unable to find Raven cert base64 string in configuration. This is stored in VS user secrets, GitHub secrets, and LP.");
    }
    var certPassword = builder.Configuration["RavenSettings:CertPassword"];
    if (string.IsNullOrEmpty(certPassword))
    {
        throw new InvalidOperationException("Unable to find Raven cert password in configuration. This is stored in VS user secrets, GitHub secrets, and LP.");
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
