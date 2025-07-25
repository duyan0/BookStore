# PowerShell script to update database with AvatarUrl column
# Run this script if Entity Framework migrations are not working

param(
    [string]$ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookStoreDb;Trusted_Connection=true;MultipleActiveResultSets=true"
)

Write-Host "Starting database update for AvatarUrl column..." -ForegroundColor Green

try {
    # Import SQL Server module if available
    if (Get-Module -ListAvailable -Name SqlServer) {
        Import-Module SqlServer
        Write-Host "SqlServer module loaded successfully." -ForegroundColor Green
    } else {
        Write-Host "SqlServer module not available. Trying alternative method..." -ForegroundColor Yellow
    }

    # SQL command to add AvatarUrl column
    $sql = @"
-- Add AvatarUrl column to Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'AvatarUrl')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [AvatarUrl] NVARCHAR(255) NULL;
    PRINT 'AvatarUrl column added successfully to Users table.';
END
ELSE
BEGIN
    PRINT 'AvatarUrl column already exists in Users table.';
END

-- Update migration history
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20250125120000_AddAvatarUrlToUser')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250125120000_AddAvatarUrlToUser', '9.0.7');
    PRINT 'Migration history updated.';
END
ELSE
BEGIN
    PRINT 'Migration already recorded in history.';
END
"@

    # Try to execute SQL using Invoke-Sqlcmd if available
    if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
        Write-Host "Executing SQL using Invoke-Sqlcmd..." -ForegroundColor Yellow
        Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sql
        Write-Host "Database update completed successfully!" -ForegroundColor Green
    } else {
        # Alternative method using .NET SqlConnection
        Write-Host "Using .NET SqlConnection method..." -ForegroundColor Yellow
        
        Add-Type -AssemblyName System.Data
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
        
        $connection.Open()
        $result = $command.ExecuteNonQuery()
        $connection.Close()
        
        Write-Host "Database update completed successfully!" -ForegroundColor Green
    }

    Write-Host "You can now restart your application and try logging in again." -ForegroundColor Green
    
} catch {
    Write-Host "Error updating database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check your connection string and database access." -ForegroundColor Red
    
    # Show the SQL that would be executed
    Write-Host "`nSQL that should be executed:" -ForegroundColor Yellow
    Write-Host $sql -ForegroundColor Cyan
}

Write-Host "`nScript completed." -ForegroundColor Green
