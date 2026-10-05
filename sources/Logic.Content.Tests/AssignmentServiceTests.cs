using Logic.Content.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Assignments;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;
using Shared.Models.Exercises.Questions;
using Shared.Models.Organizations;
using Shared.Models.Subjects;

namespace Logic.Content.Tests;

/// <summary>LP-114: assigning to children and groups, options, revoking, the child's view and family isolation.</summary>
[Collection(ContentDatabaseCollection.Name)]
public sealed class AssignmentServiceTests(ContentDatabaseFixture database)
{
    private static readonly QuestionContent Number = new(
        new TextPayload { Prompt = "7 + 5 =", InputKind = TextInputKind.Number },
        new TextSolution { AcceptedAnswers = ["12"] });

    private static readonly DateOnly Today = DateOnly.FromDateTime(TimeProvider.System.GetUtcNow().UtcDateTime);

    private readonly ContentTestContext _context = ContentTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task Assign_ChildrenAndGroup_ListsOneAssignmentPerTarget()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        var ben = await LearnerAsync(family, "Ben");
        var group = await GroupAsync(family, "Lesegruppe", ben);

        // Act
        var result = await AssignAsync(family, Targets(exerciseId, [mia, ben], [group]) with
        {
            DueDate = Today.AddDays(7),
            AllowDotArray = true,
        });
        var list = await ListAsync(family, exerciseId);

