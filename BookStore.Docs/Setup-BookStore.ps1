# =============================================
# BookStore Automatic Setup Script
# =============================================
# This PowerShell script automatically sets up BookStore on a new machine
# Run as Administrator for best results

param(
    [string]$ServerName = "(localdb)\mssqllocaldb",
    [string]$DatabaseName = "BookStoreDB",
    [switch]$SkipDatabase,
    [switch]$SkipBuild,
    [switch]$Help
)

if ($Help) {
    Write-Host "BookStore Setup Script" -ForegroundColor Green
    Write-Host "Usage: .\Setup-BookStore.ps1 [parameters]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Parameters:" -ForegroundColor Cyan
    Write-Host "  -ServerName     SQL Server instance (default: (localdb)\mssqllocaldb)"
    Write-Host "  -DatabaseName   Database name (default: BookStoreDB)"
    Write-Host "  -SkipDatabase   Skip database setup"
    Write-Host "  -SkipBuild      Skip building the application"
    Write-Host "  -Help           Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\Setup-BookStore.ps1"
    Write-Host "  .\Setup-BookStore.ps1 -ServerName '.\SQLEXPRESS'"
    Write-Host "  .\Setup-BookStore.ps1 -SkipDatabase"
    exit
}

Write-Host "===============================================" -ForegroundColor Green
Write-Host "BookStore Automatic Setup Script" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Warning "Not running as Administrator. Some operations may fail."
}

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Function to run SQL command
function Invoke-SqlCommand($connectionString, $query) {
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $result = $command.ExecuteNonQuery()
        $connection.Close()
        return $true
    }
    catch {
        Write-Error "SQL Error: $($_.Exception.Message)"
        return $false
    }
}

# Step 1: Check Prerequisites
Write-Host "Step 1: Checking Prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
if (Test-Command "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Error "✗ .NET SDK not found. Please install .NET 9.0 SDK"
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
    exit 1
}

# Check SQL Server
Write-Host "Checking SQL Server connection..." -ForegroundColor Yellow
$connectionString = "Server=$ServerName;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "✓ SQL Server connection successful" -ForegroundColor Green
} catch {
    Write-Error "✗ Cannot connect to SQL Server: $ServerName"
    Write-Host "Please ensure SQL Server is running and accessible" -ForegroundColor Red
    exit 1
}

# Step 2: Setup Database
if (-not $SkipDatabase) {
    Write-Host "Step 2: Setting up Database..." -ForegroundColor Yellow
    
    # Check if database exists
    $checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
    $dbConnection = "Server=$ServerName;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($dbConnection)
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $checkDbQuery
        $dbExists = $command.ExecuteScalar()
        $connection.Close()
        
        if ($dbExists -eq 0) {
            Write-Host "Creating database $DatabaseName..." -ForegroundColor Yellow
            
            # Run the setup SQL script
            $sqlScript = Get-Content -Path "SETUP_NEW_MACHINE.sql" -Raw
            $sqlScript = $sqlScript -replace "BookStoreDB", $DatabaseName
            
            # Execute SQL script
            sqlcmd -S $ServerName -E -Q $sqlScript
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Database setup completed successfully" -ForegroundColor Green
            } else {
                Write-Error "✗ Database setup failed"
                exit 1
            }
        } else {
            Write-Host "✓ Database $DatabaseName already exists" -ForegroundColor Green
        }
    } catch {
        Write-Error "Database setup error: $($_.Exception.Message)"
        exit 1
    }
} else {
    Write-Host "Step 2: Skipping Database setup" -ForegroundColor Yellow
}

# Step 3: Update Configuration
Write-Host "Step 3: Updating Configuration..." -ForegroundColor Yellow

# Update API appsettings.json
$apiSettingsPath = "..\BookStore.API\appsettings.json"
if (Test-Path $apiSettingsPath) {
    $apiSettings = Get-Content $apiSettingsPath | ConvertFrom-Json
    $apiSettings.ConnectionStrings.DefaultConnection = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True;"
    $apiSettings | ConvertTo-Json -Depth 10 | Set-Content $apiSettingsPath
    Write-Host "✓ API configuration updated" -ForegroundColor Green
} else {
    Write-Warning "API appsettings.json not found at $apiSettingsPath"
}

