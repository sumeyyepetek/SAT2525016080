namespace Modals;

public sealed record ModalResult(bool Confirmed, object? Value = null);
