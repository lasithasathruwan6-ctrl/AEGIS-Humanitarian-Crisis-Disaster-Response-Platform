-- Migration 006: SQL Server INSTEAD OF triggers for append-only AuditEntries
-- Blocks UPDATE and DELETE on dbo.AuditEntries.
-- TRUNCATE TABLE is blocked by denying ALTER/CONTROL to the application role.
-- Target: SQL Server 2022 (dbo.AuditEntries)

IF OBJECT_ID(N'dbo.AuditEntries', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.TR_AuditEntries_BlockUpdate', N'TR') IS NULL
    BEGIN
        EXEC(N'
            CREATE TRIGGER dbo.TR_AuditEntries_BlockUpdate
            ON dbo.AuditEntries
            INSTEAD OF UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;
                THROW 51000, ''AuditEntries is append-only: UPDATE is prohibited.'', 1;
            END
        ');
    END;

    IF OBJECT_ID(N'dbo.TR_AuditEntries_BlockDelete', N'TR') IS NULL
    BEGIN
        EXEC(N'
            CREATE TRIGGER dbo.TR_AuditEntries_BlockDelete
            ON dbo.AuditEntries
            INSTEAD OF DELETE
            AS
            BEGIN
                SET NOCOUNT ON;
                THROW 51001, ''AuditEntries is append-only: DELETE is prohibited.'', 1;
            END
        ');
    END;
END;
GO
