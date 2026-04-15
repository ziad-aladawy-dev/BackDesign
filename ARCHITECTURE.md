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

## 3. Modular Internal Views (Level 3)

To prevent visual clutter ("the jungle of arrows") and strictly enforce boundary concepts, the detailed architecture is split into two bounded visual views:
* **Level 3A:** User UI to Execution Flow (Focuses on Portals, IAM, and Command Execution).
* **Level 3B:** Integration & Data Flow (Focuses on the CQRS Read/Write paths, Master Data Sync, and Portal-Owned State).

### Level 3A: User UI to Execution Flow

This view demonstrates how actors interact with unified portals, and how those portals interact with the Execution Core and Supporting Domains. Notice that Instructors, Finance, and Admins share a consolidated **Management Portal**, heavily relying on the **RBAC Engine** to hydrate their UI dynamically.

```mermaid
flowchart TD

    %% USERS
    subgraph EndUsers [User Roles]
        R_Student([Student])
        R_Instructor([Instructor])
        R_StudentAffairs([Student Affairs])
        R_SysAdmin([System Admin])
        R_Finance([Finance Officer])
    end

    %% PORTALS (UI Shells)
    subgraph Portals [User Interfaces / API Gateways]
        UI_Student["Student Portal"]
        UI_Management["Unified Management Portal"]
    end

    %% Map Users
    R_Student --> UI_Student
    R_Instructor --> UI_Management
    R_StudentAffairs --> UI_Management
    R_Finance --> UI_Management
    R_SysAdmin --> UI_Management

    %% IAM CORE
    subgraph Core_IAM [🔴 Identity & Access: Execution Core]
        Mod_Auth["Authentication & SSO"]
        Mod_RBAC["Role & Permissions Engine"]
    end

    UI_Management -.->|Dynamically Renders via| Mod_RBAC

    %% WORKFLOW & ORCHESTRATION CORE
    subgraph Core_Workflow [🔴 Requests & Orchestration: Execution Core]
        Mod_Engine["Workflow Engine: Approvals, SLAs"]
        Mod_StudentReq["Student Requests: Certificates, Appeals"]
        Mod_RegOrchestrator["Registration Orchestrator: Local Soft Booking"]
    end

    %% SUPPORTING DOMAINS (Write-Heavy)
    subgraph Support_Domains [🟡 Supporting Domains: Operations]
        Mod_Grading["Grading & Assessment"]
        Mod_Attendance["Attendance Tracking"]
        Mod_PaymentProc["Payment Processor"]
    end

    %% OPTIONAL UI-FACING DOMAINS
    subgraph Optional_Domains [⚪ Optional / UI Features]
        Mod_Reports["Reporting & Analytics"]
        Mod_FeedbackSurveys["Feedback & Surveys"]
    end

    %% Feature Routing
    UI_Student -->|Submits Requests| Mod_StudentReq
    UI_Student -->|Initiates Registration| Mod_RegOrchestrator
    UI_Student -->|Initiates Payment| Mod_PaymentProc
    UI_Student -->|Submits| Mod_FeedbackSurveys

    UI_Management -->|Enters Grades| Mod_Grading
    UI_Management -->|Marks Attendance| Mod_Attendance
    UI_Management -->|Manages Workflows| Mod_Engine
    UI_Management -->|Views Dashboards| Mod_Reports

    %% Internal Dependencies
    Mod_StudentReq -->|Triggers| Mod_Engine
    Mod_Engine -->|Uses Roles| Mod_RBAC

    %% STYLING
    classDef core fill:#ffebee,stroke:#c62828,stroke-width:2px;
    classDef support fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef optional fill:#f5f5f5,stroke:#9e9e9e,stroke-width:2px;
    class Core_IAM,Core_Workflow core;
    class Support_Domains support;
    class Optional_Domains optional;
```

---

### Level 3B: Integration & Data Flow

This diagram illustrates the CQRS Data flow. It separates **Upstream Projections** (where SIS is the system of record) from **Portal-Owned State** (where the Local DB is the sole source of truth). It also illustrates the outbound write-back path utilizing the **Transactional Outbox**, and the system-wide **Audit Trail**.

```mermaid
flowchart TD

    %% EXTERNAL
    SIS[("Central SIS / ERP\n(Master Data)")]

    %% INTEGRATION CORE (ACL)
    subgraph Core_Integration [🔴 Integration & Sync: Execution Core]
        ACL_Inbound["Inbound Sync / Domain Mapper: ACL"]
        ACL_Outbound["Outbound Relay: Transactional Outbox"]
    end

    %% UPSTREAM PROJECTIONS
    subgraph Upstream_Domains [🟠 Upstream Projections: Read Models]
        Mod_Catalog["Academic Catalog Projection"]
        Mod_EnrollmentHistory["Enrollment History Projection"]
        Mod_FinanceProj["Finance Ledger Projection"]
    end

    %% EXECUTION / SUPPORTING COMMANDS
    subgraph Command_Domains [🟡 Commands / Orchestrators]
        Mod_RegOrchestrator["Registration Orchestrator"]
        Mod_PaymentProc["Payment Processor"]
        Mod_Grading["Grading & Assessment"]
    end

    %% PORTAL-EXCLUSIVE STATE
    subgraph Portal_State [⚪ Portal-Exclusive Data]
        Mod_UserProfile["Local User Profiles (Photos, Prefs)"]
        Mod_FeedbackSurveys["Feedback & Surveys"]
    end

    %% AUDIT & EVENTING
    subgraph Observability [⚪ Observability]
        Mod_AuditTrail["Central Audit Trail"]
    end

    LocalDB[("Local DB\n(Projections, Outbox, State)")]

    %% SIS -> Read Models (Inbound CQRS)
    SIS -->|Pulls Master Data| ACL_Inbound
    ACL_Inbound -->|Updates| Mod_Catalog
    ACL_Inbound -->|Updates| Mod_EnrollmentHistory
    ACL_Inbound -->|Updates| Mod_FinanceProj

    Upstream_Domains -->|Persists Projections| LocalDB

    %% Commands -> Write-Back (Outbound CQRS)
    Mod_RegOrchestrator -->|Checks Rules| Mod_Catalog
    Mod_RegOrchestrator -->|Writes Command to Outbox| ACL_Outbound
    Mod_PaymentProc -->|Writes Success to Outbox| ACL_Outbound
    Mod_Grading -->|Writes Final Grades to Outbox| ACL_Outbound

    ACL_Outbound -->|Async Push via Message Bus| SIS

    %% Portal-Exclusive (No SIS Sync)
    Mod_UserProfile -->|Reads/Writes Only| LocalDB
    Mod_FeedbackSurveys -->|Reads/Writes Only| LocalDB

    %% Event Logging
    Command_Domains -.->|Domain Events: Who did what| Mod_AuditTrail
    Mod_AuditTrail -->|Persists Logs| LocalDB

    %% STYLING
    classDef core fill:#ffebee,stroke:#c62828,stroke-width:2px;
    classDef upstream fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef command fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef portal fill:#f5f5f5,stroke:#9e9e9e,stroke-width:2px;
    classDef db fill:#2d5d8c,stroke:#1d3d5c,color:#fff,stroke-width:2px;

    class Core_Integration core;
    class Upstream_Domains upstream;
    class Command_Domains command;
    class Portal_State,Observability portal;
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
