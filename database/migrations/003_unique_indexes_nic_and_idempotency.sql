-- Migration 003: Unique indexes for NIC hash and idempotency keys
-- Target: SQL Server 2022 (dbo.Users, dbo.AidRequests, dbo.OutboxMessages)

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
BEGIN
    -- Prevent duplicate active accounts per keyed HMAC-SHA256 NIC hash
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_Users_NicHash'
          AND object_id = OBJECT_ID(N'dbo.Users')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_Users_NicHash
            ON dbo.Users (NicHash ASC)
            WHERE NicHash IS NOT NULL AND DeletedAt IS NULL;
    END;
END;
GO

IF OBJECT_ID(N'dbo.AidRequests', N'U') IS NOT NULL
BEGIN
    -- Enforce unique Idempotency-Key across non-deleted aid requests
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_AidRequests_IdempotencyKey'
          AND object_id = OBJECT_ID(N'dbo.AidRequests')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_AidRequests_IdempotencyKey
            ON dbo.AidRequests (IdempotencyKey ASC)
            WHERE IdempotencyKey IS NOT NULL AND DeletedAt IS NULL;
    END;
END;
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NOT NULL
BEGIN
    -- Deduplicate transactional outbox messages by idempotency key
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_OutboxMessages_IdempotencyKey'
          AND object_id = OBJECT_ID(N'dbo.OutboxMessages')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_OutboxMessages_IdempotencyKey
            ON dbo.OutboxMessages (IdempotencyKey ASC)
            WHERE IdempotencyKey IS NOT NULL;
    END;
END;
GO
