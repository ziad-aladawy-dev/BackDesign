# Target Architecture

This document describes the target state architecture of the university system, transitioning from a highly cohesive ball of mud to a **Modular Monolith** based on Domain-Driven Design (DDD) principles.

The system acts as a **Consumer/Orchestrator** over a central University Student Information System (SIS/ERP), maintaining local projections and driving value-add workflows.

---

## 1. System Context Diagram (Level 1)

This diagram illustrates how our system fits into the broader university ecosystem, interacting with end users and external systems.

```mermaid
flowchart TD
    %% Actors
    Student([Student])
    Instructor([Instructor])
    Admin([Administrators & Staff])

    %% Systems
    subgraph OurEcosystem [Our Boundary]
        PortalSystem[("University Portal System\n(Modular Monolith)")]
    end

    %% External Systems
    SIS[("University SIS / ERP\n(Source of Truth)")]
    PaymentGateway[("Payment Gateway")]
    LMS[("External LMS\n(e.g., Moodle)")]

    %% Relationships
    Student -->|Views grades, submits requests, registers| PortalSystem
    Instructor -->|Manages attendance, grading| PortalSystem
    Admin -->|Manages policies, workflows, reports| PortalSystem

    PortalSystem <-->|Syncs data, maps domain ACL| SIS
    PortalSystem -->|Processes payments| PaymentGateway
    PortalSystem -->|Integrates course content| LMS

    classDef external fill:#999,stroke:#333,color:#fff;
    classDef system fill:#1168bd,stroke:#0b4884,color:#fff;
    classDef actor fill:#08427b,stroke:#052e56,color:#fff;

    class SIS,PaymentGateway,LMS external;
    class PortalSystem system;
    class Student,Instructor,Admin actor;
```

---

## 2. Container Diagram (Level 2)

This diagram zooms into the **University Portal System** to show its high-level technical containers.

```mermaid
flowchart TD
    %% Users
    Users([Users: Students, Staff, Admins])

    subgraph PortalSystem [University Portal System]
        %% Frontend
        WebApp["Web Applications\n(Student, Admin, Staff Portals)"]

        %% API Gateway
        ApiGateway["API Gateway\n(Unified Entrypoint, Rate Limiting)"]

        %% Backend
        subgraph Backend [Modular Monolithic Backend: .NET]
            AppCore["Modular Backend System\n(All DDD Modules)"]
            SyncWorker["Background Sync Workers\n(ETL & Scheduled Jobs)"]
        end

        %% Data Stores
        Database[("Local Projection Database\n(SQL)")]
        Cache[("Distributed Cache\n(Redis)")]
        SearchEngine[("Search Engine\n(Elasticsearch)")]
    end

    %% External Systems
    SIS[("University SIS / ERP")]

    %% Relationships
    Users -->|HTTPS| WebApp
    WebApp -->|REST / GraphQL| ApiGateway
    ApiGateway --> AppCore

    AppCore --> Database
    AppCore --> Cache
    AppCore --> SearchEngine

    SyncWorker -->|Updates local projections| Database
    SyncWorker <-->|Pulls from| SIS
    AppCore <-->|Real-time API calls| SIS

    classDef container fill:#438dd5,stroke:#2d5d8c,color:#fff;
    classDef database fill:#2d5d8c,stroke:#1d3d5c,color:#fff;
    classDef external fill:#999,stroke:#333,color:#fff;

    class WebApp,ApiGateway,AppCore,SyncWorker container;
    class Database,Cache,SearchEngine database;
    class SIS external;
```

---

## 3. Comprehensive Modular Data Flow & Domain Diagram (Level 3+)

This diagram details the deep internal structure of the **Modular Monolithic Backend**. It captures:
1. All distinct **user roles** (Student, Instructor, Student Affairs, System Admin, Finance).
2. Their interactions with specific **portals/gateways**.
3. The routing of their requests to proper **Bounded Contexts** (Core, Supporting, Optional).
4. The crucial **data flow from the external SIS** down into local projection databases, managed by the **Integration Layer (ACL)**.

