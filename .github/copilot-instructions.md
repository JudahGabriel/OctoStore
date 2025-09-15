# OctoStore

OctoStore is an ASP.NET Core web service that automatically publishes open source apps from GitHub repositories to the Microsoft Store. The service scans GitHub for `ms-store-publish.json` manifest files and creates Microsoft Store submissions for both Windows executables and Progressive Web Apps (PWAs).

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- Install .NET 9.0 SDK: `curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 9.0.100`
- Update PATH: `export PATH="$HOME/.dotnet:$PATH"`
- Verify installation: `dotnet --version` (should show 9.0.100 or higher)

### Building and Testing
- Restore dependencies: `dotnet restore` -- takes ~1-2 seconds. NEVER CANCEL. Set timeout to 5+ minutes.
- Build the application: `dotnet build --configuration Release` -- takes ~2-3 seconds. NEVER CANCEL. Set timeout to 5+ minutes.
- Publish the application: `dotnet publish -c Release -o /tmp/publish` -- takes ~1-2 seconds. NEVER CANCEL. Set timeout to 5+ minutes.
- No unit tests are present in this repository
- All build commands show a vulnerability warning for System.Text.Encodings.Web 4.5.0 - this is expected and can be ignored

### Running the Application
- **CRITICAL**: The application requires RavenDB credentials to start successfully
- Run locally: `dotnet run --configuration Release`
- Without proper configuration, the application will fail with: "Unable to find Raven cert base64 string in configuration"
- Required configuration (stored in user secrets or environment variables):
  - `RavenSettings:CertBase64` - Base64 encoded RavenDB certificate
  - `RavenSettings:CertPassword` - RavenDB certificate password  
  - `RavenSettings:Urls` - RavenDB connection URLs
  - `RavenSettings:DatabaseName` - RavenDB database name
  - `AppSettings:GitHubToken` - GitHub API token for repository scanning
- Default URLs when running: HTTP at http://localhost:5131, HTTPS at https://localhost:7280

### Validation
- ALWAYS build before testing any changes: `dotnet build --configuration Release`
- Test API endpoints using the included `OctoStore.http` file with REST Client extensions
- The main endpoints are:
  - `GET /appsubmission/status?repo=OWNER/REPO` - Check app submission status
  - `POST /appsubmission/scan?repo=OWNER/REPO` - Trigger repository scan
- **Cannot fully test locally** without valid RavenDB credentials and GitHub API token
- Application startup will fail with clear error message if credentials are missing
- **Validation scenarios**: After making changes, verify:
  1. Code compiles without errors: `dotnet build --configuration Release`
  2. Application starts (will fail at runtime but should compile): `dotnet run --configuration Release`
  3. Published output is generated: `dotnet publish -c Release -o /tmp/publish`
- Always verify that your changes compile successfully before committing

## Common Tasks

The following are outputs from frequently run commands. Reference them instead of viewing, searching, or running bash commands to save time.

### Repository Root Structure
```
.git/               - Git repository metadata
.github/            - GitHub workflows and actions
  workflows/
    build-and-deploy.yml - Azure deployment pipeline
.gitignore          - Standard Visual Studio gitignore
bin/                - Build output directory (ignored by git)
obj/                - Build intermediate files (ignored by git)
Common/             - Shared utilities and helpers
Controllers/        - ASP.NET Core controllers
  AppSubmissionController.cs - Main API controller
Models/             - Data models and entities
  AppSubmission.cs - Core submission entity
  StorePublishManifest.cs - Manifest model
  AppSettings.cs  - Configuration model
  *.cs            - Various supporting models
Properties/         - Project properties
  launchSettings.json - Development run configuration
Services/           - Business logic services
  GitHubService.cs - GitHub API integration
  AppSubmissionService.cs - Submission management
  GitHubPublishManifestFinder.cs - Background scanner
  RepoScanner.cs  - Repository processing
  TimedBackgroundService.cs - Base timer service
Views/              - Razor views
  AppSubmission/
    Status.cshtml   - Status page view
LICENSE             - Custom license with commercial use restrictions
Program.cs          - Application entry point
README.md           - Comprehensive project documentation
OctoStore.csproj    - Project file (.NET 9.0)
OctoStore.sln       - Visual Studio solution file
OctoStore.http      - HTTP request examples for testing
appsettings*.json   - Application configuration files
iarc-flow.txt       - IARC age rating questionnaire notes
```

### Key Project Details
- **Framework**: .NET 9.0 ASP.NET Core web application
- **Dependencies**: 
  - Octokit (GitHub API client)
  - RavenDB.Client (NoSQL database)
  - RavenDB.DependencyInjection
  - RavenDB.StructuredLogger
- **Database**: RavenDB (NoSQL document database)
- **Background Services**: 
  - GitHubPublishManifestFinder - Scans GitHub every 6 hours for manifest files
  - RepoScanner - Processes manual repository scan requests
- **Deployment**: Azure Web App via GitHub Actions

### Architecture Overview
- **Controllers**: Handle HTTP requests for app submission management
- **Services**: Business logic for GitHub integration, manifest processing, and submission management
- **Models**: Data entities representing app submissions, manifests, and configuration
- **Background Services**: Automated scanning and processing of GitHub repositories
- **Views**: Web UI for checking submission status

### Important Code Patterns
- Uses dependency injection extensively for services and database sessions
- Background services inherit from `TimedBackgroundServiceBase`
- Database operations use RavenDB's async document sessions
- GitHub API calls are centralized in `GitHubService`
- Configuration uses the standard ASP.NET Core options pattern

### GitHub Actions CI/CD
- Triggered on pushes to main branch
- Builds with .NET 9.0 on ubuntu-latest
- Publishes to Azure Web App named 'octostore'
- Uses Azure service principal authentication
- Build takes approximately 1-2 minutes

### License and Usage
- Custom license allowing free use for non-profit/educational/individual use
- Commercial entities require separate licensing agreement
- Open source but with commercial use restrictions

## Troubleshooting

### Build Issues
- If "NETSDK1045: The current .NET SDK does not support targeting .NET 9.0" error occurs, install .NET 9.0 SDK
- Vulnerability warnings for System.Text.Encodings.Web are expected and can be ignored
- Solution-level publish warnings (NETSDK1194) are expected when using --output with solution files

### Runtime Issues  
- "Unable to find Raven cert base64 string" means RavenDB configuration is missing
- Check that all required configuration values are set in user secrets or environment variables
- GitHub API rate limiting may occur without proper authentication token

### Development Workflow
- Always use `dotnet build` before making changes to verify current state
- Configuration changes require restart of the application
- Background services run automatically but can be triggered manually via API endpoints