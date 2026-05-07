---
name: rps-developer-skill
description: Govern day-to-day SDK implementation with mandatory quality, architecture, testing, documentation, and release-compatible rules for customer-safe changes.
---

# rps-developer-skill

## Goal

Enforce consistent implementation standards for this SDK so all feature and refactor work is maintainable, testable, REST-oriented, and safe for external consumers.

## Scope

Use this skill when implementing or refactoring production SDK code, public abstractions, clients, and user-facing models.

## Required First Step

- Use the user-provided task/ticket context as the authoritative work item for scope and implementation decisions.
- For security/governance policy, rely on this skill's embedded baseline (Ticket #22 and #23 content below) without performing GitHub lookups.
- Query GitHub only when the user explicitly asks, or when a different ticket is required and not already embedded in this skill.
- Preserve ticket linkage in commit/PR metadata when a ticket id is available.

## Mandatory Security and Governance Baseline

- The policy in this section is a direct embedded baseline from Ticket #22 and Ticket #23 and is non-optional for all SDK development.

### Core Security Principles (Ticket #23)

- Secure by default.
- Opt-in diagnostics.
- No embedded secrets.
- Least privilege.
- Assume code is inspectable/decompilable.
- Prefer server-side enforcement over client trust.

### Required Controls ()

- Secrets management:
  - Never store, hardcode, or embed secrets in SDK code, tests, examples, fixtures, docs, or pipelines.
  - Consumer credentials must come from secure external configuration (environment variables/secret managers).
- Logging and diagnostics:
  - Diagnostics are disabled by default.
  - Diagnostics are explicitly enabled by consumers.
  - Sensitive data is excluded by default and included only by explicit opt-in.
  - Never log by default: API keys/tokens, auth headers, content payloads, download URLs, user/customer identifiers.
- Transport security:
  - Enforce HTTPS-only communication.
  - Reject insecure `http://` base URLs at runtime.
  - Example enforcement:
```csharp
if (options.BaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("HTTPS is required.");
}
```
- Correlation and tracing:
  - Support correlation IDs on requests for observability (`X-Correlation-Id`).
- Idempotency for mutating operations:
  - Support idempotency keys for retry-safe publish/upload operations to prevent duplication.
- Input validation:
  - Validate required inputs before API calls and fail fast with explicit argument errors.
- Error handling:
  - Use strongly typed exceptions that include structured context (status code, error code, request id where applicable).
- CI/CD security:
  - Pipeline must include build, unit tests, dependency vulnerability checks, static analysis, and docs validation.
  - Use least-privilege GitHub Actions permissions by default:
```yaml
permissions:
  contents: read
```
  - Escalate permissions only for jobs that truly require it (for example release publishing).
- Package security:
  - Publish packages only from CI/CD, not local machines.
  - Keep release credentials in secure CI secret stores only.
  - Consider signing packages where feasible.
- Dependency security:
  - Run vulnerability checks regularly (for example `dotnet list package --vulnerable`).
- Security boundary principle:
  - Do not rely on SDK secrecy/obfuscation as primary protection.
  - Backend API is the real security boundary and must enforce authentication, authorization, rate limits, and data access control.

### Distribution and Debug Surface Hardening ()

- Objective:
  - Reduce source visibility and debugger stepping surface for external SDK consumers.
  - Recognize the hard limit: .NET assemblies are inspectable; complete prevention is impossible.
- Distribution policy:
  - Keep repository private.
  - Distribute only NuGet artifacts (`.nupkg`) unless explicitly overridden by user decision.
- Artifact exposure policy:
  - Do not publish symbols/source artifacts by default (`.snupkg`, `.pdb`, SourceLink).
- Packaging hardening defaults (must remain enabled unless explicitly approved override):
```xml
<PropertyGroup>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <PublishRepositoryUrl>false</PublishRepositoryUrl>
  <EmbedUntrackedSources>false</EmbedUntrackedSources>
</PropertyGroup>
```
- Debugger stepping hardening:
  - Review sensitive implementation classes and apply `[DebuggerStepThrough]` or `[DebuggerNonUserCode]` where appropriate.
