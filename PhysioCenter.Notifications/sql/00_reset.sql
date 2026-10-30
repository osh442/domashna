USE master;
GO

IF DB_ID(N'PhysioNotifications') IS NOT NULL
BEGIN
    ALTER DATABASE PhysioNotifications SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PhysioNotifications;
END
GO
