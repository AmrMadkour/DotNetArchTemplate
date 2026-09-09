# dotNet-arch-template

A personal, standing reference solution for .NET architecture patterns — Clean Architecture, DDD, and TDD — built with **.NET 10**. It exists as a place to see how a given pattern was solved before, rather than a shippable product, so parts of it are deliberately left mid-refactor or unimplemented. See [`CLAUDE.md`](CLAUDE.md) for the full rationale and known rough edges.

## What You Can Learn Here

Quick access points — what to look at and where, updated as new patterns land.

| What | Where |
|---|---|
| Clean Architecture (layers as separate projects, inward-only dependencies) | `Domain/`, `Application/`, `Infrastructure/`, `Presentation/API/`, `Presentation/MinimalAPI/` |
| Entity invariants (`Validate()`) + computation (`CalculateSubtotal()`) on the entity | `Domain/Models/Order.cs`, `Domain/Models/Item.cs` |
| Result pattern (no exceptions for expected failures) | `Application/Results/Result.cs` |
| Use-case orchestration | `Application/Services/QuoteService.cs`, `Application/Services/IQuoteService.cs` |
| Coupon lookup (data-driven, not switch) | `Domain/Models/Coupon.cs` / `CouponCatalog` |
| Parked Strategy + Factory pattern (for when a coupon needs a different algorithm) | `Domain/Services/IDiscountStrategy.cs`, `PercentageDiscountStrategy.cs`, `FlatAmountDiscountStrategy.cs`, `DiscountStrategyFactory.cs` |
| Manual DTO ↔ domain mapping (extension methods) | `Presentation/API/Mapper/`, `Presentation/MinimalAPI/Mapper/` |
| DTOs (Presentation-only, never below) | `Presentation/API/Dtos/`, `Presentation/MinimalAPI/Dtos/` |
| Controller-style endpoint | `Presentation/API/Controllers/OrdersController.cs` |
| Minimal API endpoint | `Presentation/MinimalAPI/Endpoints/OrderEndpoints.cs` |
| Per-layer DI registration | `Application/DependencyInjection.cs`, `Infrastructure/DependencyInjection.cs` |
| API docs tooling compared side by side (native OpenAPI, Swagger UI, Scalar) | `Presentation/API/Program.cs`, `Presentation/MinimalAPI/Program.cs` |
| Minimal API typed results (`Results<Ok<T>, BadRequest<string>>` via `TypedResults`, not a bare `IResult`) | `Presentation/MinimalAPI/Endpoints/OrderEndpoints.cs` |
| Test Data Builders | `Tests/Builders/` |
| Test naming convention (`Method_Scenario_Expected`) | `Tests/DomainTests/`, `Tests/ApplicationTests/`, `Tests/APITests/` |
| Decision framework for switch vs. Strategy vs. factory | `CLAUDE.md` → Rule 10 |
| Centralized message strings, one class per layer (not a shared project) | `Domain/Constants/ValidationMessages.cs`, `Application/Constants/ValidationMessages.cs` |
| Global exception handler (RFC 7807 `ProblemDetails`, catch-all for unexpected errors) | `Presentation/API/Middleware/GlobalExceptionHandler.cs`, `Presentation/MinimalAPI/Middleware/GlobalExceptionHandler.cs` |
| Trace ID vs. correlation ID, propagated via `ILogger.BeginScope` (two structured-logging libraries compared side by side) | `Presentation/API/Middleware/RequestLoggingMiddleware.cs` (Serilog), `Presentation/MinimalAPI/Middleware/RequestLoggingMiddleware.cs` (NLog) |
| Distributed trace continuation across two "services" over HTTP (forwarded `TraceId`/`CorrelationId` headers continue the trace instead of each service minting its own) | `Presentation/API/Controllers/OrdersController.cs`, both `RequestLoggingMiddleware.cs`, `Presentation/MinimalAPI/Endpoints/LoadLogsForTestEndpoint.cs` |
| JWT bearer authentication (`/auth/token`, `[Authorize]`/`.RequireAuthorization()`) | `Presentation/API/Auth/`, `Presentation/API/Controllers/AuthController.cs`, `Presentation/MinimalAPI/Endpoints/AuthEndpoints.cs` |
| Fixed-window rate limiting, keyed by user vs. IP (partition-key logic pulled out for testability) | `Presentation/API/RateLimiting/RateLimitPartitionKeyResolver.cs`, `Presentation/MinimalAPI/RateLimiting/RateLimitPartitionKeyResolver.cs` |
| A real frontend consumer (login → JWT → protected call, client-side token expiry) | `Presentation/WebApp/src/LoginForm.jsx`, `Presentation/WebApp/src/QuoteForm.jsx` |