- Documentation truthfulness:
  - Documentation must state the inspection limits clearly (cannot fully prevent reverse inspection/decompilation).

## Mandatory Quality Baseline

- Follow SonarCloud default C# rules as the baseline quality standard.
- Authoritative rules reference: https://sonarcloud.io/organizations/pavsavov/rules?languages=cs
- Explicitly enforce `roslyn:CA1822` ("Mark members as static") when members do not access instance state.
- Target `.NET 10` or higher for all new implementation work unless explicitly overridden by the user.
- Code must be clean and maintainable:
  - intention-revealing names
  - small focused methods
  - explicit control flow
  - no dead code or unclear shortcuts
- Do not use fully-qualified framework type names inline when a `using` directive is appropriate.
  - Example: avoid `System.Runtime.CompilerServices.EnumeratorCancellation` inline in signatures.
  - Preferred: add the `using` and reference `[EnumeratorCancellation]`.
  - If there is an edge case (for example ambiguity or symbol conflict), explicitly prompt before keeping the fully-qualified form.
- Always add the required `using` directives for SDK and framework types instead of writing fully-qualified type names in code signatures or method bodies.
  - Example: avoid `Task<PublishingPlatform.SDK.Models.BookContent>`.
  - Preferred: add `using PublishingPlatform.SDK.Models;` and write `Task<BookContent>`.
- If a type name collision exists (for example namespace vs model name), use a `using` alias instead of inline fully-qualified types.
  - Example: `using BookContentModel = PublishingPlatform.SDK.Models.BookContent;` then use `Task<BookContentModel>`.

## Mandatory Architecture and Coding Rules

- SDK HTTP consumption must be REST-oriented only.
- Never use AutoMapper.
- Always use custom manual mapping through static extension methods.
- Avoid redundant constructor null guards for dependencies resolved exclusively by DI:
  - Do not add `ArgumentNullException.ThrowIfNull(...)` for DI-only constructor parameters.
  - Rely on container resolution failures for missing registrations.
  - Keep null guards for inputs that can come from external/non-DI callers (for example public methods, factory inputs, builder configuration inputs).
- Do not introduce any new third-party dependency unless the user has explicitly approved it.
- If a new third-party dependency is approved, use the newest available compatible stable version that does not conflict with existing dependency constraints.
- Keep exactly one class per file.
- Treat public, non-base implementations as SDK building blocks with self-documenting naming.

## Refactor and Polishing Policy

Use this section for cleanup work that improves maintainability without changing intended public behavior.

- Preserve public API compatibility unless the ticket explicitly approves a breaking change.
- Keep refactors behavior-preserving and covered by existing or added regression tests.
- Prefer simplifying existing collaborators over introducing new abstractions.
- Do not mix polish with unrelated feature work.
- Do not rewrite working architecture just for style consistency.
- When touching validation, mapping, transport, diagnostics, or serialization, preserve the existing responsibility boundaries.
- If a refactor changes observable behavior, document it as a behavior change, not polish.
- Public SDK entry points must fail fast for obvious invalid consumer arguments before issuing HTTP requests.
- Use internal validation helpers for generic argument checks:
  - null request objects
  - null streams
  - null required objects
  - null, empty, or whitespace identifiers
  - invalid pagination bounds
- Prefer `CallerArgumentExpression`-based helper methods so parameter names stay accurate without repeated `nameof(...)` boilerplate.
- Keep throw helpers internal; do not expose validation helpers as public SDK API.
- Generic argument faults should use standard argument exceptions:
  - `ArgumentNullException` for null required objects
  - `ArgumentException` for empty or whitespace required strings
  - `ArgumentOutOfRangeException` for invalid numeric ranges
- Keep module/domain validation in dedicated validator collaborators.
- Use SDK/domain exceptions, such as `BookValidationException`, only for SDK-owned request semantics or response-shape validation, not for simple null argument guards.
- Do not validate backend-owned business rules in the SDK.

Refactor completion checklist:

- Public contracts reviewed for accidental signature, default, or exception changes.
- Regression tests prove behavior stayed equivalent or intentionally changed.
- Existing architecture boundaries preserved or improved.
- No unrelated formatting churn.
- No new dependency introduced without explicit approval.
- PR notes explain the cleanup intent and any deliberate tradeoffs.

