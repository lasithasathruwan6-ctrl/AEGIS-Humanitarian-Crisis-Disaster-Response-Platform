-- Migration 007: Enforce safe foreign key delete behaviours
-- Prevents accidental cascade deletion of audit, financial, and aid-request history
-- and avoids SQL Server error 1785 (multiple cascade paths).
-- Target: SQL Server 2022

-- Cross-aggregate & historical FKs: always ON DELETE NO ACTION
-- 1. AidRequests.CitizenId -> Users.Id (NO ACTION)
IF OBJECT_ID(N'dbo.AidRequests', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AidRequests_Users_CitizenId')
BEGIN
    ALTER TABLE dbo.AidRequests
        ADD CONSTRAINT FK_AidRequests_Users_CitizenId
        FOREIGN KEY (CitizenId) REFERENCES dbo.Users (Id)
        ON DELETE NO ACTION;
END;
GO

-- 2. Donations.DonorId -> Users.Id (NO ACTION)
IF OBJECT_ID(N'dbo.Donations', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Donations_Users_DonorId')
BEGIN
    ALTER TABLE dbo.Donations
        ADD CONSTRAINT FK_Donations_Users_DonorId
        FOREIGN KEY (DonorId) REFERENCES dbo.Users (Id)
        ON DELETE NO ACTION;
END;
GO

-- 3. AiRecommendations.RequestId -> AidRequests.Id (NO ACTION)
IF OBJECT_ID(N'dbo.AiRecommendations', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AidRequests', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AiRecommendations_AidRequests_RequestId')
BEGIN
    ALTER TABLE dbo.AiRecommendations
        ADD CONSTRAINT FK_AiRecommendations_AidRequests_RequestId
        FOREIGN KEY (RequestId) REFERENCES dbo.AidRequests (Id)
        ON DELETE NO ACTION;
END;
GO

-- 4. RoleChangeRequests -> Users (NO ACTION on TargetUserId, RequestedBy, ApprovedBy)
IF OBJECT_ID(N'dbo.RoleChangeRequests', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RoleChangeRequests_Users_TargetUserId')
BEGIN
    ALTER TABLE dbo.RoleChangeRequests
        ADD CONSTRAINT FK_RoleChangeRequests_Users_TargetUserId
        FOREIGN KEY (TargetUserId) REFERENCES dbo.Users (Id)
        ON DELETE NO ACTION;
END;
GO

-- 5. Ephemeral token FKs: safe ON DELETE CASCADE
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RefreshTokens_Users_UserId')
BEGIN
    ALTER TABLE dbo.RefreshTokens
        ADD CONSTRAINT FK_RefreshTokens_Users_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
        ON DELETE CASCADE;
END;
GO

IF OBJECT_ID(N'dbo.OtpChallenges', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OtpChallenges_Users_UserId')
BEGIN
    ALTER TABLE dbo.OtpChallenges
        ADD CONSTRAINT FK_OtpChallenges_Users_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
        ON DELETE CASCADE;
END;
GO
