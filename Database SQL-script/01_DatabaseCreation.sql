-- Crear base de datos
-- Parte 1: 01_DatabaseCreation.sql 
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'ClinicaDB')
BEGIN
	CREATE DATABASE ClinicaDB;
END
GO

-- Usar la base de datos creada
USE ClinicaDB;
GO

-- Tabla de Roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
	CREATE TABLE Roles (
		Id INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(50) NOT NULL,
		Description NVARCHAR(255),
		CreatedAt DATETIME2 DEFAULT GETDATE()
	);

	--- Insertar roles base
	INSERT INTO Roles (Name, Description) VALUES
	('Admin', 'Administrador del sistema con acceso completo'),
	('Doctor', 'Médico de la clinica que atiende pacientes'),
	('Patient', 'Paciente del sistema'),
	('Employee', 'Empleado administrativo de la clinica');
END
GO

-- Tabla de Tipos de Documento
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DocumentTypes' AND xtype='U')
BEGIN
	CREATE TABLE DocumentTypes (
		Id INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(50) NOT NULL,
		Code NVARCHAR(10) NOT NULL UNIQUE
	);

	INSERT INTO DocumentTypes (Name, Code) VALUES
	('Cédula de Ciudadanía', 'CC'),
	('Cédula de Extranjería', 'CE'),
    ('Pasaporte', 'PAS'),
    ('Tarjeta de Identidad', 'TI');
END
GO

-- Tabla de Usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
	CREATE TABLE Users(
		Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
		UserName NVARCHAR(100) NOT NULL UNIQUE,
		Email NVARCHAR(255) NOT NULL UNIQUE,
		PasswordHash NVARCHAR(MAX) NOT NULL,
		RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(Id),
		IsActive BIT DEFAULT 1,
		CreatedAt DATETIME2 DEFAULT GETDATE(),
		UpdatedAt DATETIME2 DEFAULT GETDATE()
	);
END
GO

-- Tabla de Pacientes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Patients' AND xtype='U')
BEGIN
	CREATE TABLE Patients (
		Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
		DocumentTypeId INT NOT NULL FOREIGN KEY REFERENCES DocumentTypes(Id),
        DocumentNumber NVARCHAR(20) NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        BirthDate DATE NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        Gender NVARCHAR(20) NOT NULL,
        Address NVARCHAR(500),
        PhoneNumber NVARCHAR(20),
        UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE()
	);
END
GO

-- Tabla de Doctores
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Doctors' AND xtype='U')
BEGIN
    CREATE TABLE Doctors (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        DocumentTypeId INT NOT NULL FOREIGN KEY REFERENCES DocumentTypes(Id),
        DocumentNumber NVARCHAR(20) NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Specialty NVARCHAR(100) NOT NULL,
        LicenseNumber NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PhoneNumber NVARCHAR(20),
        UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETDATE()
    );
END
GO

-- Tabla de Citas Médicas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Appointments' AND xtype='U')
BEGIN
    CREATE TABLE Appointments (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PatientId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Patients(Id),
        DoctorId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Doctors(Id),
        AppointmentDate DATETIME2 NOT NULL,
        Status NVARCHAR(20) DEFAULT 'Scheduled',
        Reason NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE()
    );
END
GO

-- Tabla de Historial Médico
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MedicalRecords' AND xtype='U')
BEGIN
    CREATE TABLE MedicalRecords (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AppointmentId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Appointments(Id),
        PatientId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Patients(Id),
        DoctorId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Doctors(Id),
        Diagnosis NVARCHAR(MAX) NOT NULL,
        Treatment NVARCHAR(MAX),
        Prescription NVARCHAR(MAX),
        Notes NVARCHAR(MAX),
        RecordDate DATETIME2 DEFAULT GETDATE(),
        CreatedAt DATETIME2 DEFAULT GETDATE()
    );
END
GO

-- Crear índices para mejorar rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Patients_DocumentNumber')
BEGIN
    CREATE INDEX IX_Patients_DocumentNumber ON Patients(DocumentNumber);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Patients_UserId')
BEGIN
    CREATE INDEX IX_Patients_UserId ON Patients(UserId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Appointments_PatientId')
BEGIN
    CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Appointments_DoctorId')
BEGIN
    CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MedicalRecords_PatientId')
BEGIN
    CREATE INDEX IX_MedicalRecords_PatientId ON MedicalRecords(PatientId);
END
GO




