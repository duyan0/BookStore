# Manual Database Update Instructions

## Problem
The application is showing "Invalid column name 'AvatarUrl'" error because the database schema hasn't been updated to include the new AvatarUrl column that was added to the User entity.

## Solution Options

### Option 1: Using SQL Server Management Studio (SSMS)

1. **Open SQL Server Management Studio**
2. **Connect to your LocalDB instance**: `(localdb)\mssqllocaldb`
3. **Navigate to your BookStoreDb database**
4. **Open a New Query window**
5. **Execute the following SQL:**

```sql
-- Add AvatarUrl column to Users table
USE [BookStoreDb]
GO

-- Check if column already exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'AvatarUrl')
BEGIN
    -- Add AvatarUrl column
    ALTER TABLE [dbo].[Users]
    ADD [AvatarUrl] NVARCHAR(255) NULL;
    
    PRINT 'AvatarUrl column added successfully to Users table.';
END
ELSE
BEGIN
    PRINT 'AvatarUrl column already exists in Users table.';
END

-- Update migration history to reflect that this migration has been applied
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
GO
```

### Option 2: Using PowerShell Script

1. **Open PowerShell as Administrator**
2. **Navigate to the BookStore.Infrastructure/Scripts directory**
3. **Run the PowerShell script:**

```powershell
.\UpdateDatabase.ps1
```

### Option 3: Using Command Line with sqlcmd

1. **Open Command Prompt**
2. **Run the following command:**

```cmd
sqlcmd -S "(localdb)\mssqllocaldb" -d BookStoreDb -i "AddAvatarUrlColumn.sql"
```

### Option 4: Let the Application Handle It Automatically

The application has been updated with automatic column detection and creation. When you restart the application, it will automatically check for and add the AvatarUrl column if it doesn't exist.

1. **Simply restart your BookStore.API and BookStore.Web applications**
2. **The DbInitializer will automatically add the missing column**
3. **Try logging in again**

## Verification

After running any of the above options, you can verify the column was added by running:

```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AvatarUrl'
```

Expected result:
- COLUMN_NAME: AvatarUrl
- DATA_TYPE: nvarchar
- CHARACTER_MAXIMUM_LENGTH: 255
- IS_NULLABLE: YES

## Troubleshooting

### If you get "Database does not exist" error:
1. Make sure your connection string is correct
2. Try running the application once to create the database
3. Then run the SQL script

### If you get "Permission denied" error:
1. Make sure you're running as Administrator
2. Check that LocalDB is running: `sqllocaldb info mssqllocaldb`
3. Start LocalDB if needed: `sqllocaldb start mssqllocaldb`

### If the migration history update fails:
This is not critical. The column addition is the important part. The migration history is just for Entity Framework tracking.

## After Update

Once the database is updated:
1. **Restart your applications** (BookStore.API and BookStore.Web)
2. **Try logging in again** - the error should be resolved
3. **Test avatar upload functionality** in the user profile page

## Contact

If you continue to have issues, please check:
1. Connection string in appsettings.json
2. LocalDB service status
3. Database permissions
4. Application logs for additional error details
