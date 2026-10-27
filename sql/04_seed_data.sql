-- 04_seed_data.sql
-- Sample data so the database can be demonstrated right away.
-- Assumes a freshly created database (identity values start at 1).

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PhysioCenter;
GO

-- Therapists: 1 = Dimitrov, 2 = Stoyanova, 3 = Iliev
INSERT INTO dbo.Therapists (FirstName, LastName, Specialization, Phone, Email) VALUES
(N'Georgi',  N'Dimitrov', N'Sports rehabilitation',     N'+359 888 100 001', N'g.dimitrov@physio.example'),
(N'Elena',   N'Stoyanova', N'Neurological rehabilitation', N'+359 888 100 002', N'e.stoyanova@physio.example'),
(N'Nikolay', N'Iliev',    N'Manual therapy',            N'+359 888 100 003', N'n.iliev@physio.example');

-- Patients: 1..5 (patient 3 has no e-mail)
INSERT INTO dbo.Patients (FirstName, LastName, BirthDate, Gender, Phone, Email, Address) VALUES
(N'Ivan',      N'Petrov',    '1985-03-12', 'M', N'+359 877 200 001', N'ivan.petrov@mail.example',     N'12 Vitosha Blvd, Sofia'),
(N'Maria',     N'Georgieva', '1992-07-25', 'F', N'+359 877 200 002', N'maria.georgieva@mail.example', N'5 Han Asparuh St, Plovdiv'),
(N'Petar',     N'Kolev',     '1968-11-02', 'M', N'+359 877 200 003', NULL,                            N'27 Slivnitsa Blvd, Varna'),
(N'Desislava', N'Angelova',  '2001-01-30', 'F', N'+359 877 200 004', N'desi.angelova@mail.example',   NULL),
(N'Stefan',    N'Marinov',   '1979-09-14', 'M', N'+359 877 200 005', N'stefan.marinov@mail.example',  N'3 Rakovski St, Burgas');

-- Therapy plans: 1..5
INSERT INTO dbo.TherapyPlans (PatientId, TherapistId, Diagnosis, Goal, StartDate, EndDate, PlannedSessions, Status) VALUES
(1, 1, N'ACL reconstruction (post-operative)', N'Restore full knee range of motion and strength', '2026-09-01', '2026-11-30', 12, 'Active'),
(2, 3, N'Chronic lower back pain',             N'Reduce pain and return to normal daily activity', '2026-08-15', '2026-10-15', 10, 'Completed'),
(3, 2, N'Post-stroke right-side weakness',     N'Improve walking and right hand grip',             '2026-10-01', '2027-01-31', 20, 'Active'),
(4, 3, N'Cervical spondylosis',                N'Relieve neck stiffness and headaches',            '2026-10-10', '2026-11-10',  8, 'Active'),
(5, 1, N'Ankle sprain (grade 2)',              N'Return to running',                               '2026-09-20', '2026-10-04',  6, 'Cancelled');

-- Sessions: 1..16 (the therapist of a session is the therapist of its plan)
INSERT INTO dbo.Sessions (PlanId, TherapistId, SessionDateTime, DurationMinutes, TreatmentType, Status) VALUES
(1, 1, '2026-09-03T10:00:00', 45, N'Kinesitherapy',              'Completed'),  --  1
(1, 1, '2026-09-10T10:00:00', 45, N'Kinesitherapy',              'Completed'),  --  2
(1, 1, '2026-09-17T10:00:00', 45, N'Strength training',          'Completed'),  --  3
(1, 1, '2026-10-15T10:00:00', 45, N'Strength training',          'Completed'),  --  4
(1, 1, '2026-10-22T10:00:00', 45, N'Strength training',          'Scheduled'),  --  5
(2, 3, '2026-08-18T14:00:00', 45, N'Manual therapy',             'Completed'),  --  6
(2, 3, '2026-09-01T14:00:00', 45, N'Manual therapy',             'Completed'),  --  7
(2, 3, '2026-09-29T14:00:00', 45, N'Therapeutic exercise',       'Completed'),  --  8
(3, 2, '2026-10-02T09:00:00', 60, N'Neuromuscular re-education', 'Completed'),  --  9
(3, 2, '2026-10-09T09:00:00', 60, N'Neuromuscular re-education', 'Completed'),  -- 10
(3, 2, '2026-10-23T09:00:00', 60, N'Neuromuscular re-education', 'Scheduled'),  -- 11
(4, 3, '2026-10-12T14:00:00', 45, N'Manual therapy',             'Completed'),  -- 12
(4, 3, '2026-10-19T14:00:00', 45, N'Manual therapy',             'Cancelled'),  -- 13
(4, 3, '2026-10-26T14:00:00', 45, N'Manual therapy',             'Scheduled'),  -- 14
(5, 1, '2026-09-21T11:00:00', 30, N'Taping and mobilisation',    'Completed'),  -- 15
(5, 1, '2026-09-28T11:00:00', 30, N'Taping and mobilisation',    'Cancelled');  -- 16

-- Progress notes (only completed sessions have notes)
INSERT INTO dbo.ProgressNotes (SessionId, NoteText, PainLevel, MobilityScore) VALUES
( 1, N'Initial assessment. Swelling present, flexion limited to 70 degrees.',        7, 3),
( 2, N'Flexion improved to 85 degrees. Started gentle quadriceps activation.',       6, 4),
( 3, N'Flexion 100 degrees. Added stationary bike, 10 minutes.',                     5, 5),
( 4, N'Flexion 120 degrees. Walking without crutches. Pain only on stairs.',         3, 7),
( 6, N'Initial assessment. Stiffness in the lumbar region on movement.',             7, 4),
( 7, N'Pain reduced after manual therapy. Home exercises assigned.',                 5, 5),
( 8, N'Final session. Pain-free in daily activities, plan completed.',               1, 9),
( 9, N'Baseline: reduced grip strength in right hand, walks with assistance.',       6, 3),
(10, N'Better weight bearing on the right leg. Balance exercises added.',            5, 4),
(12, N'Neck stiffness and headaches. Mobilisation and posture advice given.',        6, 5),
(15, N'Grade 2 sprain. RICE protocol, taping applied.',                              6, 4);
GO

PRINT 'Sample data inserted.';
GO