## Architecture Responsibility Budget (Mandatory)

- Every new or changed class must declare one primary responsibility in a single line:
  - either in a short class intent comment,
  - or in the PR notes for that class.
- If a class starts handling 2 or more concern buckets, extract collaborators before merge.
- Allowed concern buckets:
  - orchestration
  - validation
  - mapping/serialization
  - transport/pipeline
  - diagnostics/telemetry
  - policy/configuration
- A class may coordinate multiple collaborators, but may not implement multiple concern buckets directly.

## SOLID Enforcement Rules (Mandatory)

- `S` - Single Responsibility:
  - one reason to change per class.
  - extraction is required when class code combines:
    - validation + transport execution,
    - query/header construction + response parsing,
    - domain policy + serialization logic.
- `O` - Open/Closed:
  - extend behavior via new strategy/factory/builder components.
  - do not expand large switch/if chains in core public clients when adding variants.
- `L` - Liskov Substitution:
  - replacement implementations must preserve contract behavior, including:
    - exception categories,
    - null/empty handling,
    - cancellation behavior.
- `I` - Interface Segregation:
  - prefer focused interfaces (for example `IBookQueryStringBuilder`) over broad utility interfaces.
  - avoid interfaces that force unrelated members onto one implementation.
- `D` - Dependency Inversion:
  - top-level clients depend on abstractions.
  - concrete composition belongs in DI/factory wiring, not in core orchestration classes.

## Layered Client Pattern Contract (Mandatory)

- Public client classes (for example `BooksClient`) are orchestration-only.
- Validation, query construction, header policy, and response reading must be separate collaborators.
- Transport layer must not implement domain validation or business policy branching.
- Provider adapter layers (for example GoogleBooks) must stay internal.
- Provider-specific types must never leak into public SDK contracts.

## Factory + Builder + Fluent Policy (Mandatory)

- Use static factories for internal default composition:
  - `CreateDefault()` for dependency bundles or default policy sets.
- Use builder pattern when object construction has optional strategies/policies:
  - `...Builder.Create().With...().Build()`.
- Fluent request builders are additive and optional for consumers.
- Validation rules stay centralized in validator components, not spread across builders.
- Do not create "god builders" that construct + validate + serialize + execute calls.

## Refactor Trigger Thresholds (Mandatory)

- Extraction is required when any of the following are true in a changed class:
  - class includes both transport calls and query/header building;
  - class includes both validation and JSON mapping;
  - class includes paging iteration plus response parsing plus policy headers.
- Additional extraction threshold for orchestration clients:
  - more than 8 methods total, or
  - more than 4 private helper methods.
- If a threshold is exceeded, either:
  - extract collaborators before merge, or
  - document a short, explicit architectural exception in PR notes with planned follow-up.

## Naming Convention Rules

- Public SDK contracts must use consumer-friendly names:
  - command/query inputs: `*Request`
  - explicit outputs/envelopes: `*Response` (when needed)
  - core domain entities: clear nouns (for example `Book`).
- Internal provider wire models must not leak into public surface:
  - prefer `*Payload` for provider JSON/wire shapes
  - provider-specific names must stay internal only.
- Avoid exposing `*Dto` types as public SDK contracts.

## Magic Strings and Constants Policy (Mandatory)

- Do not leave repeated business-significant string literals inline ("magic strings").
- Default placement rule: keep constants as close as possible to usage.
  - Use `private const` (or `private static readonly` when `const` is not possible) when a value is used in only one class/file.
- Promote to shared constants only when the same value is used across multiple classes or modules.
  - Shared constants must be grouped by domain/type (for example headers, routes, claim names, error codes), not in a single global "Constants" dump file.
- Configuration-like values must not be hardcoded constants.
  - Use `appsettings` + `IOptions<T>` for values expected to vary by environment, deployment, or tenant.
- Prefer stronger typing over string literals when modeling bounded value sets.
  - Use enums, dedicated value objects, or typed wrappers for domain states/codes.
