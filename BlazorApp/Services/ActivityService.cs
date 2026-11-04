using ApiContracts;

namespace BlazorApp.Services;

// Keeps the unread counts for the bell and the messages tab fresh by asking the API every few seconds.
public class ActivityService : IDisposable
{
    private readonly INotificationService notificationService;
    private CancellationTokenSource? cancellation;
    private int? userId;

    public UnreadCountsDto Counts { get; private set; } = new();

    public event Action? Changed;

    public ActivityService(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    public void Start(int forUserId)
    {
        if (userId == forUserId && cancellation is not null)
        {
            return;
        }

        Stop();
        userId = forUserId;
        cancellation = new CancellationTokenSource();
        _ = PollAsync(cancellation.Token);
    }

    public void Stop()
    {
        cancellation?.Cancel();
        cancellation = null;
        userId = null;
        Counts = new UnreadCountsDto();
        Changed?.Invoke();
    }

    public async Task RefreshAsync()
    {
        if (userId is null)
        {
            return;
        }

        try
        {
            Counts = await notificationService.GetUnreadCountsAsync(userId.Value);
            Changed?.Invoke();
        }
        catch (Exception)
        {
            // the API is down for a moment, try again on the next tick
        }
    }

    private async Task PollAsync(CancellationToken token)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(6));
        await RefreshAsync();
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                await RefreshAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // stopped
        }
    }

    public void Dispose()
    {
        cancellation?.Cancel();
    }
}
