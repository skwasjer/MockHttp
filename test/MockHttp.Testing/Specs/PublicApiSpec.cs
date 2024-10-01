﻿#if VERIFY_PUBLIC_API
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using PublicApiGenerator;

namespace MockHttp.Specs;

[UsesVerify]
public abstract class PublicApiSpec
{
    private readonly Type _assemblyMarkerType;
    private readonly string? _sourceFile;

    private readonly ApiGeneratorOptions _options = new()
    {
        ExcludeAttributes =
        [
            typeof(InternalsVisibleToAttribute).FullName!
        ],
        DenyNamespacePrefixes = []
    };

    protected PublicApiSpec(Type assemblyMarkerType, [CallerFilePath] string? sourceFile = default)
    {
        _assemblyMarkerType = assemblyMarkerType;
        _sourceFile = sourceFile;
    }

    [Fact]
    [Trait("Category", "PublicApi")]
    public Task Api_has_not_changed()
    {
        bool runningOnMono = Type.GetType("Mono.Runtime") != null;
        if (runningOnMono)
        {
            return Task.CompletedTask;
        }

        Assembly sut = _assemblyMarkerType.Assembly;
        var settings = new VerifySettings();

        string targetFramework = sut.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName?.Replace(' ', '_')
            ?? throw new InvalidOperationException("Framework display name is required.");
        settings.UseFileName(targetFramework);
        settings.UseDirectory("PublicApi");

        // Act
        string publicApi = sut.GeneratePublicApi(_options);

        // Assert
        // ReSharper disable once ExplicitCallerInfoArgument
#pragma warning disable S3236
        return Verify(publicApi, settings, _sourceFile!);
#pragma warning restore S3236
    }
}
#else
namespace MockHttp.Specs;

// Empty shim for when verification is disabled.
public abstract class PublicApiSpec
{
    // ReSharper disable once UnusedParameter.Local
    protected PublicApiSpec(Type assemblyMarkerType)
    {
    }
}
#endif