## Architecture

Clean Architecture with strictly inward-pointing dependencies, each layer its own project so the boundary is compiler-enforced:

```
Presentation/API, Presentation/MinimalAPI  →  Application  →  Domain
                                            ↘  Infrastructure ↗
Tests  →  Domain, Application, Infrastructure
```

- **`Domain/`** — `Order`, `Item`, `Quote`. No dependencies on any other project or framework. `Order`/`Item` now own their invariant checks (`string? Validate()`) and calculations (`CalculateSubtotal()`) as entity behavior, not plain property bags. `Constants/ValidationMessages` centralizes its error message strings.
- **`Application/`** — use-case orchestration, split by kind: `Services/` (`IQuoteService`/`QuoteService`), `Results/` (`Result<TValue>` — single-generic outcome wrapper with a `string ErrorMessage` for expected failures), `Constants/ValidationMessages` (its own centralized message strings). References `Domain` only; framework-agnostic.
- **`Infrastructure/`** — scaffolded, currently empty. Intended home for EF Core, repositories, and external clients.
- **`Presentation/API/`** and **`Presentation/MinimalAPI/`** — two parallel presentation layers (controller-based vs. Minimal API) solving the same use case side by side, each with its own `Dtos/` and `Mapper/` (`OrderMapper`, `ItemMapper`, `QuoteMapper`) for DTO ↔ domain translation. `API` exposes it via `OrdersController`; `MinimalAPI` exposes it via `Endpoints/OrderEndpoints.cs` (`POST /orders/quote`). Each also has its own `Middleware/GlobalExceptionHandler.cs` — a global handler (per Rule 8) that logs unexpected exceptions and returns a generic RFC 7807 `ProblemDetails` response. Both now also have: request logging with a trace ID + correlation ID (Serilog in `API`, NLog in `MinimalAPI`) that continues an inbound trace from forwarded headers instead of always minting a fresh one — demonstrated by `OrdersController` calling `MinimalAPI`'s `/diagnostics/load-logs-for-test` endpoint after preparing a quote, treating the two presentation projects as if they were separate services; JWT bearer auth (`/auth/token`, fixed demo credentials, protects the quote endpoint); and fixed-window rate limiting (3 requests/30s, by user on protected endpoints, by IP on the token endpoint). `API` also allows CORS from the React dev server.
- **`Presentation/WebApp/`** — a plain React + Vite client (not part of the .NET solution) with one form: log in (JWT), then request a quote. Manually verified in a browser, no automated tests.
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
8. Exceptions are only for the unexpected: expected failures (validation, business rules) return `Result<T>`, not exceptions; try/catch is for translating exceptions thrown by code you don't control (DB/HTTP/3rd-party) at its boundary; throw directly for invariant violations in your own code (e.g. a missing registration) — these are programmer errors, not caller-triggerable failures, so they don't get try/catch or `Result<T>` either; a global handler catches whatever's left.
9. Entity invariant validation lives on the entity itself as `string? Validate()` (`null` = valid), composed from private per-rule checks — no `out` params, no separate validation service.
10. Picking how to implement a variant/branching decision, ask: will the set of options grow (yes → dictionary/data lookup, not switch/if — OCP); does each option differ by algorithm or only by data (different algorithm → Strategy pattern, same algorithm/different data → plain lookup value); is the resolution reused across call sites or non-trivial to redo (yes → factory/resolver, single cheap call site → resolve inline via an injected collection). Then sanity-check the result against all five SOLID letters, not just OCP.
11. Test method names follow `MethodUnderTest_Scenario_ExpectedBehavior` (e.g. `Quote_WhenPrepareQuoteSucceeds_ShouldReturnOk`) — method name first, not the condition.
12. Message/exception strings are centralized per layer into a `Constants/ValidationMessages` static class (e.g. `Domain.Constants.ValidationMessages`, `Application.Constants.ValidationMessages`) — not a single shared project — so each layer stays self-contained (Domain keeps zero dependencies).
13. Tests assert against inline string literals, never against the same centralized message constant the production code reads from — asserting against the same constant makes the test tautological, since a changed message would still pass.
14. Cross-cutting request-scoped context (trace ID, correlation ID) is threaded through `ILogger.BeginScope` at the Presentation-layer middleware, not passed as method parameters — it then shows up automatically in any layer's own injected `ILogger<T>` calls without changing that layer's method signatures.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) — only needed for `Presentation/WebApp` (the React client)

