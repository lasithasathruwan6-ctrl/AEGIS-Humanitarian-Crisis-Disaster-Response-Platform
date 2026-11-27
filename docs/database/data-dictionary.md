# Database Data Dictionary

Column-level reference for `CrisisPlatform` (**SQL Server 2022**, schema `dbo`).

---

## `Users`

| Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `Id` | `UNIQUEIDENTIFIER` | No | `PK` | Surrogate user ID |
| `FullName` | `NVARCHAR(200)` | No | — | User's display name |
| `Email` | `NVARCHAR(320)` | No | `UQ` | Login & notification email |
| `NicHash` | `CHAR(64)` | Yes | `UX_Users_NicHash` (filtered `DeletedAt IS NULL`) | Keyed HMAC-SHA256 of canonical NIC |
| `EncryptedNic` | `VARBINARY(MAX)` | Yes | — | AES-256-GCM ciphertext of NIC |
| `Role` | `NVARCHAR(32)` | No | `FK -> Roles.Name` | `Citizen`, `Volunteer`, `Staff`, `Coordinator`, or `Admin` |
| `IsLocked` | `BIT` | No | Default `0` | Account lockout flag |
| `FailedLoginAttempts` | `INT` | No | Default `0` | Consecutive failed login counter |
| `DeletedAt` | `DATETIMEOFFSET(7)` | Yes | — | Soft-delete timestamp (UTC) |

## `AidRequests`

| Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `Id` | `UNIQUEIDENTIFIER` | No | `PK` | Surrogate aid request ID |
| `CitizenId` | `UNIQUEIDENTIFIER` | No | `FK -> Users.Id (NO ACTION)`, `IX_AidRequests_CitizenId_CreatedAt` | Requesting citizen |
| `NicHash` | `CHAR(64)` | No | — | NIC fingerprint at submission |
| `District` | `NVARCHAR(64)` | No | `IX_AidRequests_Status_District_CreatedAt`, `IX_AidRequests_District_CreatedAt` | One of 25 Sri Lankan districts |
| `NeedType` | `NVARCHAR(32)` | No | — | `Food`, `Water`, `Medical`, `Shelter`, `Evacuation`, `Other` |
| `Urgency` | `INT` | No | `CK_AidRequests_Urgency_Range` (`1..5`) | Triage urgency (5 = critical) |
| `Description` | `NVARCHAR(2000)` | No | — | Citizen description of need |
| `ContactPhone` | `NVARCHAR(32)` | No | — | Responder contact number |
| `Status` | `NVARCHAR(32)` | No | `IX_AidRequests_Status_District_CreatedAt` | `Pending`, `InProgress`, `Approved`, `Completed`, `Rejected` |
| `IdempotencyKey` | `NVARCHAR(128)` | Yes | `UX_AidRequests_IdempotencyKey` (filtered) | Client deduplication key |
| `RowVersion` | `ROWVERSION` | No | Concurrency token | Optimistic concurrency version |
| `CreatedAt` | `DATETIMEOFFSET(7)` | No | — | UTC submission timestamp |
| `DeletedAt` | `DATETIMEOFFSET(7)` | Yes | — | Soft-delete timestamp (UTC) |

## `RequestConversations` & `RequestMessages`

| Table.Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `RequestConversations.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Conversation thread ID |
| `RequestConversations.RequestId` | `UNIQUEIDENTIFIER` | No | `UX_RequestConversations_RequestId` | Parent aid request |
| `RequestConversations.UpdatedAt` | `DATETIMEOFFSET(7)` | No | `IX_RequestConversations_UpdatedAt` | Latest message timestamp |
| `RequestMessages.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Message ID |
| `RequestMessages.ConversationId` | `UNIQUEIDENTIFIER` | No | `IX_RequestMessages_ConversationId_SentAt` | Parent conversation |
| `RequestMessages.RequestId` | `UNIQUEIDENTIFIER` | No | `IX_RequestMessages_RequestId_SentAt` | Parent aid request |
| `RequestMessages.SenderId` | `UNIQUEIDENTIFIER` | No | `IX_RequestMessages_SenderId_SentAt` | Author user ID |
| `RequestMessages.Body` | `NVARCHAR(2000)` | No | — | Message text |
| `RequestMessages.SentAt` | `DATETIMEOFFSET(7)` | No | — | UTC send timestamp |

## `AiRecommendations` & `RoleChangeRequests`

| Table.Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `AiRecommendations.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Recommendation ID |
| `AiRecommendations.RequestId` | `UNIQUEIDENTIFIER` | No | `FK -> AidRequests.Id (NO ACTION)` | Target aid request |
| `AiRecommendations.DecidedBy` | `UNIQUEIDENTIFIER` | Yes | — | Staff member proposing decision |
| `AiRecommendations.ApprovedBy` | `UNIQUEIDENTIFIER` | Yes | `CK_AiRecommendations_Approver_Differs_From_Decider` | Second approver (`<> DecidedBy`) |
| `RoleChangeRequests.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Role-change request ID |
| `RoleChangeRequests.TargetUserId` | `UNIQUEIDENTIFIER` | No | `FK -> Users.Id (NO ACTION)` | User being promoted |
| `RoleChangeRequests.RequestedBy` | `UNIQUEIDENTIFIER` | No | `FK -> Users.Id (NO ACTION)` | Initiating Admin |
| `RoleChangeRequests.ApprovedBy` | `UNIQUEIDENTIFIER` | Yes | `CK_RoleChangeRequests_Approver_Differs_From_Requester` | Second Admin (`<> RequestedBy`) |

## `Shelters` & `Resources`

| Table.Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `Shelters.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Shelter ID |
| `Shelters.Capacity` | `INT` | No | `CK_Shelters_Capacity_NonNegative` (`>= 0`) | Maximum shelter capacity |
| `Shelters.CurrentOccupancy` | `INT` | No | `CK_Shelters_Occupancy_Range` (`0..Capacity`) | Current occupant count |
| `Shelters.RowVersion` | `ROWVERSION` | No | Concurrency token | Optimistic concurrency version |
| `Resources.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Resource inventory ID |
| `Resources.Quantity` | `INT` | No | `CK_Resources_Quantity_NonNegative` (`>= 0`) | Available unit count |
| `Resources.RowVersion` | `ROWVERSION` | No | Concurrency token | Optimistic concurrency version |

## `OutboxMessages` & `AuditEntries`

| Table.Column | Type | Nullable | Constraints / Indexes | Description |
|---|---|---:|---|---|
| `OutboxMessages.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Outbox message ID |
| `OutboxMessages.IdempotencyKey` | `NVARCHAR(128)` | Yes | `UX_OutboxMessages_IdempotencyKey` | Delivery deduplication key |
| `OutboxMessages.Status` | `NVARCHAR(32)` | No | — | `Pending`, `Delivered`, `DeadLettered` |
| `OutboxMessages.SentAt` | `DATETIMEOFFSET(7)` | Yes | — | UTC delivery timestamp (30-day retention) |
| `AuditEntries.Id` | `UNIQUEIDENTIFIER` | No | `PK` | Audit entry ID |
| `AuditEntries.PrevHash` | `CHAR(64)` | No | `TR_AuditEntries_BlockUpdate/Delete` | Previous row's `EntryHash` |
| `AuditEntries.EntryHash` | `CHAR(64)` | No | `TR_AuditEntries_BlockUpdate/Delete` | SHA-256 hash of row + `PrevHash` |
