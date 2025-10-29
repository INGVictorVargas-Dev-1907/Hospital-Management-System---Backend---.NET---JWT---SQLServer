-- Parte 2: 02_StoredProcedures.sql

USE ClinicaDB;
GO

-- Stored Procedure para crear paciente
CREATE OR ALTER PROCEDURE sp_CreatePatient
    @DocumentTypeId INT,
    @DocumentNumber NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @BirthDate DATE,
    @Email NVARCHAR(255),
    @Gender NVARCHAR(20),
    @Address NVARCHAR(500),
    @PhoneNumber NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM Patients WHERE DocumentNumber = @DocumentNumber)
        BEGIN
            THROW 50000, 'El número de documento ya existe', 1;
        END
        
        IF EXISTS (SELECT 1 FROM Patients WHERE Email = @Email)
        BEGIN
            THROW 50000, 'El email ya está registrado', 1;
        END
        
        INSERT INTO Patients (DocumentTypeId, DocumentNumber, FirstName, LastName, 
                             BirthDate, Email, Gender, Address, PhoneNumber, UserId)
        VALUES (@DocumentTypeId, @DocumentNumber, @FirstName, @LastName, 
                @BirthDate, @Email, @Gender, @Address, @PhoneNumber, @UserId);
        
        COMMIT TRANSACTION;
        
        SELECT Id, DocumentNumber, FirstName, LastName, Email 
        FROM Patients 
        WHERE DocumentNumber = @DocumentNumber;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored Procedure para obtener todos los pacientes
CREATE OR ALTER PROCEDURE sp_GetAllPatients
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        dt.Name AS DocumentType,
        p.DocumentNumber,
        p.FirstName,
        p.LastName,
        p.BirthDate,
        p.Email,
        p.Gender,
        p.Address,
        p.PhoneNumber,
        p.IsActive,
        p.CreatedAt
    FROM Patients p
    INNER JOIN DocumentTypes dt ON p.DocumentTypeId = dt.Id
    WHERE p.IsActive = 1
    ORDER BY p.FirstName, p.LastName;
END
GO

-- Stored Procedure para obtener paciente por ID
CREATE OR ALTER PROCEDURE sp_GetPatientById
    @PatientId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        dt.Name AS DocumentType,
        p.DocumentNumber,
        p.FirstName,
        p.LastName,
        p.BirthDate,
        p.Email,
        p.Gender,
        p.Address,
        p.PhoneNumber,
        p.IsActive,
        p.CreatedAt,
        u.Id AS UserId
    FROM Patients p
    INNER JOIN DocumentTypes dt ON p.DocumentTypeId = dt.Id
    INNER JOIN Users u ON p.UserId = u.Id
    WHERE p.Id = @PatientId AND p.IsActive = 1;
END
GO

-- Stored Procedure para actualizar paciente
CREATE OR ALTER PROCEDURE sp_UpdatePatient
    @PatientId UNIQUEIDENTIFIER,
    @DocumentTypeId INT,
    @DocumentNumber NVARCHAR(20),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @BirthDate DATE,
    @Email NVARCHAR(255),
    @Gender NVARCHAR(20),
    @Address NVARCHAR(500),
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM Patients WHERE DocumentNumber = @DocumentNumber AND Id != @PatientId)
        BEGIN
            THROW 50000, 'El número de documento ya existe', 1;
        END
        
        IF EXISTS (SELECT 1 FROM Patients WHERE Email = @Email AND Id != @PatientId)
        BEGIN
            THROW 50000, 'El email ya está registrado', 1;
        END
        
        UPDATE Patients 
        SET DocumentTypeId = @DocumentTypeId,
            DocumentNumber = @DocumentNumber,
            FirstName = @FirstName,
            LastName = @LastName,
            BirthDate = @BirthDate,
            Email = @Email,
            Gender = @Gender,
            Address = @Address,
            PhoneNumber = @PhoneNumber,
            UpdatedAt = GETDATE()
        WHERE Id = @PatientId;
        
        COMMIT TRANSACTION;
        
        SELECT Id, DocumentNumber, FirstName, LastName, Email 
        FROM Patients 
        WHERE Id = @PatientId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored Procedure para eliminar paciente (soft delete)