No database is required — `Infrastructure` is currently an empty scaffold. JWT/rate-limiting config for both .NET projects lives in `appsettings.json` with demo defaults, so neither needs environment variables. `Presentation/WebApp` does need one — see Getting started below.

## Getting started

```bash
git clone <repo-url>
cd dotNet-arch-template
dotnet restore dotNet-arch-template.slnx
```

### Build

```bash
dotnet build dotNet-arch-template.slnx
```

The whole solution builds clean, including `Presentation/API` and `Tests`.

### Run

```bash
dotnet run --project Presentation/API/API.csproj
dotnet run --project Presentation/MinimalAPI/MinimalAPI.csproj
```

In Development, both apps expose the same OpenAPI doc through three UIs side by side (for comparison, not a recommendation to use all three): `/swagger/index.html` (Swagger UI), `/scalar` (Scalar), and the raw `/openapi/v1.json` document.

Both require a JWT for the quote endpoint: `POST /auth/token` (`{"username":"demo","password":"demo123"}` by default, see `appsettings.json`) returns a token to send as `Authorization: Bearer <token>`.

`API`'s quote endpoint also calls out to `MinimalAPI` (`Services:MinimalApiBaseUrl` in `Presentation/API/appsettings.json`, default `http://localhost:5110`) to demonstrate cross-service trace correlation — run both for the quote endpoint to work.

```bash
cd Presentation/WebApp
cp .env.example .env   # sets VITE_API_BASE_URL — defaults to http://localhost:5023, matching Presentation/API
npm install
npm run dev   # http://localhost:5173 — requires Presentation/API running on http://localhost:5023
```

### Test

```bash
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~QuoteServiceTests"   # run one test class
```

## Status

`Application/QuoteService.PrepareQuote` is implemented: it validates the order via `Order.Validate()`, computes `Subtotal` via `Order.CalculateSubtotal()`, applies `SAVE10`/`SAVE20` coupon rules with dollar thresholds, and computes `Total` — returning `Result<Quote>` throughout instead of throwing, fully covered by `Tests/ApplicationTests/Services/QuoteServiceTests.cs`. DTO ↔ domain mapping exists for `Presentation/API` (`API.Mapper`, covered by `Tests/APITests/Mapper`) and `OrdersController` uses it correctly, covered by `Tests/APITests/Controllers/OrdersControllerTests.cs` (via `Moq`). `Presentation/MinimalAPI` now has its own `Mapper/` and a `POST /orders/quote` endpoint (`Endpoints/OrderEndpoints.cs`), not yet covered by tests. Both presentation projects now have a `GlobalExceptionHandler` wired into `Program.cs`, each fully covered by its own `GlobalExceptionHandlerTests`.

Both presentation projects also now have: request logging with a trace ID + correlation ID (`RequestLoggingMiddleware`, unit tested) that continues an inbound trace from forwarded `X-Trace-Id`/`X-Correlation-Id` headers instead of always minting a fresh one; `OrdersController` calls `MinimalAPI`'s `/diagnostics/load-logs-for-test` endpoint (a manual-verification tool, not unit tested) after preparing a quote to demonstrate this across the two presentation projects. JWT bearer auth (`JwtTokenGenerator` + `AuthController`/`AuthEndpoints`, unit tested; fixed demo credentials, no real user store); and fixed-window rate limiting (`RateLimitPartitionKeyResolver`, unit tested; the actual throttling behavior itself is framework code and isn't). `Presentation/WebApp` is a React client with a login-then-quote flow against `API`, verified manually in a browser — no automated tests.
