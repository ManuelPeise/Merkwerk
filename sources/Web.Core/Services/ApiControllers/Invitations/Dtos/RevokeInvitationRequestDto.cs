using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

public sealed record RevokeInvitationRequestDto([Range(1, long.MaxValue)] long Id);
