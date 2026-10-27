-- 02_create_tables.sql
-- Entity tables, primary keys, foreign keys, unique / check / default constraints.
--
-- Relationships:
--   Patients   1 --- N TherapyPlans     (a patient can have several therapy plans)
--   Therapists 1 --- N TherapyPlans     (the therapist who owns the plan)
--   TherapyPlans 1 --- N Sessions       (a plan is delivered in sessions)
--   Therapists 1 --- N Sessions         (the therapist who runs the session)
--   Sessions   1 --- N ProgressNotes    (notes written after a session)
--
-- Delete rules:
--   Plan -> Sessions -> ProgressNotes are CASCADE (they are owned by the plan).
--   Patient / Therapist -> anything else is NO ACTION, so medical history cannot
--   be wiped by deleting a patient or therapist by mistake. Only one cascade path
--   reaches each table, so SQL Server does not complain about "multiple cascade paths".

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PhysioCenter;
GO

-- ---------------------------------------------------------------------------
-- Patients
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.Patients (
    PatientId  INT IDENTITY(1,1) NOT NULL,
    FirstName  NVARCHAR(50)      NOT NULL,
    LastName   NVARCHAR(50)      NOT NULL,
    BirthDate  DATE              NOT NULL,
    Gender     CHAR(1)           NOT NULL,
    Phone      NVARCHAR(20)      NOT NULL,
    Email      NVARCHAR(100)     NULL,
    Address    NVARCHAR(200)     NULL,
    CreatedAt  DATETIME2(0)      NOT NULL
        CONSTRAINT DF_Patients_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Patients PRIMARY KEY CLUSTERED (PatientId),
    CONSTRAINT CK_Patients_Gender    CHECK (Gender IN ('M', 'F', 'O')),
    CONSTRAINT CK_Patients_BirthDate CHECK (BirthDate >= '19000101' AND BirthDate <= CAST(GETDATE() AS DATE)),
    CONSTRAINT CK_Patients_Email     CHECK (Email IS NULL OR Email LIKE '%_@_%._%')
);
GO

-- ---------------------------------------------------------------------------
-- Therapists
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.Therapists (
    TherapistId    INT IDENTITY(1,1) NOT NULL,
    FirstName      NVARCHAR(50)      NOT NULL,
    LastName       NVARCHAR(50)      NOT NULL,
    Specialization NVARCHAR(100)     NOT NULL,
    Phone          NVARCHAR(20)      NULL,
    Email          NVARCHAR(100)     NOT NULL,
    IsActive       BIT               NOT NULL
        CONSTRAINT DF_Therapists_IsActive DEFAULT 1,
    CONSTRAINT PK_Therapists PRIMARY KEY CLUSTERED (TherapistId),
    CONSTRAINT UQ_Therapists_Email UNIQUE (Email),
    CONSTRAINT CK_Therapists_Email CHECK (Email LIKE '%_@_%._%')
);
GO

-- ---------------------------------------------------------------------------
-- TherapyPlans
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.TherapyPlans (
    PlanId          INT IDENTITY(1,1) NOT NULL,
    PatientId       INT               NOT NULL,
    TherapistId     INT               NOT NULL,
    Diagnosis       NVARCHAR(200)     NOT NULL,
    Goal            NVARCHAR(500)     NULL,
    StartDate       DATE              NOT NULL,
    EndDate         DATE              NULL,
    PlannedSessions SMALLINT          NOT NULL,
    Status          VARCHAR(20)       NOT NULL
        CONSTRAINT DF_TherapyPlans_Status DEFAULT 'Active',
    CreatedAt       DATETIME2(0)      NOT NULL
        CONSTRAINT DF_TherapyPlans_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TherapyPlans PRIMARY KEY CLUSTERED (PlanId),
    CONSTRAINT FK_TherapyPlans_Patients
        FOREIGN KEY (PatientId)   REFERENCES dbo.Patients (PatientId),
    CONSTRAINT FK_TherapyPlans_Therapists
        FOREIGN KEY (TherapistId) REFERENCES dbo.Therapists (TherapistId),
    CONSTRAINT CK_TherapyPlans_Dates    CHECK (EndDate >= StartDate),
    CONSTRAINT CK_TherapyPlans_Planned  CHECK (PlannedSessions > 0),
    CONSTRAINT CK_TherapyPlans_Status   CHECK (Status IN ('Active', 'Completed', 'Cancelled', 'OnHold'))
);
GO

-- ---------------------------------------------------------------------------
-- Sessions
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.Sessions (
    SessionId       INT IDENTITY(1,1) NOT NULL,
    PlanId          INT               NOT NULL,
    TherapistId     INT               NOT NULL,
    SessionDateTime DATETIME2(0)      NOT NULL,
    DurationMinutes SMALLINT          NOT NULL
        CONSTRAINT DF_Sessions_Duration DEFAULT 45,
    TreatmentType   NVARCHAR(100)     NOT NULL,
    Status          VARCHAR(20)       NOT NULL
        CONSTRAINT DF_Sessions_Status DEFAULT 'Scheduled',
    CONSTRAINT PK_Sessions PRIMARY KEY CLUSTERED (SessionId),
    CONSTRAINT FK_Sessions_TherapyPlans
        FOREIGN KEY (PlanId)      REFERENCES dbo.TherapyPlans (PlanId) ON DELETE CASCADE,
    CONSTRAINT FK_Sessions_Therapists
        FOREIGN KEY (TherapistId) REFERENCES dbo.Therapists (TherapistId),
    CONSTRAINT CK_Sessions_Duration CHECK (DurationMinutes BETWEEN 5 AND 240),
    CONSTRAINT CK_Sessions_Status   CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled', 'NoShow'))
);
GO

-- ---------------------------------------------------------------------------
-- ProgressNotes
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.ProgressNotes (
    NoteId        INT IDENTITY(1,1) NOT NULL,
    SessionId     INT               NOT NULL,
    NoteText      NVARCHAR(2000)    NOT NULL,
    PainLevel     TINYINT           NOT NULL,
    MobilityScore TINYINT           NULL,
    CreatedAt     DATETIME2(0)      NOT NULL
        CONSTRAINT DF_ProgressNotes_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProgressNotes PRIMARY KEY CLUSTERED (NoteId),
    CONSTRAINT FK_ProgressNotes_Sessions
        FOREIGN KEY (SessionId) REFERENCES dbo.Sessions (SessionId) ON DELETE CASCADE,
    CONSTRAINT CK_ProgressNotes_Text     CHECK (LEN(NoteText) > 0),
    CONSTRAINT CK_ProgressNotes_Pain     CHECK (PainLevel BETWEEN 0 AND 10),
    CONSTRAINT CK_ProgressNotes_Mobility CHECK (MobilityScore BETWEEN 0 AND 10)
);
GO

PRINT 'Tables created: Patients, Therapists, TherapyPlans, Sessions, ProgressNotes.';
GO
