-- Migration 002: Indexes for RequestConversations and RequestMessages
-- Target: SQL Server 2022 (dbo.RequestConversations, dbo.RequestMessages)

IF OBJECT_ID(N'dbo.RequestConversations', N'U') IS NOT NULL
BEGIN
    -- One conversation thread per aid request
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_RequestConversations_RequestId'
          AND object_id = OBJECT_ID(N'dbo.RequestConversations')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_RequestConversations_RequestId
            ON dbo.RequestConversations (RequestId ASC);
    END;

    -- Staff inbox / recent activity ordering
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_RequestConversations_UpdatedAt'
          AND object_id = OBJECT_ID(N'dbo.RequestConversations')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_RequestConversations_UpdatedAt
            ON dbo.RequestConversations (UpdatedAt DESC)
            INCLUDE (RequestId);
    END;
END;
GO

IF OBJECT_ID(N'dbo.RequestMessages', N'U') IS NOT NULL
BEGIN
    -- Chronological pagination of messages inside a conversation
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_RequestMessages_ConversationId_SentAt'
          AND object_id = OBJECT_ID(N'dbo.RequestMessages')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_RequestMessages_ConversationId_SentAt
            ON dbo.RequestMessages (ConversationId ASC, SentAt ASC)
            INCLUDE (SenderId);
    END;

    -- Direct request-scoped message lookup
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_RequestMessages_RequestId_SentAt'
          AND object_id = OBJECT_ID(N'dbo.RequestMessages')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_RequestMessages_RequestId_SentAt
            ON dbo.RequestMessages (RequestId ASC, SentAt ASC)
            INCLUDE (SenderId);
    END;

    -- Sender audit / activity lookup
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_RequestMessages_SenderId_SentAt'
          AND object_id = OBJECT_ID(N'dbo.RequestMessages')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_RequestMessages_SenderId_SentAt
            ON dbo.RequestMessages (SenderId ASC, SentAt DESC);
    END;
END;
GO
