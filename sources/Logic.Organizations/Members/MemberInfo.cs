using Data.Database.Entities.Organizations;

namespace Logic.Organizations.Members;

public sealed record MemberInfo(long MembershipId, long UserId, string DisplayName, string Email, OrganizationRole Role, bool IsOwner);
