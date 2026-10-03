using Shared.Enums;

namespace Shared.Models.Organizations;

public sealed record MemberInfo(long MembershipId, long UserId, string DisplayName, string Email, OrganizationRole Role, bool IsOwner);
