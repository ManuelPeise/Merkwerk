using Logic.Content.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;
using Shared.Models.Exercises.Questions;
using Shared.Models.Subjects;

namespace Logic.Content.Tests;

/// <summary>LP-110: drafts, publishing as frozen versions, validation per question type, families never mix.</summary>
[Collection(ContentDatabaseCollection.Name)]
public sealed class ExerciseServiceTests(ContentDatabaseFixture database)
{
    private static readonly QuestionContent Choice = new(
        new ChoicePayload { Prompt = "Was heißt dog?", Options = ["Hund", "Katze"] },
        new ChoiceSolution { CorrectIndexes = [0] });

    private static readonly QuestionContent Number = new(
        new TextPayload { Prompt = "7 + 5 =", InputKind = TextInputKind.Number },
        new TextSolution { AcceptedAnswers = ["12"] });

    private static readonly QuestionContent Cloze = new(
        new ClozePayload { Prompt = "Setze ein.", Parts = ["Der ", " ist rot."] },
        new ClozeSolution { Gaps = [["Apfel"]] });

    private readonly ContentTestContext _context = ContentTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task CreateAndGet_AllQuestionTypes_ComeBackFromMySql()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var match = new QuestionContent(
            new MatchPayload { Prompt = "Ordne zu.", Left = ["dog", "cat"], Right = ["Katze", "Hund"] },
            new MatchSolution { RightIndexForLeft = [1, 0] });
        var flashcard = new QuestionContent(
            new FlashcardPayload { Prompt = "Weißt du es?", Front = "house" },
            new FlashcardSolution { Back = "Haus" });

        // Act
        var created = await CreateAsync(family, Input(await MathAsync(family), Choice, Number, Cloze, match, flashcard));
        var detail = await GetAsync(family, created.Exercise!.Id);

