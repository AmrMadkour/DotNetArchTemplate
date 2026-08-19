# dotNet-arch-template

A personal, standing reference solution for .NET architecture patterns — Clean Architecture, DDD, and TDD — built with **.NET 10**. It exists as a place to see how a given pattern was solved before, rather than a shippable product, so parts of it are deliberately left mid-refactor or unimplemented. See [`CLAUDE.md`](CLAUDE.md) for the full rationale and known rough edges.

## Architecture

Clean Architecture with strictly inward-pointing dependencies, each layer its own project so the boundary is compiler-enforced:

```
Presentation/API, Presentation/MinimalAPI  →  Application  →  Domain
                                            ↘  Infrastructure ↗
Tests  →  Domain, Application, Infrastructure
```

- **`Domain/`** — POCOs only (`Order`, `Item`, `Quote`). No dependencies on any other project or framework.
- **`Application/`** — use-case orchestration, split by kind: `Services/` (`IQuoteService`/`QuoteService`), `Results/` (`Result<TValue, TError>`, `Error` — generic outcome wrapper for expected failures). References `Domain` only; framework-agnostic.
- **`Infrastructure/`** — scaffolded, currently empty. Intended home for EF Core, repositories, and external clients.
- **`Presentation/API/`** and **`Presentation/MinimalAPI/`** — two parallel presentation layers (controller-based vs. Minimal API) solving the same use case side by side, each with its own `Dtos/`. `API` also has a `Mapper/` folder (`OrderMapper`, `ItemMapper`, `QuoteMapper`) for DTO ↔ domain translation; `MinimalAPI` doesn't have one yet.
- **`Tests/`** — one xUnit project, organized into per-layer folders, with fluent Test Data Builders under `Builders/`.

## Clean Architecture — what lives where

Canonical reference for what each layer is *for*, independent of what's actually implemented in this repo yet (see Architecture above for current state).

**Domain**
- Entities, with behavior (private setters, methods like `Complete()`) — not plain property bags
- Value Objects
- Domain Services (logic that spans multiple entities)
- Repository interfaces (ports) — repositories operate on Aggregates, a domain concept
- Custom domain exceptions
- Enums tied to business concepts (e.g. `OrderState`)

**Application**
- UseCases / Application Services (orchestration)
- Application-level Request/Command objects (if there's more than one entry point)
- Interfaces for external services a UseCase needs (e.g. `INotificationService`)
- Validation of orchestration logic only — business rules stay in Domain

**Infrastructure**
- Repository implementations (adapters)
- `DbContext`
- External API clients (email, payment, SharePoint, etc.)
- Concrete implementations of Application's interfaces

**Presentation**
- Controllers
- DTOs (request/response shapes)
- Middleware
- Mapping between DTO ↔ Application's data

**Does DDD change this?** No — DDD doesn't add or remove layers, it only changes what Domain contains and how. Without DDD, Domain is plain property-bag classes with no behavior. With DDD, Domain is entities with private setters + methods, Aggregates, Value Objects, Domain Services. Application, Infrastructure, and Presentation stay structurally identical either way.

## Rules

Portable conventions settled on in this repo — meant to carry over to other projects too, not just this one.

1. Each Clean Architecture layer is its own project, not just a folder, so the dependency boundary is compiler-enforced.
2. DTOs live only in the Presentation layer — never in Application or Domain.
3. DTO ↔ domain mapping is manual (static extension methods), co-located with the DTOs in Presentation — never in Application.
4. Validation is split by tier: DTO format checks in Presentation, entity invariants in Domain, rules needing external data in Application.
5. Each layer registers its own services via its own `DependencyInjection.cs` extension method.
6. Use a Test Data Builder for a type once it's constructed in more than one test; a one-off object used in a single test stays inline.
7. Presentation's public surface (controller signatures, DTOs) never exposes Domain types directly — project references don't stop this, so it's enforced by discipline/review (or an architecture test).
8. Exceptions are only for the unexpected: expected failures (validation, business rules) return `Result<T, Error>`, not exceptions; try/catch is for translating exceptions thrown by code you don't control (DB/HTTP/3rd-party) at its boundary; a global handler catches whatever's left.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

No database or external services are required — `Infrastructure` is currently an empty scaffold, and no environment variables need to be configured.

## Getting started

```bash
git clone <repo-url>
cd DotNetArchTemplate
dotnet restore dotNet-arch-template.slnx
```

### Build

```bash
dotnet build dotNet-arch-template.slnx
```

> **Note:** this currently fails to build in `Presentation/API/Controllers/OrdersController.cs` (its action declares `ActionResult<OrderDto>` but maps the result to a `QuoteDto`) — a known, intentionally parked mid-refactor stub (see `CLAUDE.md`). `Domain`, `Application`, `Infrastructure`, and `Presentation/MinimalAPI` build clean on their own; build them individually with `dotnet build <ProjectPath>` if you want to skip `API`. **`Tests` also fails to build** for the same reason, since it references `Presentation/API` — see the Test section below.

### Run

```bash
dotnet run --project Presentation/API/API.csproj          # currently fails to compile, see note above
dotnet run --project Presentation/MinimalAPI/MinimalAPI.csproj
```

### Test

```bash
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~QuoteServiceTests"   # run one test class
```

> **Note:** `Tests` references `Presentation/API` (for `Tests/APITests/Mapper`), so it currently fails to build for the same reason as `API` — see the Build note above. Both commands will work again once `OrdersController` compiles; `QuoteServiceTests` will still need updating afterward, since it asserts the old throw-based behavior.

## Status

`Application/QuoteService.PrepareQuote` is an intentional stub returning `Task<Result<Quote, Error>>`, being driven out via TDD, one rule at a time, starting from `Tests/ApplicationTests/QuoteServiceTests.cs` (currently stale — still asserts the old throw-based behavior). DTO ↔ domain mapping now exists for `Presentation/API` (`API.Mapper`, covered by `Tests/APITests/Mapper`) and `OrdersController` uses it, but has a return-type bug (see Build note above); `Presentation/MinimalAPI` has no mapping yet.
