using Logic.Content.Subjects;
using Logic.Content.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Subjects;

namespace Logic.Content.Tests;

/// <summary>LP-109: subjects with a fixed choice of colors and icons; every adult reads, only admins change.</summary>
[Collection(ContentDatabaseCollection.Name)]
public sealed class SubjectServiceTests(ContentDatabaseFixture database)
{
    private readonly ContentTestContext _context = ContentTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task ListAsync_FreshInstance_ContainsTheStandardSubjects()
    {
        var family = await _context.CreateFamilyAsync();

        var subjects = await ListAsync(family, family.MemberUserId);

        Assert.Contains(subjects!, s => s is { Name: "Deutsch", LanguageCode: "de", Color: "subject.german", Icon: "german" });
        Assert.Contains(subjects!, s => s is { Name: "Englisch", LanguageCode: "en", Color: "subject.english", Icon: "english" });
        Assert.Contains(subjects!, s => s is { Name: "Mathe", Color: "subject.math", Icon: "math" });
    }

    [Fact]
    public async Task CreateAndUpdate_Admin_ChangesTheList()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var name = UniqueName("Französisch");

        // Act
        var created = await CreateAsync(family, family.AdminUserId, new SubjectInput($" {name} ", "fr", "subject.purple", "language"));
        var updated = await _context.RunAsync<ISubjectService, SubjectChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.AdminUserId, created.Subject!.Id,
            new SubjectInput(name, "fr", "subject.orange", "globe"), default));

        // Assert
        Assert.Equal(SubjectChangeStatus.Success, created.Status);
        Assert.Equal(name, created.Subject!.Name);
        Assert.Equal(SubjectChangeStatus.Success, updated.Status);
        Assert.Contains(
            (await ListAsync(family, family.MemberUserId))!,
            s => s == new SubjectInfo(created.Subject.Id, name, "fr", "subject.orange", "globe"));
    }

    [Fact]
    public async Task CreateAsync_NameTakenInOtherCase_ReportsName()
    {
        var family = await _context.CreateFamilyAsync();

        var result = await CreateAsync(family, family.AdminUserId, new SubjectInput("mathe", "de", "subject.math", "math"));

        Assert.Equal(SubjectChangeStatus.Invalid, result.Status);
        Assert.Contains("name", result.Errors!.Keys);
    }

    [Fact]
    public async Task UpdateAsync_KeepsOwnName_IsNoConflict()
    {
        var family = await _context.CreateFamilyAsync();
        var name = UniqueName("Musik");
        var created = (await CreateAsync(family, family.AdminUserId, new SubjectInput(name, "de", "subject.magenta", "music"))).Subject!;

        var updated = await _context.RunAsync<ISubjectService, SubjectChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.AdminUserId, created.Id, new SubjectInput(name, "de", "subject.slate", "music"), default));

        Assert.Equal(SubjectChangeStatus.Success, updated.Status);
    }

    [Theory]
    [InlineData("", "de", "subject.math", "math", "name")]
    [InlineData("Sachkunde", "xx", "subject.math", "math", "languageCode")]
    [InlineData("Sachkunde", "de", "#FF0000", "math", "color")]
    [InlineData("Sachkunde", "de", "subject.math", "rocket", "icon")]
    public async Task CreateAsync_InvalidInput_ReportsField(string name, string language, string color, string icon, string field)
    {
        var family = await _context.CreateFamilyAsync();

        var result = await CreateAsync(family, family.AdminUserId, new SubjectInput(name, language, color, icon));

        Assert.Equal(SubjectChangeStatus.Invalid, result.Status);
        Assert.Contains(field, result.Errors!.Keys);
    }

    [Fact]
    public async Task CreateAsync_NameTooLong_ReportsName()
    {
        var family = await _context.CreateFamilyAsync();
        var name = new string('a', SubjectRules.NameMaxLength + 1);

        var result = await CreateAsync(family, family.AdminUserId, new SubjectInput(name, "de", "subject.math", "math"));

        Assert.Contains("name", result.Errors!.Keys);
    }

    [Fact]
    public async Task CreateAndUpdateAsync_ByMember_AreRejected()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var name = UniqueName("Sport");
        var mathe = (await ListAsync(family, family.MemberUserId))!.Single(s => s.Name == "Mathe");

        // Act
        var created = await CreateAsync(family, family.MemberUserId, new SubjectInput(name, "de", "subject.olive", "sport"));
        var updated = await _context.RunAsync<ISubjectService, SubjectChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.MemberUserId, mathe.Id, new SubjectInput("Rechnen", "de", "subject.math", "math"), default));

        // Assert
        Assert.Equal(SubjectChangeStatus.NotFound, created.Status);
        Assert.Equal(SubjectChangeStatus.NotFound, updated.Status);
        Assert.DoesNotContain((await ListAsync(family, family.MemberUserId))!, s => s.Name == name || s.Name == "Rechnen");
    }

    [Fact]
    public async Task ListAsync_AdultOfOtherFamily_ReturnsNull()
    {
        var familyA = await _context.CreateFamilyAsync();
        var familyB = await _context.CreateFamilyAsync();

        var subjects = await ListAsync(familyA, familyB.AdminUserId);

        Assert.Null(subjects);
    }

    [Fact]
    public async Task UpdateAsync_UnknownSubject_NotFound()
    {
        var family = await _context.CreateFamilyAsync();

        var result = await _context.RunAsync<ISubjectService, SubjectChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.AdminUserId, long.MaxValue, new SubjectInput("X", "de", "subject.math", "math"), default));

        Assert.Equal(SubjectChangeStatus.NotFound, result.Status);
    }

    /// <summary>Subjects are instance-wide and the tests share one database – every test needs its own names.</summary>
    private static string UniqueName(string name) => $"{name} {Guid.NewGuid().ToString("N")[..8]}";

    private Task<IReadOnlyList<SubjectInfo>?> ListAsync(Family family, long actingUserId) =>
        _context.RunAsync<ISubjectService, IReadOnlyList<SubjectInfo>?>(s => s.ListAsync(family.OrganizationId, actingUserId, default));

    private Task<SubjectChangeResult> CreateAsync(Family family, long actingUserId, SubjectInput input) =>
        _context.RunAsync<ISubjectService, SubjectChangeResult>(s => s.CreateAsync(family.OrganizationId, actingUserId, input, default));
}