```mermaid
flowchart TD

    %% ----------------------------------------------------
    %% EXTERNAL SYSTEMS (Source of Truth)
    %% ----------------------------------------------------
    subgraph ExternalEcosystem [External Systems]
        SIS[("Central SIS / ERP\n(Master Data)")]
        PaymentGtwy["Payment Gateway"]
    end

    %% ----------------------------------------------------
    %% USERS & PORTALS
    %% ----------------------------------------------------
    subgraph EndUsers [User Roles]
        R_Student([Student])
        R_Instructor([Instructor])
        R_StudentAffairs([Student Affairs])
        R_SysAdmin([System Admin])
        R_Finance([Finance Officer])
    end

    subgraph Portals [User Interfaces / API Gateway]
        UI_Student["Student Portal"]
        UI_Staff["Staff / Faculty Portal"]
        UI_Admin["Admin & Management Portal"]
    end

    %% Map Users to Portals
    R_Student --> UI_Student
    R_Instructor --> UI_Staff
    R_StudentAffairs --> UI_Admin
    R_Finance --> UI_Admin
    R_SysAdmin --> UI_Admin

    %% ----------------------------------------------------
    %% DATA LAYER
    %% ----------------------------------------------------
    subgraph DataLayer [Local Data Store]
        LocalDB[("Local Projection DB\n& Workflow State")]
    end

    %% ----------------------------------------------------
    %% BACKEND: MODULAR MONOLITH
    %% ----------------------------------------------------
    subgraph ModularBackend [Modular Monolith: Bounded Contexts]

        %% INTEGRATION CORE (ACL)
        subgraph Core_Integration [🔴 Integration & Sync: Execution Core]
            ACL_Inbound["Inbound Sync / Domain Mapper: ACL"]
            ACL_Outbound["Outbound Relay: Transactional Outbox"]
        end

        %% IAM CORE
        subgraph Core_IAM [🔴 Identity & Access: Execution Core]
            Mod_Auth["Authentication & SSO"]
            Mod_RBAC["Role & Permissions Engine"]
            Mod_UserProfile["Local User Profiles"]
        end

        %% WORKFLOW & ORCHESTRATION CORE
        subgraph Core_Workflow [🔴 Requests & Orchestration: Execution Core]
            Mod_Engine["Workflow Engine: Approvals, SLAs"]
            Mod_StudentReq["Student Requests: Certificates, Appeals"]
            Mod_RegOrchestrator["Registration Orchestrator: Local Soft Booking"]
        end

        %% UPSTREAM PROJECTIONS (Read-Only)
        subgraph Upstream_Domains [🟠 Upstream Projections: Read Models]
            Mod_Catalog["Academic Catalog Projection: Programs, Courses"]
            Mod_EnrollmentHistory["Enrollment History Projection"]
            Mod_FinanceProj["Finance Ledger Projection: Balances"]
        end

        %% SUPPORTING DOMAINS (Write-Heavy / Transactions)
        subgraph Support_Domains [🟡 Supporting Domains: Teaching & Operations]
            Mod_Grading["Grading & Assessment"]
            Mod_Attendance["Attendance Tracking"]
            Mod_Schedule["Timetable & Scheduling"]
            Mod_Advising["Advising & Graduation"]
            Mod_PaymentProc["Payment Processor"]
        end

        %% OPTIONAL DOMAINS
        subgraph Optional_Domains [⚪ Optional / Cross-Cutting Domains]
            Mod_Notify["Notifications: Email/SMS"]
            Mod_Reports["Reporting & Analytics"]
            Mod_Docs["File & Document Management"]
        end
    end

    %% ----------------------------------------------------
    %% DATA FLOWS & RELATIONSHIPS
    %% ----------------------------------------------------

    %% 1. Inbound SIS Integration Flow (CQRS Read Path)
    SIS -->|Pulls Raw Master Data| ACL_Inbound
    ACL_Inbound -->|Updates Local Read Models| Mod_Catalog
    ACL_Inbound -->|Updates Local Read Models| Mod_EnrollmentHistory
    ACL_Inbound -->|Updates Local Read Models| Mod_FinanceProj
    ACL_Inbound -->|Updates User Base| Mod_UserProfile

    %% 2. DB Persistence & Outbox
    Core_Integration -->|Reads/Writes| LocalDB
    Core_IAM -->|Reads/Writes| LocalDB
    Core_Workflow -->|Reads/Writes Outbox| LocalDB
    Upstream_Domains -->|Reads| LocalDB
    Support_Domains -->|Reads/Writes Outbox| LocalDB
    Optional_Domains -->|Reads/Writes| LocalDB

    %% 3. Portal routing to modules (Feature mapping)
    UI_Student -->|Views Catalog| Mod_Catalog
    UI_Student -->|Views Schedule| Mod_EnrollmentHistory
    UI_Student -->|Views Balances| Mod_FinanceProj
    UI_Student -->|Submits Requests| Mod_StudentReq

    %% Write Actions mapped to Command Orchestrators/Processors
    UI_Student -->|Initiates Registration| Mod_RegOrchestrator
    UI_Student -->|Initiates Payment| Mod_PaymentProc

    UI_Staff -->|Enters Grades| Mod_Grading
    UI_Staff -->|Marks Attendance| Mod_Attendance
    UI_Staff -->|Views Schedule| Mod_Schedule

    UI_Admin -->|Manages Workflows| Mod_Engine
    UI_Admin -->|Configures Roles| Mod_RBAC
    UI_Admin -->|Generates Reports| Mod_Reports

    %% 4. Internal Module Dependencies (DDD Rules)
    Mod_StudentReq -->|Triggers| Mod_Engine
    Mod_Engine -->|Uses Roles| Mod_RBAC
    Mod_RegOrchestrator -->|Checks Prerequisites| Mod_Catalog

    %% 5. The Write-Back Path (CQRS Command Path to SIS via Outbox)
    Mod_RegOrchestrator -->|Writes Command to Outbox| ACL_Outbound
    Mod_PaymentProc -->|Writes Success to Outbox| ACL_Outbound
    Mod_Grading -->|Writes Final Grades to Outbox| ACL_Outbound
    Mod_Attendance -->|Writes Logs to Outbox| ACL_Outbound

    ACL_Outbound -->|Asynchronously Pushes to| SIS

    %% 6. Optional Domain Event Hooks
    Mod_Engine -.->|Publishes Event| Mod_Notify
    Mod_RegOrchestrator -.->|Saga Compensations| Mod_Notify
    Mod_StudentReq -.->|Stores Attachments| Mod_Docs

    %% 7. External Integrations
    Mod_PaymentProc -->|Executes Transaction| PaymentGtwy

    %% ----------------------------------------------------
    %% STYLING
    %% ----------------------------------------------------
    classDef core fill:#ffebee,stroke:#c62828,stroke-width:2px;
    classDef upstream fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef support fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef optional fill:#f5f5f5,stroke:#9e9e9e,stroke-width:2px;
    classDef external fill:#e0e0e0,stroke:#616161,stroke-width:2px;
    classDef db fill:#2d5d8c,stroke:#1d3d5c,color:#fff,stroke-width:2px;

    class Core_Integration,Core_IAM,Core_Workflow core;
    class Upstream_Domains upstream;
    class Support_Domains support;
    class Optional_Domains optional;
    class SIS,PaymentGtwy external;
    class LocalDB db;

```

---

## Architectural Principles & Rules

1. **Dependency Direction**:
   - Supporting modules may depend on Core modules.
   - Core modules must **never** depend on Supporting or Optional modules.
   - Optional modules must act as isolated endpoints (often driven by domain events like `StudentEnrolledEvent`).
2. **Anti-Corruption Layer (ACL)**:
   - The system **must never trust external SIS data directly**. All external data passes through the Integration Layer, gets transformed, and is stored as a local projection in the database.
   - The local database is a *Projection* and *Workflow State Store*, not the ultimate source of truth for academic records.
3. **Eventual Consistency**:
   - Because the SIS is the source of truth for domains like `Academic Structure` and `Finance`, our system relies on scheduled syncing or event-driven updates.
4. **Resilience**:
   - If the `Notification` or `Reporting` modules fail, core operations like `StudentRequests` or `Enrollment` workflows must continue to function uninterrupted.