CREATE OR ALTER PROCEDURE sp_DeletePatient
    @PatientId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Patients 
    SET IsActive = 0, UpdatedAt = GETDATE()
    WHERE Id = @PatientId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Stored Procedure para crear historial médico
CREATE OR ALTER PROCEDURE sp_CreateMedicalRecord
    @AppointmentId UNIQUEIDENTIFIER,
    @PatientId UNIQUEIDENTIFIER,
    @DoctorId UNIQUEIDENTIFIER,
    @Diagnosis NVARCHAR(MAX),
    @Treatment NVARCHAR(MAX) = NULL,
    @Prescription NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO MedicalRecords (AppointmentId, PatientId, DoctorId, Diagnosis, 
                                   Treatment, Prescription, Notes)
        VALUES (@AppointmentId, @PatientId, @DoctorId, @Diagnosis, 
                @Treatment, @Prescription, @Notes);
        
        -- Actualizar estado de la cita a completada
        UPDATE Appointments 
        SET Status = 'Completed', UpdatedAt = GETDATE()
        WHERE Id = @AppointmentId;
        
        COMMIT TRANSACTION;
        
        SELECT Id, RecordDate, Diagnosis
        FROM MedicalRecords 
        WHERE AppointmentId = @AppointmentId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored Procedure para obtener historial médico del paciente
CREATE OR ALTER PROCEDURE sp_GetPatientMedicalHistory
    @PatientId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        mr.Id,
        mr.RecordDate,
        d.FirstName + ' ' + d.LastName AS DoctorName,
        d.Specialty,
        a.AppointmentDate,
        mr.Diagnosis,
        mr.Treatment,
        mr.Prescription,
        mr.Notes
    FROM MedicalRecords mr
    INNER JOIN Doctors d ON mr.DoctorId = d.Id
    INNER JOIN Appointments a ON mr.AppointmentId = a.Id
    WHERE mr.PatientId = @PatientId
    ORDER BY mr.RecordDate DESC;
END
GO

-- Stored Procedure para obtener citas por doctor
CREATE OR ALTER PROCEDURE sp_GetAppointmentsByDoctor
    @DoctorId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Id,
        a.AppointmentDate,
        a.Status,
        a.Reason,
        p.FirstName + ' ' + p.LastName AS PatientName,
        p.DocumentNumber,
        p.PhoneNumber,
        p.Email
    FROM Appointments a
    INNER JOIN Patients p ON a.PatientId = p.Id
    WHERE a.DoctorId = @DoctorId
    AND (@Status IS NULL OR a.Status = @Status)
    ORDER BY a.AppointmentDate DESC;
END
GO

-- Stored Procedure para obtener citas por paciente
CREATE OR ALTER PROCEDURE sp_GetAppointmentsByPatient
    @PatientId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Id,
        a.PatientId,
        p.FirstName + ' ' + p.LastName AS PatientName,
        p.DocumentNumber AS PatientDocument,
        a.DoctorId,
        d.FirstName + ' ' + d.LastName AS DoctorName,
        d.Specialty AS DoctorSpecialty,
        a.AppointmentDate,
        a.Status,
        a.Reason,
        a.CreatedAt
    FROM Appointments a
    INNER JOIN Patients p ON a.PatientId = p.Id
    INNER JOIN Doctors d ON a.DoctorId = d.Id
    WHERE a.PatientId = @PatientId
    ORDER BY a.AppointmentDate DESC;
END
GO

-- Stored Procedure para obtener todos los registros médicos
CREATE OR ALTER PROCEDURE sp_GetAllMedicalRecords
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        mr.Id,
        mr.AppointmentId,
        a.AppointmentDate,
        mr.PatientId,
        p.FirstName + ' ' + p.LastName AS PatientName,
        mr.DoctorId,
        d.FirstName + ' ' + d.LastName AS DoctorName,
        d.Specialty AS DoctorSpecialty,
        mr.Diagnosis,
        mr.Treatment,
        mr.Prescription,
        mr.Notes,
        mr.RecordDate
    FROM MedicalRecords mr
    INNER JOIN Appointments a ON mr.AppointmentId = a.Id
    INNER JOIN Patients p ON mr.PatientId = p.Id
    INNER JOIN Doctors d ON mr.DoctorId = d.Id
    ORDER BY mr.RecordDate DESC;
END
GO