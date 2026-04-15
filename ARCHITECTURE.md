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

## 3. Component / Module Diagram (Level 3)

This diagram details the internal structure of the **Modular Monolithic Backend**. It classifies the modules according to Domain-Driven Design into Core, Upstream Core, Supporting, and Optional domains.

* **Execution Core**: The absolute backbone of this orchestrator system. The system dies without these.
* **Upstream Core (Read-Only Projections)**: Core to the university, but owned by the SIS. We maintain local projections of these.
* **Supporting/Value-Add Modules**: Modules that enhance the university experience (workflows, portals).
* **Optional/Generic Modules**: Plug-and-play functionalities.

```mermaid
flowchart TD

    %% Define Subgraphs for Categorization
    subgraph ExecutionCore [🔴 Execution Core: System Cannot Run Without These]
        IAM["Identity & Access Management (IAM)\nAuth, Roles, Permissions"]
        UserContext["User Profile & Context\nLocal user projection"]
        Integration["Integration & Sync Layer (ACL)\nShields from SIS chaos"]
        Workflow["Workflow Engine\nApprovals, SLAs, Statuses"]
        StudentRequests["Student Requests System\nCertificates, Clearance"]
    end

    subgraph UpstreamCore [🟠 Upstream Core / Projections: Owned by SIS]
        AcademicStructure["Academic Structure\nPrograms, Departments, Catalog"]
        Enrollment["Enrollment Records\nCourse reg, prerequisites"]
        Finance["Financial Data\nTuition, Balances, Payments"]
    end

    subgraph SupportingModules [🟡 Supporting Modules: Value Add]
        Timetable["Timetable & Scheduling"]
        Attendance["Attendance Tracking"]
        Grading["Grading & Assessment"]
        Exams["Exams Management"]
        Advising["Advising System"]
        Graduation["Graduation Management"]
    end

    subgraph OptionalModules [⚪ Optional / Generic Modules: Plug & Play]
        Notifications["Notification System\nEmail, SMS, Push"]
        Reporting["Reporting & Analytics"]
        Documents["File & Document Management"]
        ContentLMS["Learning & Content\n(Optional internal LMS)"]
        AuditLog["Audit & Logging"]
        Calendar["Academic Calendar Config"]
    end

    %% Key Relationships (Dependency Rules)
    %% Core modules are independent or rely on Upstream projections
    Integration -->|Updates| UpstreamCore
    Integration -->|Updates| UserContext

    UserContext --> IAM
    StudentRequests --> Workflow
    StudentRequests --> UserContext
    StudentRequests --> UpstreamCore

    %% Supporting modules depend on Core & Projections
    SupportingModules --> ExecutionCore
    SupportingModules --> UpstreamCore

    %% Optional modules are usually event-driven or leaf nodes
    ExecutionCore -.->|Publishes Events| OptionalModules
    SupportingModules -.->|Publishes Events| OptionalModules

    %% Styling
    classDef core fill:#ffebee,stroke:#c62828,stroke-width:2px;
    classDef upstream fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef support fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef optional fill:#f5f5f5,stroke:#9e9e9e,stroke-width:2px;

    class IAM,UserContext,Integration,Workflow,StudentRequests core;
    class AcademicStructure,Enrollment,Finance upstream;
    class Timetable,Attendance,Grading,Exams,Advising,Graduation support;
    class Notifications,Reporting,Documents,ContentLMS,AuditLog,Calendar optional;
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