        // Assert
        Assert.Equal(ExerciseChangeStatus.Success, created.Status);
        Assert.Equal(ExerciseState.Draft, detail!.Summary.State);
        Assert.Collection(
            detail.Questions,
            q => Assert.IsType<ChoicePayload>(q.Payload),
            q => Assert.Equal(TextInputKind.Number, Assert.IsType<TextPayload>(q.Payload).InputKind),
            q => Assert.Equal(new[] { "Apfel" }, Assert.IsType<ClozeSolution>(q.Solution).Gaps[0]),
            q => Assert.Equal(new[] { 1, 0 }, Assert.IsType<MatchSolution>(q.Solution).RightIndexForLeft),
            q => Assert.Equal("Haus", Assert.IsType<FlashcardSolution>(q.Solution).Back));
    }

    [Fact]
    public async Task Publish_ThenEditDraft_VersionStaysFrozen()
    {
        // Arrange: publish version 1 with one question.
        var family = await _context.CreateFamilyAsync();
        var subjectId = await MathAsync(family);
        var exercise = (await CreateAsync(family, Input(subjectId, Number))).Exercise!;
        var published = await PublishAsync(family, exercise.Id);

        // Act: change the draft.
        var updated = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.MemberUserId, exercise.Id, Input(subjectId, Number, Choice) with { Title = "Neu" }, default));
        var version1 = await GetVersionAsync(family, exercise.Id, 1);

        // Assert
        Assert.Equal(1, published.Exercise!.LatestVersion);
        Assert.Equal(ExerciseState.Published, published.Exercise.State);
        Assert.True(updated.Exercise!.HasUnpublishedChanges);
        Assert.Equal("Rechnen bis 20", version1!.Title);
        Assert.IsType<TextPayload>(Assert.Single(version1.Questions).Payload);
    }

    [Fact]
    public async Task Publish_AfterChanges_CreatesNextVersion_WithoutChanges_KeepsVersion()
    {
        var family = await _context.CreateFamilyAsync();
        var subjectId = await MathAsync(family);
        var exercise = (await CreateAsync(family, Input(subjectId, Number))).Exercise!;
        await PublishAsync(family, exercise.Id);

        var again = await PublishAsync(family, exercise.Id);
        await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.AdminUserId, exercise.Id, Input(subjectId, Number, Cloze), default));
        var second = await PublishAsync(family, exercise.Id);

        Assert.Equal(1, again.Exercise!.LatestVersion);
        Assert.Equal(2, second.Exercise!.LatestVersion);
        Assert.False(second.Exercise.HasUnpublishedChanges);
        Assert.Equal(2, (await GetVersionAsync(family, exercise.Id, 2))!.Questions.Count);
        Assert.Single((await GetVersionAsync(family, exercise.Id, 1))!.Questions);
    }

    [Fact]
    public async Task Publish_WithoutQuestions_IsRejected()
    {
        var family = await _context.CreateFamilyAsync();
        var exercise = (await CreateAsync(family, Input(await MathAsync(family)))).Exercise!;

        var result = await PublishAsync(family, exercise.Id);

        Assert.Equal(ExerciseChangeStatus.Invalid, result.Status);
        Assert.Contains("questions", result.Errors!.Keys);
    }

    private static readonly QuestionContent[] InvalidQuestions =
    [
        // Payload and solution of different types.
        new QuestionContent(new ChoicePayload { Prompt = "?", Options = ["a", "b"] }, new TextSolution { AcceptedAnswers = ["a"] }),
        // Only one option.
        new QuestionContent(new ChoicePayload { Prompt = "?", Options = ["a"] }, new ChoiceSolution { CorrectIndexes = [0] }),
        // Two correct answers without MultipleAnswers.
        new QuestionContent(new ChoicePayload { Prompt = "?", Options = ["a", "b"] }, new ChoiceSolution { CorrectIndexes = [0, 1] }),
        // Correct answer out of range.
        new QuestionContent(new ChoicePayload { Prompt = "?", Options = ["a", "b"] }, new ChoiceSolution { CorrectIndexes = [2] }),
        // Number question with a word as answer.
        new QuestionContent(new TextPayload { Prompt = "?", InputKind = TextInputKind.Number }, new TextSolution { AcceptedAnswers = ["zwölf"] }),
        // Two gaps, one answer list.
        new QuestionContent(new ClozePayload { Prompt = "?", Parts = ["a", "b", "c"] }, new ClozeSolution { Gaps = [["x"]] }),
        // Match without a permutation.
        new QuestionContent(
            new MatchPayload { Prompt = "?", Left = ["a", "b"], Right = ["c", "d"] },
            new MatchSolution { RightIndexForLeft = [0, 0] }),
        // Empty prompt.
        new QuestionContent(new FlashcardPayload { Prompt = " ", Front = "a" }, new FlashcardSolution { Back = "b" }),
    ];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task Create_InvalidQuestion_ReportsItsNumber(int index)
    {
        var invalid = InvalidQuestions[index];
        var family = await _context.CreateFamilyAsync();

        var result = await CreateAsync(family, Input(await MathAsync(family), Number, invalid));

        Assert.Equal(ExerciseChangeStatus.Invalid, result.Status);
        Assert.Equal(new[] { "questions[1]" }, result.Errors!.Keys);
    }

    [Fact]
    public async Task Create_UnknownSubjectAndBadTitle_ReportsFields()
    {
        var family = await _context.CreateFamilyAsync();

        var result = await CreateAsync(family, Input(long.MaxValue, Number) with { Title = " ", Grade = 5 });

        Assert.Equal(ExerciseChangeStatus.Invalid, result.Status);
        Assert.Contains("title", result.Errors!.Keys);
        Assert.Contains("grade", result.Errors.Keys);
        Assert.Contains("subjectId", result.Errors.Keys);
    }

    [Fact]
    public async Task Generator_CreatePublishGet_KeepsSettingsWithoutQuestions()
    {
        // Arrange (LP-131)
        var family = await _context.CreateFamilyAsync();
        var settings = new ArithmeticSettings
        {
            Operations = [ArithmeticOperation.Add, ArithmeticOperation.Subtract],
            NumberRange = 100,
            TenTransition = TenTransition.With,
            Placeholder = PlaceholderMode.Mixed,
            TaskCount = 15,
        };
        var input = Input(await MathAsync(family)) with { ContentSource = ExerciseContentSource.Generator, Generator = settings };

        // Act
        var created = await CreateAsync(family, input);
        var published = await PublishAsync(family, created.Exercise!.Id);
        var detail = await GetAsync(family, created.Exercise.Id);
        var version = await GetVersionAsync(family, created.Exercise.Id, 1);

        // Assert
        Assert.Equal(ExerciseChangeStatus.Success, created.Status);
        Assert.Equal(15, created.Exercise.QuestionCount);
        Assert.Equal(1, published.Exercise!.LatestVersion);
        Assert.Empty(detail!.Questions);
        Assert.Equal(settings, Assert.IsType<ArithmeticSettings>(detail.Generator) with { Operations = settings.Operations });
        Assert.Equal(settings.Operations, Assert.IsType<ArithmeticSettings>(version!.Generator).Operations);
        Assert.Equal(ExerciseContentSource.Generator, version.ContentSource);
    }

    [Fact]
    public async Task Generator_SwitchBackToQuestions_DropsSettings()
    {
        var family = await _context.CreateFamilyAsync();
        var subjectId = await MathAsync(family);
        var exercise = (await CreateAsync(family, Input(subjectId) with
        {
            ContentSource = ExerciseContentSource.Generator,
            Generator = new ArithmeticSettings(),
        })).Exercise!;

        await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.MemberUserId, exercise.Id, Input(subjectId, Number), default));
        var detail = await GetAsync(family, exercise.Id);

        Assert.Null(detail!.Generator);
        Assert.Single(detail.Questions);
    }

    [Fact]
    public async Task Generator_WithoutSettingsOrWithQuestions_IsRejected()
    {
        var family = await _context.CreateFamilyAsync();
        var subjectId = await MathAsync(family);

        var withoutSettings = await CreateAsync(family, Input(subjectId) with { ContentSource = ExerciseContentSource.Generator });
        var withQuestions = await CreateAsync(family, Input(subjectId, Number) with
        {
            ContentSource = ExerciseContentSource.Generator,
            Generator = new ArithmeticSettings(),
        });
        var invalidSettings = await CreateAsync(family, Input(subjectId) with
        {
            ContentSource = ExerciseContentSource.Generator,
            Generator = new ArithmeticSettings { NumberRange = 2 },
        });
        var settingsOnQuestions = await CreateAsync(family, Input(subjectId, Number) with { Generator = new ArithmeticSettings() });

        Assert.Contains("generator", withoutSettings.Errors!.Keys);
        Assert.Contains("questions", withQuestions.Errors!.Keys);
        Assert.Contains("generator.numberRange", invalidSettings.Errors!.Keys);
        Assert.Contains("generator", settingsOnQuestions.Errors!.Keys);
    }

    [Fact]
    public async Task Create_WordListSource_NotSupportedYet()
    {
        var family = await _context.CreateFamilyAsync();

        var result = await CreateAsync(family, Input(await MathAsync(family)) with { ContentSource = ExerciseContentSource.WordList });

        Assert.Contains("contentSource", result.Errors!.Keys);
    }

    [Fact]
    public async Task OtherFamily_CannotSeeChangeOrPublish()
    {
        // Arrange: an exercise of family A; an adult of family B guesses its id.
        var familyA = await _context.CreateFamilyAsync();
        var subjectId = await MathAsync(familyA);
        var exercise = (await CreateAsync(familyA, Input(subjectId, Number))).Exercise!;
        await PublishAsync(familyA, exercise.Id);
        var familyB = await _context.CreateFamilyAsync();

        // Act
        var detail = await GetAsync(familyB, exercise.Id);
        var version = await GetVersionAsync(familyB, exercise.Id, 1);
        var updated = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.UpdateAsync(
            familyB.OrganizationId, familyB.AdminUserId, exercise.Id, Input(subjectId, Choice), default));
        var published = await PublishAsync(familyB, exercise.Id);
        var listOfA = await _context.RunAsync<IExerciseService, IReadOnlyList<ExerciseSummary>?>(s =>
            s.ListAsync(familyA.OrganizationId, familyB.AdminUserId, default));

        // Assert
        Assert.Null(detail);
        Assert.Null(version);
        Assert.Equal(ExerciseChangeStatus.NotFound, updated.Status);
        Assert.Equal(ExerciseChangeStatus.NotFound, published.Status);
        Assert.Null(listOfA);
    }

    [Fact]
    public async Task Archive_ThenRestore_ChangesStateKeepsVersion()
    {
        var family = await _context.CreateFamilyAsync();
        var exercise = (await CreateAsync(family, Input(await MathAsync(family), Number))).Exercise!;
        await PublishAsync(family, exercise.Id);

        var archived = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.SetArchivedAsync(family.OrganizationId, family.MemberUserId, exercise.Id, true, default));
        var restored = await _context.RunAsync<IExerciseService, ExerciseChangeResult>(s =>
            s.SetArchivedAsync(family.OrganizationId, family.MemberUserId, exercise.Id, false, default));

        Assert.Equal(ExerciseState.Archived, archived.Exercise!.State);
        Assert.Equal(ExerciseState.Published, restored.Exercise!.State);
        Assert.NotNull(await GetVersionAsync(family, exercise.Id, 1));
    }

    private static ExerciseInput Input(long subjectId, params QuestionContent[] questions) =>
        new("Rechnen bis 20", subjectId, 2, ExerciseContentSource.Questions, questions);

    private async Task<long> MathAsync(Family family) =>
        (await _context.RunAsync<ISubjectService, IReadOnlyList<SubjectInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.AdminUserId, default)))!.Single(s => s.Name == "Mathe").Id;

    private Task<ExerciseChangeResult> CreateAsync(Family family, ExerciseInput input) =>
        _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.CreateAsync(family.OrganizationId, family.MemberUserId, input, default));

    private Task<ExerciseDetail?> GetAsync(Family family, long exerciseId) =>
        _context.RunAsync<IExerciseService, ExerciseDetail?>(s => s.GetAsync(family.OrganizationId, family.MemberUserId, exerciseId, default));

    private Task<ExerciseChangeResult> PublishAsync(Family family, long exerciseId) =>
        _context.RunAsync<IExerciseService, ExerciseChangeResult>(s => s.PublishAsync(family.OrganizationId, family.MemberUserId, exerciseId, default));

    private Task<ExerciseSnapshot?> GetVersionAsync(Family family, long exerciseId, int number) =>
        _context.RunAsync<IExerciseService, ExerciseSnapshot?>(s =>
            s.GetVersionAsync(family.OrganizationId, family.MemberUserId, exerciseId, number, default));
}
