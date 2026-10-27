-- 01_create_database.sql
-- Creates the PhysioCenter database (safe to run more than once).

USE master;
GO

IF DB_ID(N'PhysioCenter') IS NULL
    CREATE DATABASE PhysioCenter;
GO

PRINT 'Database PhysioCenter is ready.';
GO
