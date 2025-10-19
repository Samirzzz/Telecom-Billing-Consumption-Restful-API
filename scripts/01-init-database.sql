USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TelecomBilling')
BEGIN
    CREATE DATABASE TelecomBilling;
END
GO

USE TelecomBilling;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Username nvarchar(50) NOT NULL UNIQUE,
        Email nvarchar(100) NOT NULL UNIQUE,
        PasswordHash nvarchar(max) NOT NULL,
        Role int NOT NULL,
        CreatedAt datetime2 NOT NULL,
        IsActive bit NOT NULL,
        Name nvarchar(100) NOT NULL,
        PhoneNumber nvarchar(20) NOT NULL UNIQUE,
        PlanType nvarchar(50) NOT NULL,
        Country nvarchar(50) NOT NULL,
        IsRoaming bit NOT NULL,
        LastUpdated datetime2 NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Token nvarchar(max) NOT NULL UNIQUE,
        ExpiresAt datetime2 NOT NULL,
        CreatedAt datetime2 NOT NULL,
        IsRevoked bit NOT NULL,
        UserId int NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsageRecords')
BEGIN
    CREATE TABLE UsageRecords (
        Id int IDENTITY(1,1) PRIMARY KEY,
        UserId int NOT NULL,
        Timestamp datetime2 NOT NULL,
        CallMinutes int NOT NULL,
        DataMB int NOT NULL,
        SMSCount int NOT NULL,
        IsPeakTime bit NOT NULL,
        IsRoaming bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        Id int IDENTITY(1,1) PRIMARY KEY,
        UserId int NOT NULL,
        Month nvarchar(7) NOT NULL,
        BillingDate datetime2 NOT NULL,
        TotalAmount decimal(18,2) NOT NULL,
        VoiceAmount decimal(18,2) NOT NULL,
        DataAmount decimal(18,2) NOT NULL,
        SMSAmount decimal(18,2) NOT NULL,
        RoamingAmount decimal(18,2) NOT NULL,
        VoiceMinutes int NOT NULL,
        DataMB int NOT NULL,
        SMSMessages int NOT NULL,
        RoamingMinutes int NOT NULL,
        RoamingDataMB int NOT NULL,
        RoamingSMSMessages int NOT NULL,
        CreatedAt datetime2 NOT NULL,
        LastUpdated datetime2 NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        UNIQUE(UserId, Month)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TariffRules')
BEGIN
    CREATE TABLE TariffRules (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        PlanType nvarchar(50) NOT NULL,
        VoicePeakRate decimal(18,2) NOT NULL,
        VoiceOffPeakRate decimal(18,2) NOT NULL,
        DataRate decimal(18,2) NOT NULL,
        SMSRate decimal(18,2) NOT NULL,
        RoamingVoiceRate decimal(18,2) NOT NULL,
        RoamingDataRate decimal(18,2) NOT NULL,
        RoamingSMSRate decimal(18,2) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        LastUpdated datetime2 NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BundleLimits')
BEGIN
    CREATE TABLE BundleLimits (
        Id int IDENTITY(1,1) PRIMARY KEY,
        PlanType nvarchar(50) NOT NULL UNIQUE,
        VoiceMinutesLimit int NOT NULL,
        DataMBLimit int NOT NULL,
        SMSLimit int NOT NULL,
        PeakTimeMinutesLimit int NOT NULL,
        OffPeakTimeMinutesLimit int NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        LastUpdated datetime2 NULL
    );
END
GO

CREATE INDEX IX_UsageRecords_UserId_Timestamp ON UsageRecords(UserId, Timestamp);
GO
