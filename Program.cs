using OctoStore.Models;
using OctoStore.Services;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Raven.StructuredLogger;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure app configuration to include environment variables and appsettings.production.json
builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true, reloadOnChange: true) // load environment-specific settings
    .AddEnvironmentVariables(); // Load settings from envs

// Add services to the container.
builder.Services.AddOptions();
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection(nameof(GitHubSettings)));
builder.Services.AddControllersWithViews();

// Add authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
    });

// Add GitHub OAuth only if configured
var gitHubSettings = builder.Configuration.GetSection(nameof(GitHubSettings)).Get<GitHubSettings>();
if (!string.IsNullOrEmpty(gitHubSettings?.ClientId) && !string.IsNullOrEmpty(gitHubSettings?.ClientSecret))
{
    builder.Services.AddAuthentication().AddOAuth("GitHub", options =>
    {
        options.ClientId = gitHubSettings.ClientId;
        options.ClientSecret = gitHubSettings.ClientSecret;
        options.CallbackPath = "/auth/github-callback";
        
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                
                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                
                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                
                // Manually add claims
                var id = json.RootElement.GetProperty("id").GetInt64().ToString();
                var login = json.RootElement.TryGetProperty("login", out var loginElement) ? loginElement.GetString() : "Unknown";
                var avatarUrl = json.RootElement.TryGetProperty("avatar_url", out var avatarElement) ? avatarElement.GetString() : "";
                
                context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                context.Identity?.AddClaim(new Claim(ClaimTypes.Name, login ?? "Unknown"));
                context.Identity?.AddClaim(new Claim("avatar_url", avatarUrl ?? ""));
                
                // Store the access token for API calls
                context.Identity?.AddClaim(new Claim("access_token", context.AccessToken!));
            }
        };
        
        options.Scope.Add("repo");
    });
}
else
{
    // GitHub OAuth is not configured - this is expected during development without secrets
    Console.WriteLine("Warning: GitHub OAuth is not configured. GitHub authentication will not be available.");
}

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
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
