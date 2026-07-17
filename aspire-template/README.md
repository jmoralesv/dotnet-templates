# Clean Architecture Template

A .NET 10.0 solution template built with Clean Architecture, Aspire, and Central Package Management.

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 (or any IDE that supports .NET 10.0 and the new `.slnx` solution format)

## Central Package Management

This repository uses [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management) to keep NuGet package versions consistent across all projects:

- `Directory.Build.props` — shared MSBuild properties such as `TargetFramework`, nullable reference types, code-analysis settings, and common package references (e.g. `SonarAnalyzer.CSharp`).
- `Directory.Packages.props` — single source of truth for every package version used in the solution. All `.csproj` files reference packages without specifying a version.
- `nuget.config` — configures a single NuGet package source with package source mapping.

## What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, PostgreSQL
  - Serilog
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Testing projects
  - Architecture testing

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!
