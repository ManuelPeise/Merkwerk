namespace Web.Client.Services.DragDrop;

/// <summary>A card was dropped: which card, from which zone, into which zone, at which position.</summary>
public sealed record DropEvent(string ItemId, string FromZone, string ToZone, int NewIndex);
