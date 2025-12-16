namespace SqlResourceMonitor.Web.Monitor.Base;

/// <summary>
/// The background service that handles refreshing monitored databases
/// </summary>
public class RefreshService(
    IEnumerable<IRefreshable> _refreshables,
    ILogger<RefreshService> _logger,
    IConfiguration _config) : BackgroundService
{
    public double RefreshSeconds => double.TryParse(_config["Refresh:Seconds"], out var seconds) ? seconds : 30.0;
    public int RefreshParallel => int.TryParse(_config["Refresh:Parallel"], out var parallel) ? parallel : 10;

    public async Task RefreshItems(CancellationToken token)
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = RefreshParallel,
            CancellationToken = token
        };

        await Parallel.ForEachAsync(_refreshables, options, async (item, ct) =>
        {
            await item.TriggerRefresh(ct);
		});
    }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_refreshables.Any())
        {
            _logger.LogWarning("No refreshable items found, stopping refresh service");
            return;
		}

        _logger.LogInformation("Starting refresh service with {Seconds} second interval and {Parallel} parallelism in the background", RefreshSeconds, RefreshParallel);

        var interval = TimeSpan.FromSeconds(RefreshSeconds);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Beginning refresh cycle for {count} items", _refreshables.Count());
                await RefreshItems(stoppingToken);
                _logger.LogDebug("Refresh cycle complete, waiting {interval} before next cycle", interval);
                await Task.Delay(interval, stoppingToken);
            }
		}
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Refresh service is stopping due to cancellation request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the refresh service");
            throw;
		}
	}
}
