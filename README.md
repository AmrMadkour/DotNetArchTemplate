# DotNetArchTemplate

A personal, standing reference solution for .NET architecture patterns — Clean Architecture, DDD, and TDD — built with **.NET 10**. It exists as a place to see how a given pattern was solved before, rather than a shippable product, so parts of it are deliberately left mid-refactor or unimplemented. See [`CLAUDE.md`](CLAUDE.md) for the full rationale and known rough edges.

## Architecture

Clean Architecture with strictly inward-pointing dependencies, each layer its own project so the boundary is compiler-enforced:

```
Presentation/API, Presentation/MinimalAPI  →  Application  →  Domain
                                            ↘  Infrastructure ↗
Tests  →  Domain, Application, Infrastructure
```

- **`Domain/`** — POCOs only (`Order`, `Item`, `Quote`). No dependencies on any other project or framework.
- **`Application/`** — use-case orchestration (`IQuoteService`/`QuoteService`). References `Domain` only; framework-agnostic.
- **`Infrastructure/`** — scaffolded, currently empty. Intended home for EF Core, repositories, and external clients.
- **`Presentation/API/`** and **`Presentation/MinimalAPI/`** — two parallel presentation layers (controller-based vs. Minimal API) solving the same use case side by side, each with its own `Dtos/`. `API` also has a `Mapper/` folder (`OrderMapper`, `ItemMapper`, `QuoteMapper`) for DTO ↔ domain translation; `MinimalAPI` doesn't have one yet.
- **`Tests/`** — one xUnit project, organized into per-layer folders, with fluent Test Data Builders under `Builders/`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

No database or external services are required — `Infrastructure` is currently an empty scaffold, and no environment variables need to be configured.

## Getting started

```bash
git clone <repo-url>
cd DotNetArchTemplate
dotnet restore DotNetArchTemplate.slnx
```

### Build

```bash
dotnet build DotNetArchTemplate.slnx
```

> **Note:** this currently fails with `CS1503` in `Presentation/API/Controllers/OrdersController.cs` (the mapped `Order` isn't passed into `PrepareQuote` yet) — a known, intentionally parked mid-refactor stub (see `CLAUDE.md`). `Domain`, `Application`, `Infrastructure`, and `Presentation/MinimalAPI` build clean on their own; build them individually with `dotnet build <ProjectPath>` if you want to skip `API`. **`Tests` also fails to build** for the same reason, since it references `Presentation/API` — see the Test section below.

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

> **Note:** `Tests` references `Presentation/API` (for `Tests/APITests/Mapper`), so it currently fails to build for the same `CS1503` reason as `API` — see the Build note above. Both commands will work again once `OrdersController` compiles.

## Status

`Application/QuoteService.PrepareQuote` is an intentional stub being driven out via TDD, one rule at a time, starting from `Tests/ApplicationTests/QuoteServiceTests.cs`. DTO ↔ domain mapping now exists for `Presentation/API` (`API.Mapper`, covered by `Tests/APITests/Mapper`) and `OrdersController` is mid-wire-up to use it; `Presentation/MinimalAPI` has no mapping yet.
