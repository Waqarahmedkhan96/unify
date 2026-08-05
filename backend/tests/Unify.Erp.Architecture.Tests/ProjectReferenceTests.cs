using System.Reflection;
using Unify.Erp.Api;
using Unify.Erp.Application;
using Unify.Erp.Contracts.System;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Infrastructure;

namespace Unify.Erp.Architecture.Tests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void Domain_does_not_reference_outer_layers()
    {
        var referencedAssemblies = typeof(Organisation).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("Unify.Erp.Application", referencedAssemblies);
        Assert.DoesNotContain("Unify.Erp.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("Unify.Erp.Api", referencedAssemblies);
    }

    [Fact]
    public void Application_does_not_reference_api_or_infrastructure()
    {
        var referencedAssemblies = typeof(Application.DependencyInjection).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("Unify.Erp.Api", referencedAssemblies);
        Assert.DoesNotContain("Unify.Erp.Infrastructure", referencedAssemblies);
    }

    [Fact]
    public void Api_is_the_composition_root()
    {
        var apiReferences = typeof(ApiAssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.Contains("Unify.Erp.Application", apiReferences);
        Assert.Contains("Unify.Erp.Infrastructure", apiReferences);
        Assert.Contains("Unify.Erp.Contracts", apiReferences);
    }

    [Fact]
    public void Architecture_test_project_loads_all_foundation_assemblies()
    {
        var assemblies = new[]
        {
            typeof(ApiAssemblyMarker).Assembly,
            typeof(Application.DependencyInjection).Assembly,
            typeof(Organisation).Assembly,
            typeof(Infrastructure.DependencyInjection).Assembly,
            typeof(HealthResponse).Assembly
        };

        Assert.All(assemblies, assembly => Assert.NotNull(assembly.GetName().Name));
    }
}
