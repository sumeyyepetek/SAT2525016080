using Microsoft.AspNetCore.Components;
using Modals;

namespace Services;

public sealed class ModalService
{
    internal event Func<ModalRequest, Task<ModalResult>>? OnShow;
    internal event Func<ModalResult, Task>? OnClose;

    public Task<ModalResult> Show(string message, ModalType type = ModalType.Alert,
        string title = "", int? autoCloseSeconds = null, string? value = null)
        => Show(new ModalOptions
        {
            Message = message, Type = type, Title = title,
            AutoCloseSeconds = autoCloseSeconds, Value = value
        });

    public Task<ModalResult> Show(ModalOptions options)
        => ShowRequest(new(options));

    public Task<ModalResult> Show<TComponent>(ModalOptions? options = null,
        Dictionary<string, object>? parameters = null) where TComponent : IComponent
        => ShowRequest(new(options ?? new(), typeof(TComponent), parameters));

    public Task Close(bool confirmed = false, object? value = null)
        => OnClose?.Invoke(new(confirmed, value)) ?? Task.CompletedTask;

    private Task<ModalResult> ShowRequest(ModalRequest request)
    {
        if (request.Options.AutoCloseSeconds is <= 0)
            throw new ArgumentOutOfRangeException(nameof(ModalOptions.AutoCloseSeconds));

        return OnShow?.Invoke(request)
            ?? throw new InvalidOperationException("Layout içine <XModal /> eklenmelidir.");
    }
}
