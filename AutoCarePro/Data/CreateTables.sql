-- Switch to master database first
USE master;
GO

-- Drop existing database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'AutoCarePro')
BEGIN
    -- Kill all connections to the database
    DECLARE @kill varchar(8000) = '';  
    SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
    FROM sys.dm_exec_sessions
    WHERE database_id  = db_id('AutoCarePro')
    
    EXEC(@kill);
    
    -- Now drop the database
    DROP DATABASE AutoCarePro;
END
GO

-- Create and use the database
CREATE DATABASE AutoCarePro;
GO

USE AutoCarePro;
GO

-- Drop existing triggers if they exist
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Vehicles_UpdateTimestamp')
    DROP TRIGGER TR_Vehicles_UpdateTimestamp;
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_MaintenanceRecords_UpdateTimestamp')
    DROP TRIGGER TR_MaintenanceRecords_UpdateTimestamp;
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_MaintenanceRecommendations_UpdateTimestamp')
    DROP TRIGGER TR_MaintenanceRecommendations_UpdateTimestamp;
GO

-- Drop existing tables if they exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRecommendations')
    DROP TABLE MaintenanceRecommendations;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRecords')
    DROP TABLE MaintenanceRecords;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
    DROP TABLE Vehicles;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    DROP TABLE Users;
GO

-- Create Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,  -- Store hashed passwords only
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    Type NVARCHAR(20) NOT NULL DEFAULT 'Car Owner',  -- Added Type column
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME
);
GO

-- Create Vehicles table
CREATE TABLE Vehicles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Make NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    LicensePlate NVARCHAR(20) NOT NULL,
    VIN NVARCHAR(17) UNIQUE,
    CurrentMileage DECIMAL(10,2) NOT NULL,
    LastServiceDate DATETIME,
    NextServiceDate DATETIME,
    FuelType NVARCHAR(20),
    TransmissionType NVARCHAR(20),
    Color NVARCHAR(30),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- Create MaintenanceRecords table
CREATE TABLE MaintenanceRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    MaintenanceDate DATETIME NOT NULL,
    MaintenanceType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    MileageAtMaintenance DECIMAL(10,2) NOT NULL,
    Cost DECIMAL(10,2) NOT NULL,
    ServiceProvider NVARCHAR(100),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE
);
GO

-- Create MaintenanceRecommendations table
CREATE TABLE MaintenanceRecommendations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    Component NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Priority NVARCHAR(20) NOT NULL,
    RecommendedDate DATETIME NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE
);
GO

-- Create indexes for better query performance
CREATE INDEX IX_Vehicles_UserId ON Vehicles(UserId);
CREATE INDEX IX_MaintenanceRecords_VehicleId ON MaintenanceRecords(VehicleId);
CREATE INDEX IX_MaintenanceRecords_MaintenanceDate ON MaintenanceRecords(MaintenanceDate);
CREATE INDEX IX_MaintenanceRecommendations_VehicleId ON MaintenanceRecommendations(VehicleId);
CREATE INDEX IX_MaintenanceRecommendations_RecommendedDate ON MaintenanceRecommendations(RecommendedDate);
GO

-- Add check constraints
ALTER TABLE Vehicles
ADD CONSTRAINT CHK_Vehicles_Year CHECK (Year >= 1900 AND Year <= YEAR(GETDATE()) + 1);

ALTER TABLE Vehicles
ADD CONSTRAINT CHK_Vehicles_CurrentMileage CHECK (CurrentMileage >= 0);

ALTER TABLE MaintenanceRecords
ADD CONSTRAINT CHK_MaintenanceRecords_Cost CHECK (Cost >= 0);

ALTER TABLE MaintenanceRecords
ADD CONSTRAINT CHK_MaintenanceRecords_MileageAtMaintenance CHECK (MileageAtMaintenance >= 0);
GO

-- Add trigger to update UpdatedAt timestamp for Vehicles
CREATE TRIGGER TR_Vehicles_UpdateTimestamp
ON Vehicles
AFTER UPDATE
AS
BEGIN
    UPDATE Vehicles
    SET UpdatedAt = GETDATE()
    FROM Vehicles v
    INNER JOIN inserted i ON v.Id = i.Id;
END;
GO

-- Add trigger to update UpdatedAt timestamp for MaintenanceRecords
CREATE TRIGGER TR_MaintenanceRecords_UpdateTimestamp
ON MaintenanceRecords
AFTER UPDATE
AS
BEGIN
    UPDATE MaintenanceRecords
    SET UpdatedAt = GETDATE()
    FROM MaintenanceRecords mr
    INNER JOIN inserted i ON mr.Id = i.Id;
END;
GO

-- Add trigger to update UpdatedAt timestamp for MaintenanceRecommendations
CREATE TRIGGER TR_MaintenanceRecommendations_UpdateTimestamp
ON MaintenanceRecommendations
AFTER UPDATE
AS
BEGIN
    UPDATE MaintenanceRecommendations
    SET UpdatedAt = GETDATE()
    FROM MaintenanceRecommendations mr
    INNER JOIN inserted i ON mr.Id = i.Id;
END;
GO 