# Step 4: Build Application
if (-not $SkipBuild) {
    Write-Host "Step 4: Building Application..." -ForegroundColor Yellow
    
    # Navigate to solution directory
    $solutionPath = ".."
    Push-Location $solutionPath
    
    try {
        # Restore packages
        Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
        dotnet restore
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Packages restored successfully" -ForegroundColor Green
        } else {
            Write-Error "✗ Package restore failed"
            Pop-Location
            exit 1
        }
        
        # Build solution
        Write-Host "Building solution..." -ForegroundColor Yellow
        dotnet build --configuration Release
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Build completed successfully" -ForegroundColor Green
        } else {
            Write-Error "✗ Build failed"
            Pop-Location
            exit 1
        }
        
    } finally {
        Pop-Location
    }
} else {
    Write-Host "Step 4: Skipping Build" -ForegroundColor Yellow
}

# Step 5: Create Start Scripts
Write-Host "Step 5: Creating Start Scripts..." -ForegroundColor Yellow

# Create start-api.bat
$startApiScript = @"
@echo off
echo Starting BookStore API...
cd /d "%~dp0..\BookStore.API"
dotnet run
pause
"@
$startApiScript | Out-File -FilePath "start-api.bat" -Encoding ASCII

# Create start-web.bat
$startWebScript = @"
@echo off
echo Starting BookStore Web...
cd /d "%~dp0..\BookStore.Web"
dotnet run
pause
"@
$startWebScript | Out-File -FilePath "start-web.bat" -Encoding ASCII

# Create start-both.bat
$startBothScript = @"
@echo off
echo Starting BookStore Application...
echo.
echo Starting API in new window...
start "BookStore API" cmd /k "cd /d "%~dp0..\BookStore.API" && dotnet run"

echo Waiting 5 seconds for API to start...
timeout /t 5 /nobreak > nul

echo Starting Web in new window...
start "BookStore Web" cmd /k "cd /d "%~dp0..\BookStore.Web" && dotnet run"

echo.
echo Both applications are starting...
echo API: http://localhost:5274
echo Web: http://localhost:5106
echo Admin: http://localhost:5106/Admin
echo.
echo Press any key to exit...
pause > nul
"@
$startBothScript | Out-File -FilePath "start-both.bat" -Encoding ASCII

Write-Host "✓ Start scripts created" -ForegroundColor Green

# Final Summary
Write-Host ""
Write-Host "===============================================" -ForegroundColor Green
Write-Host "BookStore Setup Complete!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Run 'start-both.bat' to start both API and Web" -ForegroundColor White
Write-Host "2. Open browser to http://localhost:5106" -ForegroundColor White
Write-Host "3. Login as admin: admin@bookstore.com / Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "Useful URLs:" -ForegroundColor Cyan
Write-Host "• Website: http://localhost:5106" -ForegroundColor White
Write-Host "• Admin Panel: http://localhost:5106/Admin" -ForegroundColor White
Write-Host "• API: http://localhost:5274" -ForegroundColor White
Write-Host "• Swagger: http://localhost:5274/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Database:" -ForegroundColor Cyan
Write-Host "• Server: $ServerName" -ForegroundColor White
Write-Host "• Database: $DatabaseName" -ForegroundColor White
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Cyan
Write-Host "• README.md - Overview and quick start" -ForegroundColor White
Write-Host "• USER_GUIDE.md - User manual" -ForegroundColor White
Write-Host "• ADMIN_GUIDE.md - Admin manual" -ForegroundColor White
Write-Host "• DEPLOYMENT_GUIDE.md - Deployment instructions" -ForegroundColor White
Write-Host ""
Write-Host "Happy coding! 📚" -ForegroundColor Green