- Prefer `nameof(...)` for member/parameter/property names instead of hardcoded identifier strings.
- If introducing a new shared constant group, name it by intent and scope (for example `HeaderNames`, `ClaimTypes`, `ErrorCodes`) and keep it cohesive.

Anti-patterns (must avoid):

- One catch-all `Constants.cs` containing unrelated values.
- Duplicating the same literal across multiple files.
- Hardcoded environment/configuration values in production code.

## URL Construction Policy (Mandatory)

- Do not scatter full URL string literals across the codebase.
- Base URLs are configuration, not constants:
  - Store host/base address in `appsettings` and bind via `IOptions<TOptions>`.
- Keep route definitions close to the consuming domain/client.
  - Group endpoint templates by responsibility (for example `BooksEndpoints`, `UsersEndpoints`), not in a global endpoints file.
- For parameterized routes, use typed builder methods instead of ad-hoc string concatenation.
  - Example pattern: `ById(string id)`, `Chapters(string id, int chapter)`.
- Always encode dynamic path segments using `Uri.EscapeDataString(...)`.
- Build absolute URIs via `Uri` composition (base + relative), not manual slash-joining.
- Build query strings with a dedicated helper/builder (for example `QueryHelpers`, `FormUrlEncodedContent`, or a small internal query builder), not manual `"&"` concatenation.
- Omit optional query parameters when values are null/empty/default unless the external API contract explicitly requires sending them.
- Keep endpoint route constants/methods internal unless explicitly needed by public SDK contracts.
- Route builders must stay transport-focused and must not contain business validation logic.

Anti-patterns (must avoid):

- `string` concatenation for full URLs (`base + "/" + path + "?" + ...`).
- Repeating the same route fragments in multiple classes.
- Leaving dynamic path values unencoded.
- Mixing route construction and domain-policy branching in the same method/class.

## XML Documentation Requirement

- Add XML documentation comments to all classes, interfaces, properties, and members (private and public).
- Public SDK-facing members require extra care:
  - explain intent and behavior clearly,
  - document parameters, return values, side effects, and exceptions where applicable,
  - write comments from the SDK consumer perspective so developers can understand how to use the API safely.

## Mapping Rule (Required Pattern)

Use explicit static extension mapping.

```csharp
public static class BookPayloadMappingExtensions
{
    public static Book ToDomain(this BookPayload payload)
    {
        return new Book(
            id: payload.Id,
            title: payload.Title,
            author: payload.Author);
    }
}
```

## Exceptions Rule (Required Pattern)

Every meaningful fault scenario must use a custom exception type that explains the problem and its context.

```csharp
public sealed class BookPublishingFailedException : Exception
{
    public BookPublishingFailedException(string bookId, string reason)
        : base($"Publishing failed for book '{bookId}'. Reason: {reason}")
    {
        BookId = bookId;
        Reason = reason;
    }

    public string BookId { get; }
    public string Reason { get; }
}
```

## Static Factory and Builder Guidance

Public building blocks should expose clear creation entry points via static factories, and support progressive composition via a Builder where useful.

Static factory example:

```csharp
public sealed class PublishingRequest
{
    private PublishingRequest(string bookId, string channel)
    {
        BookId = bookId;
        Channel = channel;
    }

    public string BookId { get; }
    public string Channel { get; }

    public static PublishingRequest ForBook(string bookId)
    {
        return new PublishingRequest(bookId, channel: "default");
    }
}
```

Builder example:

```csharp
public sealed class PublishingRequestBuilder
{
    private string _bookId = string.Empty;
    private string _channel = "default";

    public static PublishingRequestBuilder Create()
    {
        return new PublishingRequestBuilder();
    }

    public PublishingRequestBuilder WithBookId(string bookId)
    {
        _bookId = bookId;
        return this;
    }

    public PublishingRequestBuilder WithChannel(string channel)
    {
        _channel = channel;
        return this;
    }

    public PublishingRequest Build()
    {
        return new PublishingRequest(_bookId, _channel);
    }
}
```

## Testing Requirement

- Unit tests are mandatory for changed behavior.
- Use the `rps-dotnet-testing` skill for all test implementation and test review work.
- For architecture-sensitive changes, include collaborator seam tests (not only end-to-end tests):
  - validator branch tests
  - query builder deterministic output tests
  - header factory precedence tests
  - response reader null/error handling tests
