# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

This is **not** a product codebase — it's the user's personal, standing reference solution for .NET architecture patterns (Clean Architecture, DDD, TDD, exception handling, etc.). It doubles as a hands-on practice project. When the user starts a new real project, they come back here to see how a pattern was solved before rather than re-deriving it. Treat correctness and clarity of the pattern as more important than shipping a feature — this solution is meant to stay in a demonstrably clean state.

Do not delete or "clean up" work-in-progress/incomplete code unless asked — half-finished pieces (e.g. `OrdersController.cs`) may be intentionally parked mid-refactor. If something looks broken, ask before fixing it, unless you were already told to leave it alone.

## Rules

Portable conventions settled on in this repo — meant to carry over to other projects too, not just this one. Grown one at a time as we build; add a new numbered rule when something becomes a settled convention, not a one-off.

1. Each Clean Architecture layer is its own project, not just a folder, so the dependency boundary is compiler-enforced.
2. DTOs live only in the Presentation layer — never in Application or Domain.
3. DTO ↔ domain mapping is manual (static extension methods), co-located with the DTOs in Presentation — never in Application.
4. Validation is split by tier: DTO format checks in Presentation, entity invariants in Domain, rules needing external data in Application.
5. Each layer registers its own services via its own `DependencyInjection.cs` extension method.
6. Use a Test Data Builder for a type once it's constructed in more than one test; a one-off object used in a single test stays inline.
7. Presentation's public surface (controller signatures, DTOs) never exposes Domain types directly — project references don't stop this, so it's enforced by discipline/review (or an architecture test).
8. Exceptions are only for the unexpected: expected failures (validation, business rules) return `Result<T, Error>`, not exceptions; try/catch is for translating exceptions thrown by code you don't control (DB/HTTP/3rd-party) at its boundary; a global handler catches whatever's left.

## Commands

```bash
dotnet build dotNet-arch-template.slnx      # build entire solution
dotnet test Tests/Tests.csproj            # run all tests
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~QuoteServiceTests"   # run one test class
dotnet run --project Presentation/API/API.csproj
dotnet run --project Presentation/MinimalAPI/MinimalAPI.csproj
```

Target framework: `net10.0` across all projects. Nullable reference types and implicit usings are enabled everywhere.

## Architecture

Clean Architecture with strictly inward-pointing dependencies, each layer its own project (not just a folder) so the boundary is compiler-enforced:

```
Presentation/API, Presentation/MinimalAPI  →  Application  →  Domain
                                            ↘  Infrastructure ↗
Tests  →  Domain, Application, Infrastructure
```

