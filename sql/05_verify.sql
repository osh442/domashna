-- 05_verify.sql
-- Shows the structure that was created and proves the constraints work.
-- Every test runs inside a transaction that is rolled back, so no data changes.
-- Look for PASS / FAIL lines in the output.

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PhysioCenter;
GO

-- ===========================================================================
-- A. Structure
-- ===========================================================================
PRINT '--- Tables and row counts ---';
SELECT t.name AS TableName, SUM(p.rows) AS Rows
FROM sys.tables AS t
JOIN sys.partitions AS p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;
GO

PRINT '--- Foreign keys ---';
SELECT fk.name AS ForeignKey,
       OBJECT_NAME(fk.parent_object_id)     AS ChildTable,
       OBJECT_NAME(fk.referenced_object_id) AS ParentTable,
       fk.delete_referential_action_desc    AS OnDelete
FROM sys.foreign_keys AS fk
ORDER BY ChildTable, ForeignKey;
GO

PRINT '--- Check and unique constraints ---';
SELECT OBJECT_NAME(parent_object_id) AS TableName, name AS Constraint_Name, 'CHECK' AS Kind, definition AS Definition
FROM sys.check_constraints
UNION ALL
SELECT OBJECT_NAME(parent_object_id), name, 'UNIQUE', NULL
FROM sys.key_constraints
WHERE type = 'UQ'
ORDER BY TableName, Kind, Constraint_Name;
GO

PRINT '--- Indexes ---';
SELECT OBJECT_NAME(i.object_id) AS TableName, i.name AS IndexName, i.type_desc AS Type,
       i.is_primary_key AS IsPK, i.is_unique AS IsUnique, i.has_filter AS IsFiltered
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
WHERE i.type > 0
ORDER BY TableName, i.name;
GO

-- ===========================================================================
-- B. Constraint tests (all rolled back)
-- ===========================================================================
PRINT '--- Constraint tests ---';
GO

-- 1. Pain level must be 0..10
DECLARE @SessionId INT = (SELECT MIN(SessionId) FROM dbo.Sessions);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.ProgressNotes (SessionId, NoteText, PainLevel) VALUES (@SessionId, N'test', 11);
    ROLLBACK;
    PRINT N'FAIL  1. PainLevel = 11 was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() = 547 THEN N'PASS' ELSE N'FAIL' END
             + N'  1. PainLevel = 11 rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 2. A plan cannot end before it starts
DECLARE @PatientId INT = (SELECT MIN(PatientId) FROM dbo.Patients);
DECLARE @TherapistId INT = (SELECT MIN(TherapistId) FROM dbo.Therapists);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.TherapyPlans (PatientId, TherapistId, Diagnosis, StartDate, EndDate, PlannedSessions)
    VALUES (@PatientId, @TherapistId, N'test', '2026-12-31', '2026-01-01', 5);
    ROLLBACK;
    PRINT N'FAIL  2. EndDate before StartDate was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() = 547 THEN N'PASS' ELSE N'FAIL' END
             + N'  2. EndDate before StartDate rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 3. A plan needs an existing patient (foreign key)
DECLARE @TherapistId INT = (SELECT MIN(TherapistId) FROM dbo.Therapists);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.TherapyPlans (PatientId, TherapistId, Diagnosis, StartDate, PlannedSessions)
    VALUES (999999, @TherapistId, N'test', '2026-10-01', 5);
    ROLLBACK;
    PRINT N'FAIL  3. Plan for a non-existent patient was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() = 547 THEN N'PASS' ELSE N'FAIL' END
             + N'  3. Plan for a non-existent patient rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 4. Session status must be a known value
DECLARE @PlanId INT = (SELECT MIN(PlanId) FROM dbo.TherapyPlans);
DECLARE @TherapistId INT = (SELECT MIN(TherapistId) FROM dbo.Therapists);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Sessions (PlanId, TherapistId, SessionDateTime, TreatmentType, Status)
    VALUES (@PlanId, @TherapistId, '2030-01-01T08:00:00', N'test', 'Banana');
    ROLLBACK;
    PRINT N'FAIL  4. Invalid session status was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() = 547 THEN N'PASS' ELSE N'FAIL' END
             + N'  4. Invalid session status rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 5. Therapist e-mail is unique
DECLARE @Email NVARCHAR(100) = (SELECT MIN(Email) FROM dbo.Therapists);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Therapists (FirstName, LastName, Specialization, Email)
    VALUES (N'Dup', N'Licate', N'test', @Email);
    ROLLBACK;
    PRINT N'FAIL  5. Duplicate therapist e-mail was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() IN (2627, 2601) THEN N'PASS' ELSE N'FAIL' END
             + N'  5. Duplicate therapist e-mail rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 6. Patient e-mail is unique when present ...
DECLARE @Email NVARCHAR(100) = (SELECT MIN(Email) FROM dbo.Patients WHERE Email IS NOT NULL);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Patients (FirstName, LastName, BirthDate, Gender, Phone, Email)
    VALUES (N'Dup', N'Licate', '1990-01-01', 'M', N'+359 000', @Email);
    ROLLBACK;
    PRINT N'FAIL  6. Duplicate patient e-mail was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() IN (2627, 2601) THEN N'PASS' ELSE N'FAIL' END
             + N'  6. Duplicate patient e-mail rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 7. ... but several patients may have no e-mail at all
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Patients (FirstName, LastName, BirthDate, Gender, Phone, Email) VALUES
    (N'No', N'Mail1', '1990-01-01', 'M', N'+359 001', NULL),
    (N'No', N'Mail2', '1990-01-01', 'F', N'+359 002', NULL);
    ROLLBACK;
    PRINT N'PASS  7. Several patients without e-mail are allowed';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT N'FAIL  7. Patients without e-mail rejected: ' + ERROR_MESSAGE();
