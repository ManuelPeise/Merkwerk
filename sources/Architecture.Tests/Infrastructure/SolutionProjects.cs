using System.Xml.Linq;

namespace Architecture.Tests.Infrastructure;

/// <summary>Reads the .NET projects listed in sources/Merkwerk.slnx and their project references.</summary>
internal static class SolutionProjects
{
    private static readonly Lazy<IReadOnlyList<SolutionProject>> Projects = new(Load);

    /// <summary>Production projects only: test projects and the web client (esproj) are left out.</summary>
    public static IReadOnlyList<SolutionProject> Production => Projects.Value;

    private static IReadOnlyList<SolutionProject> Load()
    {
        var sourcesDirectory = FindSourcesDirectory();
        var solution = XDocument.Load(Path.Combine(sourcesDirectory, "Merkwerk.slnx"));

        return solution.Descendants("Project")
            .Select(p => p.Attribute("Path")!.Value)
            .Where(path => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.Combine(sourcesDirectory, path.Replace('/', Path.DirectorySeparatorChar)))
            .Select(ReadProject)
            .Where(p => !p.IsTestProject)
            .ToList();
    }

    private static SolutionProject ReadProject(string projectPath)
    {
        var project = XDocument.Load(projectPath);
        var name = Path.GetFileNameWithoutExtension(projectPath);

        var references = project.Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(r.Attribute("Include")!.Value.Replace('\\', '/')))
            .ToList();

        var isTestProject = project.Descendants("IsTestProject").Any(e => e.Value == "true");

        return new SolutionProject(name, references, isTestProject);
    }

    private static string FindSourcesDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Merkwerk.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Merkwerk.slnx not found above " + AppContext.BaseDirectory);
    }
}

internal sealed record SolutionProject(string Name, IReadOnlyList<string> References, bool IsTestProject);
