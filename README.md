# TuColmadoRD - Cloud-Local Hybrid System

![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)
![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-purple.svg)
![Pattern](https://img.shields.io/badge/Pattern-Railway_Oriented_Programming-orange.svg)
![ORM](https://img.shields.io/badge/ORM-EF_Core-green.svg)

**TuColmadoRD** is the core backend and business logic nexus for a modern point-of-sale system tailored for retail grocery stores ("Colmados"). It employs a hybrid-cloud architecture ensuring operations even during internet outages, backed by a cryptographic anti-tamper security layer.

---

## 🏗 System Architecture

The project strictly follows **Domain-Driven Design (DDD)** and **Clean Architecture**, enforcing inversion of dependencies across highly segregated logic layers. 

### Modules
1. **`TuColmadoRD.Core.Domain`**: Defines all Models, ValueObjects (`TenantIdentifier`), Enums, and custom `DomainError` variations. Agnostic of frameworks.
2. **`TuColmadoRD.Core.Application`**: Handlers, Use Cases, Pipelines, DTOs. Orchestrated using **MediatR** and standardizing fallible responses using the **Railway-Oriented Programming (ROP)** `Result<T>` pattern.
3. **`TuColmadoRD.Infrastructure.Persistence`**: **Entity Framework Core** schemas, Migrations, Repositories, and the `DbContext`. Handled for **SQL Server** in Development and **PostgreSQL** in Production.
4. **`TuColmadoRD.Infrastructure.CrossCutting`**: Common non-business utilities like Dependency Injection configs, network monitors, background hosted services, local DB tenant resolving (`LocalDeviceTenantProvider`), and the offline `LicenseVerifierService`.
5. **`TuColmadoRD.Presentation.API`**: RESTful API endpoints, Swagger exposition.
6. **`TuColmadoRD.ApiGateway`**: The central **YARP (Yet Another Reverse Proxy)** gateway proxying both `.NET API` functionalities and the standalone `Node.js Auth API`. Hosts unified multi-system Swagger UI configuration natively.

### 🛡 Offline Subscription & Anti-Tamper System

A cornerstone module designed specifically to prevent subscription manipulation on standalone devices:
- **`ITimeGuard` & `SystemConfig`**: Employs an internal monotonic database clock ("Last Known Time") enforcing progression and detecting OS manual date tampering. Blockading all execution if reverse clock drift is detected via `ClockAdvancePipelineBehavior`.
- **`LicenseVerifierService`**: Offloads license evaluations offline by using an externally issued `RS256` token validated strictly against the device's public key injected at initial pairing (`device_identity.dat`).
- **`SubscriptionGuardMiddleware`**: Blocks REST endpoint access if the active offline JWT sub-license fails validity execution.

---

## 🚀 Getting Started

### 1. Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- SQL Server (Development) or Npgsql (Postgres)
- Active `device_identity.dat` linked through the gateway initialization flow.

### 2. Startup
Navigate to the root directory and build:
```bash
dotnet restore TuColmadoRD.slnx
dotnet build TuColmadoRD.slnx
```

Execute Database EF Migrations:
```bash
$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet ef database update --project src\infrastructure\TuColmadoRD.Infrastructure.Persistence --startup-project src\Presentations\TuColmadoRD.Presentation.API
```

Start systems simultaneously:
```bash
dotnet run --project src/Presentations/TuColmadoRD.ApiGateway
dotnet run --project src/Presentations/TuColmadoRD.Presentation.API
```

### 3. Unified Swagger & UI
Head to `http://localhost:<YARP_PORT>/` to browse the master interactive directory routing your connection properly to `/swagger/index.html` picking endpoints from both microservices.

## 🐳 Dockerization & CI/CD
Complete `Dockerfile` targets are mapped across `Dockerfile.api` and `Dockerfile.gateway`. 
Continuous deployment is governed by the comprehensive `.github/workflows/devops.yml` connecting securely to the deployment server orchestrating `docker-compose.yml` instances upon merges to `master`.
