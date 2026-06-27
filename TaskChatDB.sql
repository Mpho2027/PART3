-- ============================================================
-- TaskChat Database Setup Script
-- Run this in SQL Server Management Studio (SSMS)
-- ============================================================

CREATE DATABASE TaskChat;
GO

USE TaskChat;
GO

CREATE TABLE Tasks (
    TaskId      INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(500)  NOT NULL,
    IsCompleted BIT            NOT NULL DEFAULT 0,
    ReminderDate DATETIME      NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
);
GO
