/// <summary>
/// Exposes SDK internal types and members to the SDK test project so tests can validate
/// internal behavior without widening the public API surface.
/// </summary>
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PublishingPlatform.SDK.Tests")]
/// <summary>
/// Exposes SDK internals to Castle DynamicProxy (used by common mocking frameworks) so
/// tests can create proxies/mocks for internal abstractions.
/// </summary>
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
