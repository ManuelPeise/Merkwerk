namespace Web.Client.Services.DragDrop;

/// <param name="Group">Zones with the same group exchange cards.</param>
/// <param name="AllowSort">Cards can be reordered inside the zone.</param>
/// <param name="TouchDelayMs">How long a finger must rest on a card before dragging starts (lets the page scroll).</param>
public sealed record DragDropOptions(string Group, bool AllowSort, int TouchDelayMs = 120);
