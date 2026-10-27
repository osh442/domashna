-- 00_reset.sql
-- Drops the PhysioCenter database so the other scripts can be re-run from scratch.
-- Optional: only run it when you want to start over.

USE master;
GO

IF DB_ID(N'PhysioCenter') IS NOT NULL
BEGIN
    ALTER DATABASE PhysioCenter SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PhysioCenter;
END
GO

PRINT 'Database PhysioCenter dropped (if it existed).';
GO
