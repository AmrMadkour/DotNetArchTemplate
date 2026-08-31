# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

This is **not** a product codebase — it's the user's personal, standing reference solution for .NET architecture patterns (Clean Architecture, DDD, TDD, exception handling, etc.). It doubles as a hands-on practice project. When the user starts a new real project, they come back here to see how a pattern was solved before rather than re-deriving it. Treat correctness and clarity of the pattern as more important than shipping a feature — this solution is meant to stay in a demonstrably clean state.

Do not delete or "clean up" work-in-progress/incomplete code unless asked — half-finished pieces may be intentionally parked mid-refactor. If something looks broken, ask before fixing it, unless you were already told to leave it alone.

## How to talk to me

For any answer that would otherwise be long (explanations, concept teaching, code reviews, code walkthroughs, design discussions): give the full real content, don't over-explain or pad, split it into short sections with clear headers instead of one dense block, and write it in a plain, human tone. Whenever the answer is split into sections at all, send one section at a time and wait for a "go ahead" before the next — don't dump it all in one message, even if the whole answer wouldn't otherwise count as "genuinely long." This is also captured as this project's `/how-we-talk` command (`.claude/commands/how-we-talk.md`) — project-local by design, since not every command should follow you into every repo.

## Rules

Portable conventions settled on in this repo — meant to carry over to other projects too, not just this one. Grown one at a time as we build; add a new numbered rule when something becomes a settled convention, not a one-off.

1. Each Clean Architecture layer is its own project, not just a folder, so the dependency boundary is compiler-enforced.
2. DTOs live only in the Presentation layer — never in Application or Domain.
3. DTO ↔ domain mapping is manual (static extension methods), co-located with the DTOs in Presentation — never in Application.
4. Validation is split by tier: DTO format checks in Presentation, entity invariants in Domain, rules needing external data in Application.
5. Each layer registers its own services via its own `DependencyInjection.cs` extension method.
6. Use a Test Data Builder for a type once it's constructed in more than one test; a one-off object used in a single test stays inline.
7. Presentation's public surface (controller signatures, DTOs) never exposes Domain types directly — project references don't stop this, so it's enforced by discipline/review (or an architecture test).
8. Exceptions are only for the unexpected: expected failures (validation, business rules) return `Result<T>`, not exceptions; try/catch is for translating exceptions thrown by code you don't control (DB/HTTP/3rd-party) at its boundary; throw directly for invariant violations in your own code (e.g. a missing registration) — these are programmer errors, not caller-triggerable failures, so they don't get try/catch or `Result<T>` either; a global handler catches whatever's left.
9. Entity invariant validation lives on the entity itself as `string? Validate()` (`null` = valid), composed from private per-rule checks — no `out` params, no separate validation service.
10. Picking how to implement a variant/branching decision, ask: will the set of options grow (yes → dictionary/data lookup, not switch/if — OCP); does each option differ by algorithm or only by data (different algorithm → Strategy pattern, same algorithm/different data → plain lookup value); is the resolution reused across call sites or non-trivial to redo (yes → factory/resolver, single cheap call site → resolve inline via an injected collection). Then sanity-check the result against all five SOLID letters, not just OCP.
11. Test method names follow `MethodUnderTest_Scenario_ExpectedBehavior` (e.g. `Quote_WhenPrepareQuoteSucceeds_ShouldReturnOk`) — method name first, not the condition.
12. Message/exception strings are centralized per layer into a `Constants/ValidationMessages` static class (e.g. `Domain.Constants.ValidationMessages`, `Application.Constants.ValidationMessages`) — not a single shared project — so each layer stays self-contained (Domain keeps zero dependencies).
13. Tests assert against inline string literals, never against the same centralized message constant the production code reads from — asserting against the same constant makes the test tautological, since a changed message would still pass.

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

