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

- Before starting implementation work, request a ticket from GitHub using the installed GitHub plugin in Codex.
- Treat the ticket as the authoritative work item for scope, tracking, and commit/PR linkage.

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
- Do not introduce any new third-party dependency unless the user has explicitly approved it.
- If a new third-party dependency is approved, use the newest available compatible stable version that does not conflict with existing dependency constraints.
- Keep exactly one class per file.
- Treat public, non-base implementations as SDK building blocks with self-documenting naming.

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
  - Customer-facing changes expressed in Release Please-compatible commit/PR metadata.
  - Example `.csx` added/updated for each newly exposed public client.
- Policy compliance complete:
  - No AutoMapper usage.
  - Manual static extension mapping used.
  - No new dependency without explicit consent.
  - One class per file preserved.
  - REST-oriented client behavior preserved.

## Out-of-Scope Guardrails

- Do not add non-REST transport clients as SDK consumption alternatives.
- Do not bypass custom exception modeling with generic exceptions for domain-specific failures.
- Do not weaken naming clarity or collapse multiple responsibilities into one class.