- **`Domain/`** — POCOs only (`Models/Order`, `Item`, `Quote`). Zero dependencies on any other project or framework. This is where domain *behavior* (business rules that only need the entity's own state, e.g. "an order must have at least one item") is supposed to live — currently it does not yet; see below.
- **`Application/`** — use-case orchestration, split into subfolders by kind: `Services/` (`IQuoteService`/`QuoteService`, namespace `Application.Services`), `Results/` (`Result<TValue, TError>` and `Error` — the generic outcome wrapper expected-failures use, per Rule 8), `Exceptions/OrderValidationException` (now unused now that `PrepareQuote` reports failure via `Result` instead of throwing — parked, not deleted), `DependencyInjection.cs` (`AddApplicationServices()` extension method — registers `IQuoteService`/`QuoteService`). References `Domain` only (plus the `Microsoft.Extensions.DependencyInjection.Abstractions` package for that extension method). Framework-agnostic (no ASP.NET, no EF Core). Rules that need external data (e.g. a DB lookup) belong here or in a domain service, not in `Domain`. `IQuoteService.PrepareQuote` returns `Task<Result<Quote, Error>>`, not `Task<Quote>`. Whether `Services/` stays grouped by kind or moves to per-feature folders as more use cases are added is an open question, deliberately deferred for now.
- **`Infrastructure/`** — scaffolded, currently empty aside from `DependencyInjection.cs` (`AddInfrastructureServices()`, a no-op placeholder for now). Intended home for EF Core, repositories, external clients — anything that implements an interface defined in `Application`. References `Application`.
- **`Presentation/API/`** and **`Presentation/MinimalAPI/`** — two parallel presentation layers deliberately kept side by side (controller-based vs Minimal API), both solving the same use case, both referencing `Application` + `Infrastructure`. Each owns its **own** `Dtos/` (`OrderDto`, `ItemDto`, `QuoteDto`) — DTOs are a presentation-only concept and never appear below this layer. The two presentation projects' DTOs are independent copies, not shared, by design. `API` also has a `Mapper/` folder (`OrderMapper`, `ItemMapper`, `QuoteMapper` — static extension classes, one per type, `ToDomain()`/`ToDto()`) for DTO ↔ domain translation; `Program.cs` wires DI via `AddApplicationServices()`/`AddInfrastructureServices()`. `MinimalAPI` has no mapper yet.
- **`Tests/`** — one xUnit project, organized into per-layer folders (`ApplicationTests/`, `APITests/` — with a `Mapper/` subfolder mirroring `Presentation/API/Mapper/` — and `DomainTests/`/`InfrastructureTests/` once those layers grow content) rather than one test project per layer. `Builders/` holds fluent Test Data Builders (e.g. `OrderBuilder` for `Domain.Order`, `ItemDtoBuilder` for `API.Dtos.ItemDto`) used to keep test arrange-sections terse — extend this pattern (one builder per type, reused across the tests that need it) rather than constructing objects inline, but only once a builder is actually reused by more than one test; a one-off object used in a single test can stay inline. `Tests.csproj` references `Presentation/API` (needed for `APITests`), so `Tests` currently fails to build whenever `API` does.

### Clean Architecture — what lives where

Canonical reference for what each layer is *for*, independent of what's actually implemented in this repo yet (see the bullets above and "Known, deliberate rough edges" below for current state).

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

### Known, deliberate rough edges

- **`Presentation/API/Controllers/OrdersController.cs`** correctly maps `OrderDto` → `Order`, awaits `_quoteService.PrepareQuote(order)`, and branches on the returned `Result<Quote, Error>` (`Ok`/`BadRequest`) — but its action still declares `Task<ActionResult<OrderDto>>` while `result.Value.ToDto()` produces a `QuoteDto`, so it fails to compile with a type-mismatch error. This is a known, parked issue; the rest of the solution builds clean around it (except `Tests`, which now depends on `API`). Do not "fix" it unless explicitly asked to work on it.
- **`Application/QuoteService.PrepareQuote`** is an intentional stub (`throw new NotImplementedException()` unconditionally) returning `Task<Result<Quote, Error>>` — implementation is being driven out via TDD, one rule at a time, starting from `Tests/ApplicationTests/QuoteServiceTests.cs`.
- **`Tests/ApplicationTests/QuoteServiceTests.cs`** is stale relative to the `Result`-based contract: it still asserts the old throw-based behavior (`Assert.ThrowsAsync<OrderValidationException>`), which no longer matches `PrepareQuote`'s signature or intended behavior. Parked, not fixed yet.
- DTO ↔ domain mapping now exists for `Presentation/API` (`API.Mapper`, covered by `Tests/APITests/Mapper`); `Presentation/MinimalAPI` has none yet.

### Design conventions being followed in this repo

- Validation is split by tier: DTO-level format checks (Data Annotations/FluentValidation) belong in `Presentation`; business invariants that only need an entity's own data belong on the `Domain` entity itself; rules needing external data belong in `Application`/a domain service.
- Domain entities are currently anemic (no behavior) — moving validation/computation methods like `PrepareQuote` onto `Order` itself (rather than leaving all logic in `QuoteService`) is the intended next step toward this actually being DDD rather than just layered architecture.
- DTO ↔ domain mapping is manual (extension methods, one static class per type, e.g. `OrderMapper`), not AutoMapper — deliberate choice for compile-time safety, explicitness (this is a reference solution meant to be read), and to avoid AutoMapper's 2025 move to a commercial license. If manual mapping ever becomes genuinely repetitive at scale, prefer Mapperly (compile-time source generator) over AutoMapper. Mappers live in `Presentation` (co-located with the DTOs they translate), never in `Application` — `Application` must not reference DTO types.
- Each project registers its own services via a `DependencyInjection.cs` extension method (`AddApplicationServices()`, `AddInfrastructureServices()`), composed in each presentation project's `Program.cs`. This is deliberate now that there are two presentation projects sharing the same services (avoids duplicating registration), not just a stylistic default.
- `.claude/settings.local.json` in this repo contains permission entries referencing an unrelated project (`amr-engineering-portfolio`) — leftover, not meaningful to this solution.
