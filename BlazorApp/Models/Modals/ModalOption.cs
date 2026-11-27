namespace Modals;

public sealed record ModalOptions
{
    public ModalType Type { get; init; }
    public string Title { get; init; } = "";
    public string? Message { get; init; }
    public string? Value { get; init; }
    public int? AutoCloseSeconds { get; init; }
    public string OkText { get; init; } = "Tamam";
    public string CancelText { get; init; } = "İptal";
    public bool ShowFooter { get; init; } = true;
}