- **`Domain/`** — `Models/Order`, `Item`, `Quote`, `Coupon`, `DiscountType`; `Services/IDiscountStrategy` + its two implementations (`PercentageDiscountStrategy`, `FlatAmountDiscountStrategy`) and `DiscountStrategyFactory` (parked Strategy/Factory scaffold, not wired into `QuoteService` yet); `Constants/ValidationMessages` (centralized invariant/error message strings for `Order`, `Item`, and `DiscountStrategyFactory`, per Rule 12). Zero dependencies on any other project or framework. `Order`/`Item` now own domain *behavior*: `string? Validate()` (invariant checks, per Rule 9) and `CalculateSubtotal()` (computation) live on the entities themselves, not in `Application`.
- **`Application/`** — use-case orchestration, split into subfolders by kind: `Services/` (`IQuoteService`/`QuoteService`, namespace `Application.Services`), `Results/` (`Result<TValue>` — single-generic outcome wrapper with a `string ErrorMessage`, per Rule 8; both `Success()` and `Failure()` throw on a null/empty argument, symmetrically), `Constants/ValidationMessages` (centralized message strings for `QuoteService` and `Result<TValue>`'s guard exceptions, per Rule 12), `Exceptions/` (empty scaffold — `OrderValidationException` was removed now that `PrepareQuote` reports failure via `Result` instead of throwing), `DependencyInjection.cs` (`AddApplicationServices()` extension method — registers `IQuoteService`/`QuoteService`). References `Domain` only (plus the `Microsoft.Extensions.DependencyInjection.Abstractions` package for that extension method). Framework-agnostic (no ASP.NET, no EF Core). Rules that need external data (e.g. a DB lookup) belong here or in a domain service, not in `Domain`. `IQuoteService.PrepareQuote` returns `Task<Result<Quote>>`, not `Task<Quote>`. Whether `Services/` stays grouped by kind or moves to per-feature folders as more use cases are added is an open question, deliberately deferred for now.
- **`Infrastructure/`** — scaffolded, currently empty aside from `DependencyInjection.cs` (`AddInfrastructureServices()`, a no-op placeholder for now). Intended home for EF Core, repositories, external clients — anything that implements an interface defined in `Application`. References `Application`.
- **`Presentation/API/`** and **`Presentation/MinimalAPI/`** — two parallel presentation layers deliberately kept side by side (controller-based vs Minimal API), both solving the same use case, both referencing `Application` + `Infrastructure`. Each owns its **own** `Dtos/` (`OrderDto`, `ItemDto`, `QuoteDto`) and its **own** `Mapper/` (`OrderMapper`, `ItemMapper`, `QuoteMapper` — static extension classes, one per type, `ToDomain()`/`ToDto()`) — DTOs are a presentation-only concept and never appear below this layer, and the two presentation projects' DTOs/mappers are independent copies, not shared, by design. `Program.cs` in both wires DI via `AddApplicationServices()`/`AddInfrastructureServices()`. `API` exposes the use case via `Controllers/OrdersController.cs` (`POST api/Orders/Quote`); `MinimalAPI` exposes it via `Endpoints/OrderEndpoints.cs` (`MapOrderEndpoints()` extension, same one-extension-per-concern pattern as Rule 5, registered in `Program.cs`; route `POST /orders/quote`), returning `Task<Results<Ok<QuoteDto>, BadRequest<string>>>` via `TypedResults` rather than a bare `IResult`, so each outcome self-describes its status code for the OpenAPI generator instead of relying on convention. Both presentation projects wire native OpenAPI (`AddOpenApi()`/`MapOpenApi()`), Swashbuckle (`AddSwaggerGen()`/`UseSwagger()`/`UseSwaggerUI()`), and Scalar (`MapScalarApiReference()`) side by side in `Program.cs`, all dev-only (`IsDevelopment()`) — deliberately redundant for learning/comparison in this repo, not a pattern to carry into a real project (pick one). `MinimalAPI` additionally needs `AddEndpointsApiExplorer()` before `AddSwaggerGen()`, since — unlike `API` — it has no `AddControllers()` call to register that for it, and without it Swashbuckle can't discover the minimal API endpoints.
- **`Tests/`** — one xUnit project, organized into per-layer folders rather than one test project per layer: `ApplicationTests/Services` (`QuoteServiceTests`), `ApplicationTests/Results` (`ResultTests`), `APITests/Mapper` (mirroring `Presentation/API/Mapper/`), `APITests/Controllers` (`OrdersControllerTests`, via `Moq`), `DomainTests/Models` + `DomainTests/Services` (entities and discount strategies). `Builders/` holds fluent Test Data Builders (one per DTO/domain type, e.g. `OrderBuilder`/`OrderDtoBuilder`, `QuoteBuilder`/`QuoteDtoBuilder`) used to keep test arrange-sections terse — extend this pattern (reused across the tests that need it) rather than constructing objects inline, but only once a builder is actually reused by more than one test; a one-off object used in a single test can stay inline. `Tests.csproj` references `Presentation/API` (needed for `APITests`) and adds `Moq` (controller mocking) and `FluentAssertions` (kept as a commented-out alternative assertion next to the live xUnit `Assert`/`Assert.Equivalent` call in each test, for reference, not actively used).

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

- **`Application/QuoteService.PrepareQuote`** is implemented (validates via `Order.Validate()`, computes `Subtotal`/`Total`, applies `SAVE10`/`SAVE20` coupon rules via a `Domain.Models.Coupon`/`CouponCatalog` lookup — dollar thresholds against `MinSubtotal`, percentage off) — no longer the earlier `NotImplementedException` stub, and no longer the switch-on-`order.CouponCode` version either. A parked Strategy/Factory scaffold (`Domain/Services/IDiscountStrategy`, `PercentageDiscountStrategy`, `FlatAmountDiscountStrategy`, `DiscountStrategyFactory`) also exists for the day a coupon needs a genuinely different formula, not just different numbers — see the comment in `PrepareQuote` for how it would plug in; not wired in yet since every coupon today is still a percentage.
- DTO ↔ domain mapping exists for both presentation projects (`API.Mapper`, covered by `Tests/APITests/Mapper`; `MinimalAPI.Mapper`, not yet covered by tests).

### Design conventions being followed in this repo

- Validation is split by tier: DTO-level format checks (Data Annotations/FluentValidation) belong in `Presentation`; business invariants that only need an entity's own data belong on the `Domain` entity itself; rules needing external data belong in `Application`/a domain service.
- Domain entities are no longer anemic: `Order.Validate()`/`Item.Validate()` (invariants) and `Order.CalculateSubtotal()`/`Item.CalculateSubtotal()` (computation) live on the entities per Rule 9, with `QuoteService` orchestrating over them rather than containing the logic itself.
- DTO ↔ domain mapping is manual (extension methods, one static class per type, e.g. `OrderMapper`), not AutoMapper — deliberate choice for compile-time safety, explicitness (this is a reference solution meant to be read), and to avoid AutoMapper's 2025 move to a commercial license. If manual mapping ever becomes genuinely repetitive at scale, prefer Mapperly (compile-time source generator) over AutoMapper. Mappers live in `Presentation` (co-located with the DTOs they translate), never in `Application` — `Application` must not reference DTO types.
- Each project registers its own services via a `DependencyInjection.cs` extension method (`AddApplicationServices()`, `AddInfrastructureServices()`), composed in each presentation project's `Program.cs`. This is deliberate now that there are two presentation projects sharing the same services (avoids duplicating registration), not just a stylistic default.
