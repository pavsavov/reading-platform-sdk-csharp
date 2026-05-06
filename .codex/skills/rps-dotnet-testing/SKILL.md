---
name: rps-dotnet-testing
description: Create, update, or review .NET unit tests for this SDK with strict testing standards. Use when writing new tests, expanding coverage, refactoring tests, fixing regressions, or validating security and business logic behavior in xUnit-based test suites.
---

# rps-dotnet-testing

## Goal

Produce comprehensive, maintainable unit tests for this repository using the required .NET testing stack and conventions.

## Mandatory Tooling

Use all of the following unless the user explicitly overrides:

- `xUnit` as the test framework.
- `AwesomeAssertions` for assertions.
- `NSubstitute` for mocks, stubs, and interaction checks.
- `Bogus` for deterministic dummy data generation.

Do not introduce alternative testing frameworks or mocking/assertion libraries without explicit user approval.

## Test Organization Workflow

1. Identify the test target and classify behavior into:
   - business logic paths
   - security-relevant paths
   - error/edge conditions
2. Organize tests by feature and method, keeping file names and test names explicit about behavior.
3. Follow Arrange-Act-Assert consistently.
4. Use one behavioral assertion focus per test when practical; add multiple assertions only when they describe one business rule.
5. Prefer readable test data builders and `Bogus` generators over hand-written repeated literals.

## Coverage Expectations

For each public behavior under test, include:

- happy path behavior
- boundary and invalid input behavior
- exception and failure behavior
- relevant side effects and interactions

### Security Evaluation Is Mandatory

When applicable, test security-oriented behaviors such as:

- authentication token/header propagation
- authorization-related branching
- input validation and guard clauses
- sensitive data handling in errors/logging
- unsafe default behavior regressions

If a security category is not applicable, state why in comments or PR notes.

## Assertion and Mocking Guidance

- Use `AwesomeAssertions` fluent syntax for clarity.
- Use `NSubstitute` to verify interactions with collaborators and to control external dependencies.
- Avoid over-mocking pure logic.
- Keep mocks focused on collaboration boundaries (HTTP clients, providers, repositories, clock, etc.).

## Data Generation Guidance

- Use `Bogus` with stable seeds where reproducibility matters.
- Generate realistic objects and vary values to prevent false confidence from static fixtures.
- Keep generated data intention-revealing by wrapping generators in helper methods when repeated.

## Exit Criteria

Before finishing, ensure:

- tests compile and run
- both business logic and security behavior are evaluated
- coverage includes success, failure, and edge scenarios
- assertions are clear and deterministic
- no required library (`xUnit`, `AwesomeAssertions`, `NSubstitute`, `Bogus`) is missing
- test coverage is at least 80% of new code
