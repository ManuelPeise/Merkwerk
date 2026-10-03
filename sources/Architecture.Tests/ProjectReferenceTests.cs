using Architecture.Tests.Infrastructure;

namespace Architecture.Tests;

/// <summary>The dependency table of AGENTS.md §3, checked against the project files in Merkwerk.slnx.</summary>
public sealed class ProjectReferenceTests
{
    [Fact]
    public void ProjectReferences_AllProductionProjects_FollowDependencyTable()
    {
        var violations = SolutionProjects.Production
            .SelectMany(project => project.References
                .Where(reference => !IsAllowed(project.Name, reference))
                .Select(reference => $"{project.Name} -> {reference}"))
            .ToList();

        Assert.True(violations.Count == 0, "Not allowed by AGENTS.md §3: " + string.Join(", ", violations));
    }

    [Fact]
    public void LogicAssemblies_AllLogicProjects_AreCoveredByArchitectureTests()
    {
        var logicProjects = SolutionProjects.Production
            .Where(p => p.Name.StartsWith("Logic.", StringComparison.Ordinal))
            .Select(p => p.Name)
            .Order()
            .ToList();

        var covered = Assemblies.Logic.Select(a => a.GetName().Name!).Order().ToList();

        Assert.Equal(logicProjects, covered);
    }

    private static bool IsAllowed(string project, string reference) => project switch
    {
        "Shared" => false,
        "Logic.Shared" or "Data.Database" => reference is "Shared",
        "Data.Accessor" => reference is "Data.Database" or "Shared",
        "Web.Core" => reference.StartsWith("Logic.", StringComparison.Ordinal)
            || reference is "Shared" or "Data.Accessor" or "Data.Database",
        "Logic.Notifications" => reference is "Logic.Shared" or "Shared",
        "Logic.Authentication" => reference is "Logic.Shared" or "Logic.Notifications" or "Data.Accessor" or "Shared",
        _ when project.StartsWith("Logic.", StringComparison.Ordinal) =>
            reference is "Logic.Shared" or "Logic.Notifications" or "Logic.Authentication" or "Data.Accessor" or "Shared",
        _ => false,
    };
}
