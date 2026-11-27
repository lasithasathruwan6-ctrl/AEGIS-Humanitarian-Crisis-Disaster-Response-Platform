-- Seed 001: Reference Data (5 Roles, 6 NeedCategories, 25 Sri Lankan Districts)
-- Idempotent T-SQL MERGE script for SQL Server 2022

SET NOCOUNT ON;

-- 1. Roles (5)
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
BEGIN
    MERGE dbo.Roles AS target
    USING (VALUES
        (1, N'Citizen',     N'Registered citizen submitting and tracking aid requests'),
        (2, N'Volunteer',   N'Verified field volunteer eligible for request assignment'),
        (3, N'Staff',       N'Organisation staff member triaging requests and shelters'),
        (4, N'Coordinator', N'District/regional coordinator managing resources and AI approvals'),
        (5, N'Admin',       N'Platform administrator managing staff invitations and role changes')
    ) AS source (Id, Name, Description)
    ON target.Name = source.Name
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, Description) VALUES (source.Id, source.Name, source.Description);
END;
GO

-- 2. Need Categories (6)
IF OBJECT_ID(N'dbo.NeedCategories', N'U') IS NOT NULL
BEGIN
    MERGE dbo.NeedCategories AS target
    USING (VALUES
        (1, 'FOOD',       N'Food',       1),
        (2, 'WATER',      N'Water',      1),
        (3, 'MEDICAL',    N'Medical',    1),
        (4, 'SHELTER',    N'Shelter',    1),
        (5, 'EVACUATION', N'Evacuation', 1),
        (6, 'OTHER',      N'Other',      1)
    ) AS source (Id, Code, DisplayName, IsActive)
    ON target.Code = source.Code
    WHEN MATCHED THEN
        UPDATE SET DisplayName = source.DisplayName, IsActive = source.IsActive
    WHEN NOT MATCHED THEN
        INSERT (Id, Code, DisplayName, IsActive) VALUES (source.Id, source.Code, source.DisplayName, source.IsActive);
END;
GO

-- 3. Sri Lankan Administrative Districts (25)
IF OBJECT_ID(N'dbo.Districts', N'U') IS NOT NULL
BEGIN
    MERGE dbo.Districts AS target
    USING (VALUES
        ( 1, 'LK-11', N'Colombo',      N'Western',       6.9271, 79.8612),
        ( 2, 'LK-12', N'Gampaha',      N'Western',       7.0873, 80.0144),
        ( 3, 'LK-13', N'Kalutara',     N'Western',       6.5854, 79.9607),
        ( 4, 'LK-21', N'Kandy',        N'Central',       7.2906, 80.6337),
        ( 5, 'LK-22', N'Matale',       N'Central',       7.4675, 80.6234),
        ( 6, 'LK-23', N'Nuwara Eliya', N'Central',       6.9497, 80.7891),
        ( 7, 'LK-31', N'Galle',        N'Southern',      6.0535, 80.2210),
        ( 8, 'LK-32', N'Matara',       N'Southern',      5.9549, 80.5550),
        ( 9, 'LK-33', N'Hambantota',   N'Southern',      6.1429, 81.1212),
        (10, 'LK-41', N'Jaffna',       N'Northern',      9.6615, 80.0255),
        (11, 'LK-42', N'Kilinochchi',  N'Northern',      9.3803, 80.3770),
        (12, 'LK-43', N'Mannar',       N'Northern',      8.9810, 79.9044),
        (13, 'LK-44', N'Vavuniya',     N'Northern',      8.7514, 80.4971),
        (14, 'LK-45', N'Mullaitivu',   N'Northern',      9.2671, 80.8142),
        (15, 'LK-51', N'Batticaloa',   N'Eastern',       7.7310, 81.6747),
        (16, 'LK-52', N'Ampara',       N'Eastern',       7.2975, 81.6724),
        (17, 'LK-53', N'Trincomalee',  N'Eastern',       8.5874, 81.2152),
        (18, 'LK-61', N'Kurunegala',   N'North Western', 7.4863, 80.3647),
        (19, 'LK-62', N'Puttalam',     N'North Western', 8.0408, 79.8394),
        (20, 'LK-71', N'Anuradhapura', N'North Central', 8.3114, 80.4037),
        (21, 'LK-72', N'Polonnaruwa',  N'North Central', 7.9403, 81.0188),
        (22, 'LK-81', N'Badulla',      N'Uva',           6.9934, 81.0550),
        (23, 'LK-82', N'Monaragala',   N'Uva',           6.8728, 81.3507),
        (24, 'LK-91', N'Ratnapura',    N'Sabaragamuwa',  6.7056, 80.3847),
        (25, 'LK-92', N'Kegalle',      N'Sabaragamuwa',  7.2513, 80.3464)
    ) AS source (Id, Code, Name, Province, Latitude, Longitude)
    ON target.Code = source.Code
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name, Province = source.Province, Latitude = source.Latitude, Longitude = source.Longitude
    WHEN NOT MATCHED THEN
        INSERT (Id, Code, Name, Province, Latitude, Longitude)
        VALUES (source.Id, source.Code, source.Name, source.Province, source.Latitude, source.Longitude);
END;
GO
