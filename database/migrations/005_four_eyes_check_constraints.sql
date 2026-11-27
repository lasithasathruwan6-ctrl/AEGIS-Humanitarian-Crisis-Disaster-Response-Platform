-- Migration 005: Four-eyes (two-person) CHECK constraints
-- Ensures the second approver is never the same user as the decider/requester.
-- Target: SQL Server 2022 (dbo.AiRecommendations, dbo.RoleChangeRequests)

IF OBJECT_ID(N'dbo.AiRecommendations', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_AiRecommendations_Approver_Differs_From_Decider'
          AND parent_object_id = OBJECT_ID(N'dbo.AiRecommendations')
    )
    BEGIN
        ALTER TABLE dbo.AiRecommendations WITH CHECK
            ADD CONSTRAINT CK_AiRecommendations_Approver_Differs_From_Decider
            CHECK (ApprovedBy IS NULL OR ApprovedBy <> DecidedBy);
    END;
END;
GO

IF OBJECT_ID(N'dbo.RoleChangeRequests', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_RoleChangeRequests_Approver_Differs_From_Requester'
          AND parent_object_id = OBJECT_ID(N'dbo.RoleChangeRequests')
    )
    BEGIN
        ALTER TABLE dbo.RoleChangeRequests WITH CHECK
            ADD CONSTRAINT CK_RoleChangeRequests_Approver_Differs_From_Requester
            CHECK (ApprovedBy IS NULL OR ApprovedBy <> RequestedBy);
    END;
END;
GO
