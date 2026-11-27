-- Migration 001: Indexes for AidRequests queue and filtering queries
-- Target: SQL Server 2022 (dbo.AidRequests)

IF OBJECT_ID(N'dbo.AidRequests', N'U') IS NOT NULL
BEGIN
    -- Primary queue and coordinator dashboard index: filter by Status + District, sort newest first
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_AidRequests_Status_District_CreatedAt'
          AND object_id = OBJECT_ID(N'dbo.AidRequests')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_AidRequests_Status_District_CreatedAt
            ON dbo.AidRequests (Status ASC, District ASC, CreatedAt DESC)
            INCLUDE (NeedType, Urgency)
            WHERE DeletedAt IS NULL;
    END;

    -- District-scoped listing without status filter
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_AidRequests_District_CreatedAt'
          AND object_id = OBJECT_ID(N'dbo.AidRequests')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_AidRequests_District_CreatedAt
            ON dbo.AidRequests (District ASC, CreatedAt DESC)
            INCLUDE (Status, NeedType, Urgency)
            WHERE DeletedAt IS NULL;
    END;

    -- Citizen "My Requests" lookup ordered by submission date
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_AidRequests_CitizenId_CreatedAt'
          AND object_id = OBJECT_ID(N'dbo.AidRequests')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_AidRequests_CitizenId_CreatedAt
            ON dbo.AidRequests (CitizenId ASC, CreatedAt DESC)
            INCLUDE (Status, District, NeedType, Urgency)
            WHERE DeletedAt IS NULL;
    END;
END;
GO
