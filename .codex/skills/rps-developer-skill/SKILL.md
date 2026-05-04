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
- Target `.NET 10` or higher for all new implementation work unless explicitly overridden by the user.
- Code must be clean and maintainable:
  - intention-revealing names
  - small focused methods
  - explicit control flow
  - no dead code or unclear shortcuts

## Mandatory Architecture and Coding Rules

- SDK HTTP consumption must be REST-oriented only.
- Never use AutoMapper.
- Always use custom manual mapping through static extension methods.
- Do not introduce any new third-party dependency unless the user has explicitly approved it.
- If a new third-party dependency is approved, use the newest available compatible stable version that does not conflict with existing dependency constraints.
- Keep exactly one class per file.
- Treat public, non-base implementations as SDK building blocks with self-documenting naming.

## XML Documentation Requirement

- Add XML documentation comments to all classes, interfaces, properties, and members (private and public).
- Public SDK-facing members require extra care:
  - explain intent and behavior clearly,
  - document parameters, return values, side effects, and exceptions where applicable,
  - write comments from the SDK consumer perspective so developers can understand how to use the API safely.

## Mapping Rule (Required Pattern)

Use explicit static extension mapping.

```csharp
public static class BookDtoMappingExtensions
{
    public static Book ToDomain(this BookDto dto)
    {
        return new Book(
            id: dto.Id,
            title: dto.Title,
            author: dto.Author);
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
- Testing complete:
  - Unit tests added/updated using `rps-dotnet-testing` guidance.
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
