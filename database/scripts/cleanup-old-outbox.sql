-- Maintenance Script: Batched cleanup of delivered OutboxMessages older than retention window
-- Target: SQL Server 2022 (dbo.OutboxMessages)

SET NOCOUNT ON;

DECLARE @RetentionDays INT = 30;
DECLARE @BatchSize INT = 1000;
DECLARE @Cutoff DATETIMEOFFSET(7) = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());
DECLARE @RowsDeleted INT = 1;
DECLARE @TotalDeleted INT = 0;

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    THROW 50010, 'dbo.OutboxMessages table does not exist.', 1;
END;

WHILE @RowsDeleted > 0
BEGIN
    DELETE TOP (@BatchSize)
    FROM dbo.OutboxMessages
    WHERE Status = N'Delivered'
      AND SentAt IS NOT NULL
      AND SentAt < @Cutoff;

    SET @RowsDeleted = @@ROWCOUNT;
    SET @TotalDeleted += @RowsDeleted;
END;

PRINT CONCAT('Outbox cleanup complete. Deleted ', @TotalDeleted, ' delivered rows older than ', @RetentionDays, ' days.');
GO
