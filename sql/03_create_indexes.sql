-- 03_create_indexes.sql
-- Primary keys already have clustered indexes. This script adds:
--   * indexes on every foreign key column (fast joins, fast cascade deletes)
--   * indexes for common searches (patient name, session date)
--   * filtered unique indexes that enforce business rules

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PhysioCenter;
GO

-- Foreign key indexes
CREATE NONCLUSTERED INDEX IX_TherapyPlans_PatientId
    ON dbo.TherapyPlans (PatientId);

CREATE NONCLUSTERED INDEX IX_TherapyPlans_TherapistId
    ON dbo.TherapyPlans (TherapistId);

CREATE NONCLUSTERED INDEX IX_Sessions_PlanId
    ON dbo.Sessions (PlanId);

-- Also serves "a therapist's schedule" queries
CREATE NONCLUSTERED INDEX IX_Sessions_TherapistId_SessionDateTime
    ON dbo.Sessions (TherapistId, SessionDateTime);

-- Covers pain / mobility progress queries without touching the table
CREATE NONCLUSTERED INDEX IX_ProgressNotes_SessionId
    ON dbo.ProgressNotes (SessionId)
    INCLUDE (PainLevel, MobilityScore, CreatedAt);

-- Search indexes
CREATE NONCLUSTERED INDEX IX_Patients_LastName_FirstName
    ON dbo.Patients (LastName, FirstName);

CREATE NONCLUSTERED INDEX IX_Sessions_SessionDateTime
    ON dbo.Sessions (SessionDateTime)
    INCLUDE (Status);

-- Business rule: a patient e-mail is unique, but e-mail is optional
-- (a normal UNIQUE constraint would allow only one NULL).
CREATE UNIQUE NONCLUSTERED INDEX UX_Patients_Email
    ON dbo.Patients (Email)
    WHERE Email IS NOT NULL;

-- Business rule: a therapist cannot have two active sessions at the same time.
-- Cancelled sessions are ignored so the slot can be booked again.
CREATE UNIQUE NONCLUSTERED INDEX UX_Sessions_Therapist_Start
    ON dbo.Sessions (TherapistId, SessionDateTime)
    WHERE Status <> 'Cancelled';
GO

PRINT 'Indexes created.';
GO
