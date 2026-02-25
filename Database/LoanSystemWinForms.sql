USE [master];
GO

IF DB_ID(N'LoanSystemWinForms') IS NULL
BEGIN
    CREATE DATABASE [LoanSystemWinForms];
END;
GO

USE [LoanSystemWinForms];
GO

SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id NVARCHAR(64) NOT NULL PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(320) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(512) NOT NULL,
        Role INT NOT NULL,
        Status INT NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.GameHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GameHistories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(64) NOT NULL,
        GameName NVARCHAR(100) NOT NULL,
        Score INT NOT NULL,
        Result NVARCHAR(200) NOT NULL,
        PlayedAt DATETIME2(7) NOT NULL,
        CONSTRAINT FK_GameHistories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.LoanHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoanHistories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(64) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        InterestRate FLOAT NOT NULL,
        TermYears INT NOT NULL,
        TermMonths INT NOT NULL,
        TotalPayment DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL,
        CONSTRAINT FK_LoanHistories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_GameHistories_UserId_PlayedAt'
      AND object_id = OBJECT_ID(N'dbo.GameHistories')
)
BEGIN
    CREATE INDEX IX_GameHistories_UserId_PlayedAt
    ON dbo.GameHistories(UserId, PlayedAt DESC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_LoanHistories_UserId_CreatedAt'
      AND object_id = OBJECT_ID(N'dbo.LoanHistories')
)
BEGIN
    CREATE INDEX IX_LoanHistories_UserId_CreatedAt
    ON dbo.LoanHistories(UserId, CreatedAt DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE LOWER(Email) = LOWER(N'admin@loansystem.com'))
BEGIN
    INSERT INTO dbo.Users (Id, FullName, Email, PasswordHash, Role, Status, CreatedAt)
    VALUES (
        REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N''),
        N'System Administrator',
        N'admin@loansystem.com',
        N'AAECAwQFBgcICQoLDA0ODw==:j/y1nhXuLpowYyCS3HEz0917oiNd/4H6swmCvwcgLl4=',
        1,
        1,
        SYSUTCDATETIME()
    );
END;
GO
