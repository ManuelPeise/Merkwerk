using System.Xml.Linq;

namespace Architecture.Tests;

/// <summary>Reads Merkwerk.slnx and the referenced .csproj files to get the project reference graph.</summary>
internal sealed class SolutionInfo
{
    private const string SolutionFileName = "Merkwerk.slnx";

    private SolutionInfo(IReadOnlyDictionary<string, IReadOnlyList<string>> projects)
    {
        Projects = projects;
    }

    /// <summary>Project name → names of directly referenced projects.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Projects { get; }

    public static SolutionInfo Load()
    {
        var solutionPath = FindSolutionFile();
        var solutionDirectory = Path.GetDirectoryName(solutionPath)!;

        var projectPaths = XDocument.Load(solutionPath)
            .Descendants("Project")
            .Select(p => (string?)p.Attribute("Path"))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Path.GetFullPath(Path.Combine(solutionDirectory, p!.Replace('\\', '/'))));

        var projects = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var projectPath in projectPaths)
        {
            var references = XDocument.Load(projectPath)
                .Descendants("ProjectReference")
                .Select(r => (string?)r.Attribute("Include"))
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => Path.GetFileNameWithoutExtension(r!.Replace('\\', '/')))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            projects[Path.GetFileNameWithoutExtension(projectPath)] = references;
        }

        return new SolutionInfo(projects);
    }

    /// <summary>All projects reachable from <paramref name="project"/> through project references.</summary>
    public IReadOnlySet<string> TransitiveReferences(string project)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(Projects.GetValueOrDefault(project, []));

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (var next in Projects.GetValueOrDefault(current, []))
            {
                pending.Push(next);
            }
        }

        return visited;
    }

    private static string FindSolutionFile()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(directory.FullName, "sources", SolutionFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"{SolutionFileName} not found above {AppContext.BaseDirectory}.");
    }
}