END CATCH;
GO

-- 8. A therapist cannot be double-booked ...
DECLARE @PlanId INT, @TherapistId INT, @When DATETIME2(0), @msg NVARCHAR(300);
SELECT TOP (1) @PlanId = PlanId, @TherapistId = TherapistId, @When = SessionDateTime
FROM dbo.Sessions WHERE Status <> 'Cancelled' ORDER BY SessionId;
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Sessions (PlanId, TherapistId, SessionDateTime, TreatmentType)
    VALUES (@PlanId, @TherapistId, @When, N'test');
    ROLLBACK;
    PRINT N'FAIL  8. Double booking was accepted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() IN (2627, 2601) THEN N'PASS' ELSE N'FAIL' END
             + N'  8. Double booking rejected (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 9. ... but a cancelled slot can be booked again
DECLARE @PlanId INT, @TherapistId INT, @When DATETIME2(0);
SELECT TOP (1) @PlanId = PlanId, @TherapistId = TherapistId, @When = SessionDateTime
FROM dbo.Sessions WHERE Status = 'Cancelled' ORDER BY SessionId;
BEGIN TRY
    BEGIN TRAN;
    INSERT INTO dbo.Sessions (PlanId, TherapistId, SessionDateTime, TreatmentType)
    VALUES (@PlanId, @TherapistId, @When, N'test');
    ROLLBACK;
    PRINT N'PASS  9. A cancelled slot can be rebooked';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT N'FAIL  9. Rebooking a cancelled slot rejected: ' + ERROR_MESSAGE();
END CATCH;
GO

-- 10. A patient with therapy plans cannot be deleted
DECLARE @PatientId INT = (SELECT TOP (1) PatientId FROM dbo.TherapyPlans ORDER BY PlanId);
DECLARE @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    DELETE FROM dbo.Patients WHERE PatientId = @PatientId;
    ROLLBACK;
    PRINT N'FAIL  10. Patient with plans was deleted';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SET @msg = CASE WHEN ERROR_NUMBER() = 547 THEN N'PASS' ELSE N'FAIL' END
             + N'  10. Deleting a patient with plans blocked (error ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + N')';
    PRINT @msg;
END CATCH;
GO

-- 11. Deleting a plan cascades to its sessions and progress notes
DECLARE @PlanId INT = (SELECT TOP (1) s.PlanId
                       FROM dbo.Sessions AS s
                       WHERE EXISTS (SELECT 1 FROM dbo.ProgressNotes AS n WHERE n.SessionId = s.SessionId)
                       ORDER BY s.PlanId);
DECLARE @NotesOfPlan INT = (SELECT COUNT(*) FROM dbo.ProgressNotes AS n
                            JOIN dbo.Sessions AS s ON s.SessionId = n.SessionId
                            WHERE s.PlanId = @PlanId);
DECLARE @TotalNotesBefore INT = (SELECT COUNT(*) FROM dbo.ProgressNotes);
DECLARE @SessionsLeft INT, @NotesRemoved INT, @msg NVARCHAR(300);
BEGIN TRY
    BEGIN TRAN;
    DELETE FROM dbo.TherapyPlans WHERE PlanId = @PlanId;
    SET @SessionsLeft = (SELECT COUNT(*) FROM dbo.Sessions WHERE PlanId = @PlanId);
    SET @NotesRemoved = @TotalNotesBefore - (SELECT COUNT(*) FROM dbo.ProgressNotes);
    ROLLBACK;
    SET @msg = CASE WHEN @SessionsLeft = 0 AND @NotesRemoved = @NotesOfPlan AND @NotesOfPlan > 0
                    THEN N'PASS' ELSE N'FAIL' END
             + N'  11. Deleting plan ' + CAST(@PlanId AS NVARCHAR(10))
             + N' removed its sessions and ' + CAST(@NotesRemoved AS NVARCHAR(10)) + N' notes (expected '
             + CAST(@NotesOfPlan AS NVARCHAR(10)) + N')';
    PRINT @msg;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT N'FAIL  11. Cascade test error: ' + ERROR_MESSAGE();
END CATCH;
GO

-- ===========================================================================
-- C. Sample report: pain level per patient over time
-- ===========================================================================
PRINT '--- Progress report: patient -> plan -> session -> note ---';
SELECT p.FirstName + N' ' + p.LastName AS Patient,
       tp.Diagnosis,
       th.LastName                     AS Therapist,
       s.SessionDateTime,
       n.PainLevel,
       n.MobilityScore
FROM dbo.ProgressNotes AS n
JOIN dbo.Sessions      AS s  ON s.SessionId   = n.SessionId
JOIN dbo.TherapyPlans  AS tp ON tp.PlanId     = s.PlanId
JOIN dbo.Patients      AS p  ON p.PatientId   = tp.PatientId
JOIN dbo.Therapists    AS th ON th.TherapistId = s.TherapistId
ORDER BY p.LastName, s.SessionDateTime;
GO