        // Assert
        Assert.Equal(AssignmentChangeStatus.Success, result.Status);
        Assert.Equal(3, result.Assignments!.Count);
        Assert.Collection(
            list!,
            a => Assert.Equal(mia, a.LearnerId),
            a => Assert.Equal(ben, a.LearnerId),
            a => Assert.Equal(group, a.GroupId));
        Assert.All(list!, a =>
        {
            Assert.Equal(Today.AddDays(7), a.DueDate);
            Assert.True(a.AllowDotArray);
            Assert.False(a.AllowTimesTableMatrix);
            Assert.Null(a.Generator);
        });
    }

    [Fact]
    public async Task Assign_Again_ChangesOptionsInsteadOfDuplicating()
    {
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        var first = (await AssignAsync(family, Targets(exerciseId, [mia], []))).Assignments!.Single();

        var second = (await AssignAsync(family, Targets(exerciseId, [mia], []) with
        {
            DueDate = Today.AddDays(3),
            AllowTimesTableMatrix = true,
        })).Assignments!.Single();

        Assert.Equal(first.Id, second.Id);
        var assignment = Assert.Single((await ListAsync(family, exerciseId))!);
        Assert.Equal(Today.AddDays(3), assignment.DueDate);
        Assert.True(assignment.AllowTimesTableMatrix);
    }

    [Fact]
    public async Task Assign_DraftOrArchivedExercise_IsRejected()
    {
        // Arrange: a draft that was never published, and a published exercise that is archived.
        var family = await _context.CreateFamilyAsync();
        var mia = await LearnerAsync(family, "Mia");
        var draft = await CreateExerciseAsync(family, Input(await MathAsync(family), Number));
        var archived = await PublishedExerciseAsync(family);
        await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.SetArchivedAsync(family.OrganizationId, family.MemberUserId, archived, true, default));

        // Act
        var draftResult = await AssignAsync(family, Targets(draft, [mia], []));
        var archivedResult = await AssignAsync(family, Targets(archived, [mia], []));

        // Assert
        Assert.Contains("exerciseId", draftResult.Errors!.Keys);
        Assert.Contains("exerciseId", archivedResult.Errors!.Keys);
    }

    [Fact]
    public async Task Assign_InvalidTargetsAndDueDate_ReportsFields()
    {
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var otherFamily = await _context.CreateFamilyAsync();
        var foreignChild = await LearnerAsync(otherFamily, "Fremd");
        _context.CurrentUser.OrganizationId = family.OrganizationId;

        var noTargets = await AssignAsync(family, Targets(exerciseId, [], []));
        var foreign = await AssignAsync(family, Targets(exerciseId, [foreignChild], [long.MaxValue]) with
        {
            DueDate = Today.AddDays(-10),
        });

        Assert.Equal(AssignmentChangeStatus.Invalid, noTargets.Status);
        Assert.Contains("learnerIds", noTargets.Errors!.Keys);
        Assert.Contains("learnerIds", foreign.Errors!.Keys);
        Assert.Contains("groupIds", foreign.Errors.Keys);
        Assert.Contains("dueDate", foreign.Errors.Keys);
    }

    [Fact]
    public async Task Assign_GeneratorSettings_OnlyValidOnesForGeneratorExercises()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var mia = await LearnerAsync(family, "Mia");
        var subjectId = await MathAsync(family);
        var generatorExercise = await CreateExerciseAsync(family, Input(subjectId) with
        {
            ContentSource = ExerciseContentSource.Generator,
            Generator = new ArithmeticSettings { NumberRange = 20 },
        });
        await PublishAsync(family, generatorExercise);
        var questionExercise = await PublishedExerciseAsync(family);
        var harder = new ArithmeticSettings { NumberRange = 100, TaskCount = 20 };

        // Act
        var valid = await AssignAsync(family, Targets(generatorExercise, [mia], []) with { Generator = harder });
        var invalid = await AssignAsync(family, Targets(generatorExercise, [mia], []) with
        {
            Generator = new ArithmeticSettings { NumberRange = 2 },
        });
        var onQuestions = await AssignAsync(family, Targets(questionExercise, [mia], []) with { Generator = harder });
        var forChild = await ListForLearnerAsync(family, mia);

        // Assert
        Assert.Equal(AssignmentChangeStatus.Success, valid.Status);
        Assert.Contains("generator.numberRange", invalid.Errors!.Keys);
        Assert.Contains("generator", onQuestions.Errors!.Keys);
        var settings = Assert.IsType<ArithmeticSettings>(Assert.Single(forChild).Generator);
        Assert.Equal(100, settings.NumberRange);
        Assert.Equal(20, settings.TaskCount);
    }

    [Fact]
    public async Task ListForLearner_GroupAssignment_FollowsTheGroupsMembers()
    {
        // Arrange: the group is assigned while Ben is not in it yet.
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        var ben = await LearnerAsync(family, "Ben");
        var group = await GroupAsync(family, "Mathe-Gruppe", mia);
        await AssignAsync(family, Targets(exerciseId, [], [group]));

        // Act: Ben joins, Mia leaves.
        await _context.RunAsync<IGroupService, GroupChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.AdminUserId, group, new GroupInput("Mathe-Gruppe", [ben]), default));
        var forBen = await ListForLearnerAsync(family, ben);
        var forMia = await ListForLearnerAsync(family, mia);

        // Assert
        Assert.Equal(exerciseId, Assert.Single(forBen).ExerciseId);
        Assert.Empty(forMia);
    }

    [Fact]
    public async Task ListForLearner_DirectAndGroup_OneEntryDirectWins_DueFirst()
    {
        // Arrange: exercise 1 directly (no due date) and through the group (due in 2 days); exercise 2 due tomorrow.
        var family = await _context.CreateFamilyAsync();
        var first = await PublishedExerciseAsync(family);
        var second = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        var group = await GroupAsync(family, "Alle", mia);
        var direct = (await AssignAsync(family, Targets(first, [mia], []))).Assignments!.Single();
        await AssignAsync(family, Targets(first, [], [group]) with { DueDate = Today.AddDays(2) });
        await AssignAsync(family, Targets(second, [mia], []) with { DueDate = Today.AddDays(1) });

        // Act
        var list = await ListForLearnerAsync(family, mia);

        // Assert
        Assert.Collection(
            list,
            a => Assert.Equal(second, a.ExerciseId),
            a =>
            {
                Assert.Equal(direct.Id, a.AssignmentId);
                Assert.Null(a.DueDate);
            });
    }

    [Fact]
    public async Task ListForLearner_ArchivedExercise_IsLeftOut()
    {
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        await AssignAsync(family, Targets(exerciseId, [mia], []));

        await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.SetArchivedAsync(family.OrganizationId, family.MemberUserId, exerciseId, true, default));

        Assert.Empty(await ListForLearnerAsync(family, mia));
        Assert.Single((await ListAsync(family, exerciseId))!);
    }

    [Fact]
    public async Task Revoke_RemovesAssignment_DeletedChildTakesItsAssignmentsAlong()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(family);
        var mia = await LearnerAsync(family, "Mia");
        var ben = await LearnerAsync(family, "Ben");
        var assigned = (await AssignAsync(family, Targets(exerciseId, [mia, ben], []))).Assignments!;

        // Act
        var revoked = await RevokeAsync(family, assigned.Single(a => a.LearnerId == mia).Id);
        await _context.RunAsync<ILearnerService, LearnerChangeStatus>(s =>
            s.DeleteAsync(family.OrganizationId, family.AdminUserId, ben, default));

        // Assert
        Assert.Equal(AssignmentChangeStatus.Success, revoked);
        Assert.Empty((await ListAsync(family, exerciseId))!);
    }

    [Fact]
    public async Task OtherFamily_CannotListAssignOrRevoke()
    {
        // Arrange: an assignment of family A; an adult of family B guesses the ids.
        var familyA = await _context.CreateFamilyAsync();
        var exerciseId = await PublishedExerciseAsync(familyA);
        var mia = await LearnerAsync(familyA, "Mia");
        var assignment = (await AssignAsync(familyA, Targets(exerciseId, [mia], []))).Assignments!.Single();
        var familyB = await _context.CreateFamilyAsync();

        // Act
        var list = await ListAsync(familyB, exerciseId);
        var assign = await AssignAsync(familyB, Targets(exerciseId, [mia], []));
        var revoke = await RevokeAsync(familyB, assignment.Id);
        var childView = await ListForLearnerAsync(familyB, mia);
        var listOfAByB = await _context.RunAsync<IAssignmentService, IReadOnlyList<AssignmentInfo>?>(s =>
            s.ListForExerciseAsync(familyA.OrganizationId, familyB.AdminUserId, exerciseId, default));

        // Assert
        Assert.Null(list);
        Assert.Equal(AssignmentChangeStatus.NotFound, assign.Status);
        Assert.Equal(AssignmentChangeStatus.NotFound, revoke);
        Assert.Empty(childView);
        Assert.Null(listOfAByB);
    }

    private static AssignmentInput Targets(long exerciseId, long[] learnerIds, long[] groupIds) =>
        new(exerciseId, learnerIds, groupIds, DueDate: null, AllowDotArray: false, AllowTimesTableMatrix: false);

    private static ExerciseInput Input(long subjectId, params QuestionContent[] questions) =>
        new("Rechnen bis 20", subjectId, 2, ExerciseContentSource.Questions, questions);

    private async Task<long> MathAsync(Family family) =>
        (await _context.RunAsync<ISubjectService, IReadOnlyList<SubjectInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.AdminUserId, default)))!.Single(s => s.Name == "Mathe").Id;

    private async Task<long> CreateExerciseAsync(Family family, ExerciseInput input)
    {
        var result = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.CreateAsync(family.OrganizationId, family.MemberUserId, input, default));

        Assert.Equal(ExerciseChangeStatus.Success, result.Status);
        return result.Exercise!.Id;
    }

    private async Task PublishAsync(Family family, long exerciseId)
    {
        var result = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.PublishAsync(family.OrganizationId, family.MemberUserId, exerciseId, default));

        Assert.Equal(ExerciseChangeStatus.Success, result.Status);
    }

    private async Task<long> PublishedExerciseAsync(Family family)
    {
        var exerciseId = await CreateExerciseAsync(family, Input(await MathAsync(family), Number));
        await PublishAsync(family, exerciseId);
        return exerciseId;
    }

    private async Task<long> LearnerAsync(Family family, string name)
    {
        var result = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s =>
            s.CreateAsync(family.OrganizationId, family.AdminUserId, new LearnerInput(name, 2, "fox"), default));

        Assert.Equal(LearnerChangeStatus.Success, result.Status);
        return result.Learner!.Id;
    }

    private async Task<long> GroupAsync(Family family, string name, params long[] learnerIds)
    {
        var result = await _context.RunAsync<IGroupService, GroupChangeResult>(s =>
            s.CreateAsync(family.OrganizationId, family.AdminUserId, new GroupInput(name, learnerIds), default));

        Assert.Equal(GroupChangeStatus.Success, result.Status);
        return result.Group!.Id;
    }

    private Task<AssignmentChangeResult> AssignAsync(Family family, AssignmentInput input) =>
        _context.RunAsync<IAssignmentService, AssignmentChangeResult>(s =>
            s.AssignAsync(family.OrganizationId, family.MemberUserId, input, default));

    private Task<IReadOnlyList<AssignmentInfo>?> ListAsync(Family family, long exerciseId) =>
        _context.RunAsync<IAssignmentService, IReadOnlyList<AssignmentInfo>?>(s =>
            s.ListForExerciseAsync(family.OrganizationId, family.MemberUserId, exerciseId, default));

    private Task<AssignmentChangeStatus> RevokeAsync(Family family, long assignmentId) =>
        _context.RunAsync<IAssignmentService, AssignmentChangeStatus>(s =>
            s.RevokeAsync(family.OrganizationId, family.MemberUserId, assignmentId, default));

    private Task<IReadOnlyList<LearnerAssignment>> ListForLearnerAsync(Family family, long learnerId) =>
        _context.RunAsync<IAssignmentService, IReadOnlyList<LearnerAssignment>>(s =>
            s.ListForLearnerAsync(family.OrganizationId, learnerId, default));
}
