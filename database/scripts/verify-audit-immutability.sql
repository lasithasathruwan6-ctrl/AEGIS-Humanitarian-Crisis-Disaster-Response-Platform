-- Verification Script: Ensure dbo.AuditEntries blocks UPDATE, DELETE, and TRUNCATE
-- Target: SQL Server 2022

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.AuditEntries', N'U') IS NULL
BEGIN
    THROW 50001, 'dbo.AuditEntries table does not exist.', 1;
END;

DECLARE @Blocked BIT = 0;

-- 1. Verify UPDATE is blocked
BEGIN TRY
    UPDATE dbo.AuditEntries SET Action = N'TAMPERED' WHERE 1 = 0;
    -- Even with a matching or non-matching predicate, INSTEAD OF UPDATE trigger should raise
    UPDATE TOP (1) dbo.AuditEntries SET Action = Action;
    THROW 50002, 'Audit immutability check failed: UPDATE was not blocked.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50002 THROW;
    SET @Blocked = 1;
END CATCH;

-- 2. Verify DELETE is blocked
BEGIN TRY
    DELETE TOP (1) FROM dbo.AuditEntries;
    THROW 50003, 'Audit immutability check failed: DELETE was not blocked.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50003 THROW;
    SET @Blocked = 1;
END CATCH;

-- 3. Verify TRUNCATE is blocked (permission denied for app role + FK/trigger protection)
BEGIN TRY
    TRUNCATE TABLE dbo.AuditEntries;
    THROW 50004, 'Audit immutability check failed: TRUNCATE was not blocked.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 50004 THROW;
    SET @Blocked = 1;
END CATCH;

PRINT 'OK: dbo.AuditEntries blocked UPDATE, DELETE, and TRUNCATE.';
GO
