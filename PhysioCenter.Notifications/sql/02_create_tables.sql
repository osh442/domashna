SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PhysioNotifications;
GO

CREATE TABLE dbo.Notifications (
    NotificationId  INT IDENTITY(1,1) NOT NULL,
    EventType       VARCHAR(30)       NOT NULL,
    SessionId       INT               NOT NULL,
    PatientId       INT               NOT NULL,
    PatientName     NVARCHAR(101)     NOT NULL,
    TherapistId     INT               NOT NULL,
    TherapistName   NVARCHAR(101)     NOT NULL,
    SessionDateTime DATETIME2(0)      NOT NULL,
    SessionStatus   VARCHAR(20)       NOT NULL,
    Message         NVARCHAR(500)     NOT NULL,
    ReceivedAt      DATETIME2(0)      NOT NULL
        CONSTRAINT DF_Notifications_ReceivedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (NotificationId),
    CONSTRAINT CK_Notifications_EventType
        CHECK (EventType IN ('SessionCreated', 'SessionUpdated', 'SessionDeleted'))
);
GO

CREATE INDEX IX_Notifications_SessionId ON dbo.Notifications (SessionId);
CREATE INDEX IX_Notifications_PatientId ON dbo.Notifications (PatientId);
CREATE INDEX IX_Notifications_ReceivedAt ON dbo.Notifications (ReceivedAt DESC);
GO
