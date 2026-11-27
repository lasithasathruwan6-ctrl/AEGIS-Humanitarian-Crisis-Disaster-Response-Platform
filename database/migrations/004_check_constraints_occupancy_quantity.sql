-- Migration 004: CHECK constraints for shelter occupancy, resource quantity, and urgency
-- Target: SQL Server 2022 (dbo.Shelters, dbo.Resources, dbo.AidRequests)

IF OBJECT_ID(N'dbo.Shelters', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_Shelters_Capacity_NonNegative'
          AND parent_object_id = OBJECT_ID(N'dbo.Shelters')
    )
    BEGIN
        ALTER TABLE dbo.Shelters WITH CHECK
            ADD CONSTRAINT CK_Shelters_Capacity_NonNegative
            CHECK (Capacity >= 0);
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_Shelters_Occupancy_Range'
          AND parent_object_id = OBJECT_ID(N'dbo.Shelters')
    )
    BEGIN
        ALTER TABLE dbo.Shelters WITH CHECK
            ADD CONSTRAINT CK_Shelters_Occupancy_Range
            CHECK (CurrentOccupancy >= 0 AND CurrentOccupancy <= Capacity);
    END;
END;
GO

IF OBJECT_ID(N'dbo.Resources', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_Resources_Quantity_NonNegative'
          AND parent_object_id = OBJECT_ID(N'dbo.Resources')
    )
    BEGIN
        ALTER TABLE dbo.Resources WITH CHECK
            ADD CONSTRAINT CK_Resources_Quantity_NonNegative
            CHECK (Quantity >= 0);
    END;
END;
GO

IF OBJECT_ID(N'dbo.AidRequests', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_AidRequests_Urgency_Range'
          AND parent_object_id = OBJECT_ID(N'dbo.AidRequests')
    )
    BEGIN
        ALTER TABLE dbo.AidRequests WITH CHECK
            ADD CONSTRAINT CK_AidRequests_Urgency_Range
            CHECK (Urgency BETWEEN 1 AND 5);
    END;
END;
GO