- For each newly extracted responsibility, add at least one regression test that prevents logic from collapsing back into orchestration classes.
- Required test coverage includes:
  - happy path
  - edge/invalid paths
  - exception/failure behavior
  - relevant security-oriented behavior where applicable

## Documentation and Release Responsibilities

- For customer-facing SDK changes (public contracts, public behavior, exposed models/clients), provide release-note-ready change intent through Release Please-compatible conventional commit/PR metadata.
- Keep changelog outcomes consumer-focused and compatible with existing Release Please automation.
- Do not manually introduce a separate manual version-bump workflow.
- Update `README.md` when a new usage pattern, library decision, or architecture decision affects SDK consumers.
- Every newly exposed public client must include a simple consumer example in `examples/` as a C# script (`.csx`) plus minimal usage notes.

## Execution Checklist (Must Pass Before Completion)

- API/contract impact reviewed:
  - Confirm whether public interfaces, signatures, models, defaults, or exception contracts changed.
- Security/governance baseline reviewed:
  - Embedded Ticket #22 and #23 controls were applied or explicitly exception-noted.
  - Security principles are preserved: secure-by-default, opt-in diagnostics, no secrets, least privilege, inspectable-code assumption, backend enforcement.
  - No secrets are embedded in SDK code, test assets, docs, or pipelines.
  - HTTPS-only transport enforcement preserved for SDK communication.
  - Diagnostics remain opt-in and sensitive-data-safe by default.
  - Correlation id and idempotency support were preserved where mutating/traceable flows are touched.
  - Strong input validation and typed exception behavior were preserved for changed flows.
  - Backend-enforcement principle preserved (no client-side secrecy assumptions).
  - CI/CD security gates include vulnerability/static-analysis coverage for affected changes.
  - Workflow/job permissions follow least privilege by default.
  - NuGet publishing path remains CI/CD-only and secret-safe.
  - Dependency vulnerability posture reviewed for affected dependency changes.
  - Package/source visibility policy remains hardened by default (`.nupkg` distribution, no symbol/source artifacts by default).
  - Sensitive classes touched by the change were reviewed for debugger-step surface hardening.
  - Documentation still states decompilation/inspection limits truthfully.
- Architecture responsibility budget reviewed:
  - Each changed class has exactly one declared primary responsibility.
  - Any multi-bucket behavior has been extracted or exception-noted.
- SOLID compliance reviewed:
  - `S/O/L/I/D` checks applied and pass/fail justified in PR notes when needed.
- Layering compliance reviewed:
  - Public clients are orchestration-only.
  - Transport has no domain validation.
  - Provider-specific types remain internal.
- Sonar architecture pre-flight completed:
  - Reviewed Sonar Architecture -> Split responsibilities on the PR branch.
  - Inspected top 3 highest-depth components touched by the PR.
  - If depth increased in touched namespaces, either refactored or documented accepted tradeoff.
- Architecture Delta PR note included with:
  - touched layers
  - extracted collaborators
  - remaining intentional coupling
- Testing complete:
  - Unit tests added/updated using `rps-dotnet-testing` guidance.
  - Architecture seam tests included where collaborators were introduced.
- Docs and release alignment complete:
  - `README.md` updated when consumer-relevant patterns/decisions changed.
  - Security/distribution guidance updated when packaging or diagnostics behavior changes.
  - Customer-facing changes expressed in Release Please-compatible commit/PR metadata.
  - Example `.csx` added/updated for each newly exposed public client.
- Policy compliance complete:
  - No AutoMapper usage.
  - Manual static extension mapping used.
  - No new dependency without explicit consent.
  - One class per file preserved.
  - REST-oriented client behavior preserved.
  - Packaging hardening and symbol/source exposure policy preserved unless explicitly approved override.

## Out-of-Scope Guardrails

- Do not add non-REST transport clients as SDK consumption alternatives.
- Do not bypass custom exception modeling with generic exceptions for domain-specific failures.
- Do not weaken naming clarity or collapse multiple responsibilities into one class.
