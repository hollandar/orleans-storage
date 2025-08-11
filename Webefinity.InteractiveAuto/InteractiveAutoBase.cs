using Microsoft.AspNetCore.Components;

namespace Webefinity.InteractiveAuto;

public abstract class InteractiveAutoBase<TState> : ComponentBase, IDisposable where TState : class, new()
{
    private TState? currentState;
    private PersistingComponentStateSubscription? componentStateRegistration;

    [Inject] public PersistentComponentState ComponentState { get; set; } = default!;
    protected TState State => currentState ?? throw new StateNotInitializedException();
    protected bool HasState => currentState != null;

    protected override async Task OnInitializedAsync()
    {
        ArgumentNullException.ThrowIfNull(this.GetType().FullName);
        this.componentStateRegistration = ComponentState.RegisterOnPersisting(PersistComponentState);
        if (ComponentState.TryTakeFromJson(this.GetType().FullName!, out TState? persistedState) && persistedState is not null)
        {
            this.currentState = persistedState;
        }
        else
        {
            this.currentState = new TState();
            await OnLoadStateAsync(currentState);
            await this.InvokeAsync(StateHasChanged);
        }

        await base.OnInitializedAsync();
    }

    protected Task PersistComponentState()
    {
        ArgumentNullException.ThrowIfNull(this.GetType().FullName);
        ComponentState.PersistAsJson(this.GetType().FullName!, this.currentState);
        return Task.CompletedTask;
    }

    protected abstract Task OnLoadStateAsync(TState currentState);

    protected async Task ReloadState()
    {
        ArgumentNullException.ThrowIfNull(this.currentState);
        await OnLoadStateAsync(this.currentState);
        await this.InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        this.componentStateRegistration?.Dispose();
    }
}
