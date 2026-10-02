namespace Architecture.Tests;

/// <summary>
/// Enforces the project reference rules from AGENTS.md §3 and ADR 010 on the .csproj level.
/// Works without any code in the projects; type-level rules (e.g. Logic must not use MerkwerkDbContext)
/// are added with NetArchTest once the types exist (LP-101).
/// </summary>
public sealed class ProjectDependencyTests
{
    /// <summary>Allowed direct project references per production project. Test projects are not restricted.</summary>
    private static readonly IReadOnlyDictionary<string, string[]> AllowedReferences = new Dictionary<string, string[]>
    {
        ["Shared"] = [],
        ["Logic.Shared"] = ["Shared"],
        ["Data.Database"] = ["Shared"],
        ["Data.Accessor"] = ["Data.Database", "Shared"],
        ["Logic"] = ["Logic.Shared", "Shared", "Data.Accessor"],
        ["Logic.Authentication"] = ["Logic.Shared", "Shared", "Data.Accessor"],
        ["Service"] = ["Logic", "Logic.Authentication", "Shared"],
        ["Web.Client"] = ["Logic.Shared", "Shared"],
        ["Web"] = ["Web.Client", "Service", "Logic", "Data.Accessor", "Data.Database"],
    };

    private static readonly SolutionInfo Solution = SolutionInfo.Load();

    public static TheoryData<string> ProductionProjects()
    {
        var data = new TheoryData<string>();
        foreach (var project in Solution.Projects.Keys.Where(p => !IsTestProject(p)).Order())
        {
            data.Add(project);
        }

        return data;
    }

    [Fact]
    public void Solution_ContainsAllExpectedProjects()
    {
        var missing = AllowedReferences.Keys.Where(p => !Solution.Projects.ContainsKey(p)).ToList();

        Assert.True(missing.Count == 0, $"Projects missing from Merkwerk.slnx: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryProductionProject_HasADependencyRule()
    {
        var withoutRule = Solution.Projects.Keys
            .Where(p => !IsTestProject(p) && !AllowedReferences.ContainsKey(p))
            .ToList();

        Assert.True(
            withoutRule.Count == 0,
            $"New project(s) without a dependency rule – add them to AllowedReferences and AGENTS.md §3: {string.Join(", ", withoutRule)}");
    }

    [Theory]
    [MemberData(nameof(ProductionProjects))]
    public void Project_ReferencesOnlyAllowedProjects(string project)
    {
        if (!AllowedReferences.TryGetValue(project, out var allowed))
        {
            return; // reported by EveryProductionProject_HasADependencyRule
        }

        var forbidden = Solution.Projects[project].Except(allowed).ToList();

        Assert.True(forbidden.Count == 0, $"{project} must not reference: {string.Join(", ", forbidden)}");
    }

    [Theory]
    [InlineData("Web.Client", "Logic")]
    [InlineData("Web.Client", "Logic.Authentication")]
    [InlineData("Web.Client", "Data.Accessor")]
    [InlineData("Web.Client", "Data.Database")]
    [InlineData("Logic.Shared", "Data.Database")]
    [InlineData("Shared", "Data.Database")]
    public void Project_DoesNotReachProjectTransitively(string project, string forbidden)
    {
        var reachable = Solution.TransitiveReferences(project);

        Assert.DoesNotContain(forbidden, reachable);
    }

    [Theory]
    [InlineData("Logic")]
    [InlineData("Logic.Authentication")]
    public void LogicProject_DoesNotReferenceDataDatabaseDirectly(string project)
    {
        Assert.DoesNotContain("Data.Database", Solution.Projects[project]);
    }

    private static bool IsTestProject(string name) =>
        name.EndsWith(".Tests", StringComparison.Ordinal) || name.EndsWith(".IntegrationTests", StringComparison.Ordinal);
}
