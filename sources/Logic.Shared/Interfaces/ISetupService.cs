using Shared.Models.Organizations;

namespace Logic.Shared.Interfaces;

/// <summary>First-run setup (LP-105): creates the owner account and the first family, then signs the owner in.</summary>
public interface ISetupService
{
    /// <summary>True as long as no organization exists.</summary>
    Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken);

    Task<SetupResult> InitializeAsync(SetupRequest request, CancellationToken cancellationToken);
}
