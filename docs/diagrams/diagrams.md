# Diagrams — Crisis Aid Platform

All diagrams are written as [Mermaid](https://mermaid.js.org) code so they render directly in GitHub Markdown and can be diffed like source code.

---

## Class Diagram

Core domain entities and their relationships.

```mermaid
classDiagram
    direction TB

    class User {
        +Guid Id
        +string FullName
        +string Email
        +string HashedNic
        +string EncryptedNic
        +string Role
        +bool IsLocked
        +int FailedLoginAttempts
    }

    class AidRequest {
        +Guid Id
        +Guid CitizenId
        +string Type
        +string Description
        +double Latitude
        +double Longitude
        +string Status
        +DateTime CreatedAt
    }

    class RequestMessage {
        +Guid Id
        +Guid RequestId
        +Guid SenderId
        +string Body
        +DateTime SentAt
    }

    class AiRecommendation {
        +Guid Id
        +Guid RequestId
        +string SuggestedAction
        +string Status
        +Guid? ApprovedBy
        +string? Note
        +DateTime CreatedAt
    }

    class Shelter {
        +Guid Id
        +string Name
        +double Latitude
        +double Longitude
        +int Capacity
        +int CurrentOccupancy
    }

    class Volunteer {
        +Guid Id
        +Guid UserId
        +string Skills
        +bool IsAvailable
    }

    class Resource {
        +Guid Id
        +string Name
        +string Type
        +int Quantity
        +Guid ManagedByOrg
    }

    class Donation {
        +Guid Id
        +Guid DonorId
        +decimal Amount
        +string Currency
        +DateTime DonatedAt
    }

    class AuditEntry {
        +Guid Id
        +string Action
        +Guid PerformedBy
        +string TargetEntity
        +Guid TargetId
        +DateTime Timestamp
    }

    class RefreshToken {
        +Guid Id
        +Guid UserId
        +string TokenHash
        +DateTime ExpiresAt
        +bool IsRevoked
    }

    User "1" --> "*" AidRequest : submits
    User "1" --> "*" Donation : makes
    AidRequest "1" --> "*" RequestMessage : has
    AidRequest "1" --> "0..1" AiRecommendation : generates
    AiRecommendation --> User : approvedBy
    Volunteer --> User : profile of
    AidRequest --> Volunteer : assignedTo
    User "1" --> "*" RefreshToken : owns
    User "1" --> "*" AuditEntry : performs
```

---

## Sequence Diagram — Request Lifecycle with Human-in-the-Loop

```mermaid
sequenceDiagram
    actor Citizen
    participant Frontend
    participant API
    participant DB
    participant AIService
    actor Coordinator

    Citizen->>Frontend: Submit aid request
    Frontend->>API: POST /api/v1/requests (Bearer token)
    API->>DB: Insert AidRequest (status=Pending)
    API-->>Frontend: 201 { id, status }
    Frontend-->>Citizen: "Request submitted"

    API->>AIService: Trigger recommendation for request
    AIService->>DB: Insert AiRecommendation (status=Pending)
    AIService-->>API: Recommendation ready

    Coordinator->>Frontend: Open recommendation
    Frontend->>API: GET /api/v1/ai/recommendations/{id}
    API->>DB: Fetch recommendation + request
    API-->>Frontend: Recommendation details
    Frontend-->>Coordinator: Review UI

    Coordinator->>Frontend: Approve with note
    Frontend->>API: POST /api/v1/ai/recommendations/{id}/approve
    API->>DB: Update AiRecommendation (status=Approved, approvedBy)
    API->>DB: Insert AuditEntry (action=RecommendationApproved)
    API-->>Frontend: 200 { status: Approved }
    Frontend-->>Coordinator: "Decision recorded"
```

---

## Sequence Diagram — Authentication Flow (OTP + JWT)

```mermaid
sequenceDiagram
    actor User
    participant Frontend
    participant API
    participant DB
    participant SMTP

    User->>Frontend: Enter email + password
    Frontend->>API: POST /api/v1/auth/login
    API->>DB: Verify credentials, check lockout
    DB-->>API: Valid
    API->>DB: Generate & store hashed OTP
    API->>SMTP: Send OTP email
    API-->>Frontend: 200 { otpRequired: true }
    Frontend-->>User: OTP prompt

    User->>Frontend: Enter OTP
    Frontend->>API: POST /api/v1/auth/otp/verify
    API->>DB: Verify OTP hash + expiry
    DB-->>API: Valid
    API->>DB: Store refresh token hash
    API-->>Frontend: 200 { accessToken, refreshToken }
    Frontend-->>User: Logged in

    Note over Frontend,API: Access token expires (15 min)

    Frontend->>API: POST /api/v1/auth/refresh
    API->>DB: Validate + rotate refresh token
    DB-->>API: New token issued
    API-->>Frontend: 200 { accessToken, refreshToken }
```

---

## Deployment Diagram

```mermaid
flowchart LR
    subgraph Browser["User Browser"]
        FE["Next.js Frontend\n(port 3000)"]
    end

    subgraph ReverseProxy["Reverse Proxy (nginx)"]
        RP["TLS termination\n443 → 8080 / 3000"]
    end

    subgraph DockerHost["Docker Host"]
        subgraph API_Container["API Container"]
            ASPNET["ASP.NET Core 10\nport 8080 HTTP"]
        end
        subgraph DB_Container["DB Container"]
            SQL["SQL Server 2022\nport 1433"]
        end
        ASPNET -->|"ADO.NET / EF Core"| SQL
    end

    subgraph External["External Services"]
        SMTP["SMTP Relay\n(OTP / invitations)"]
    end

    Browser -->|"HTTPS 443"| RP
    RP -->|"HTTP 8080"| ASPNET
    ASPNET -->|"STARTTLS 587"| SMTP

    style DockerHost fill:#f0f4ff,stroke:#99b,stroke-width:1px
    style Browser fill:#fff8e1,stroke:#bb9,stroke-width:1px
    style External fill:#fce4ec,stroke:#c99,stroke-width:1px
```

---

## Notes

- Diagrams are rendered automatically by GitHub Markdown.
- To preview locally install the [Mermaid CLI](https://github.com/mermaid-js/mermaid-cli): `npm i -g @mermaid-js/mermaid-cli` then `mmdc -i docs/diagrams/diagrams.md`.
- When the architecture changes, update the relevant diagram in the same PR as the code change.
