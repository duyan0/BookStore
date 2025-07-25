-- Add AvatarUrl column to Users table
-- This script manually adds the AvatarUrl column that should have been added by migration

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
