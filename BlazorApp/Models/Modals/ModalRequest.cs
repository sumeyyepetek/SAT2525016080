namespace Modals;

internal sealed record ModalRequest(ModalOptions Options, Type? ComponentType = null,
    Dictionary<string, object>? Parameters = null)
{
    internal TaskCompletionSource<ModalResult> Completion { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
