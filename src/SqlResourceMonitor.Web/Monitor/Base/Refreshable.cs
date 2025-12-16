namespace SqlResourceMonitor.Web.Monitor.Base;

/// <summary>
/// Represents something that can be refreshed on a schedule
/// </summary>
public interface IRefreshable : IDisposable
{
	/// <summary>
	/// The name of the refreshable instance
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Whether or not the current instance is refreshing
	/// </summary>
	bool Refreshing { get; }

	/// <summary>
	/// The last time the item was refreshed
	/// </summary>
	DateTimeOffset LastRefresh { get; }

	/// <summary>
	/// Used for managing refresh operations
	/// </summary>
	/// <param name="token">The cancellation token for the request</param>
	Task<bool> TriggerRefresh(CancellationToken token);
}

/// <inheritdoc cref="IRefreshable" />
public abstract class Refreshable(ILogger _logger, string _name) : IRefreshable
{
	/// <summary>
	/// This is used to prevent concurrent access to the database instance
	/// </summary>
	private readonly SemaphoreSlim _lock = new(1, 1);

	/// <inheritdoc />
	public string Name { get; } = _name;

	/// <inheritdoc />
	public bool Refreshing { get; private set; }

	/// <inheritdoc />
	public DateTimeOffset LastRefresh { get; private set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// Anything you want refreshed can go here
    /// </summary>
    /// <param name="token">The cancellation token for the request</param>
    /// <returns>Whether or not the refresh operation was successful</returns>
    public abstract Task<bool> Refresh(CancellationToken token);

	/// <summary>
	/// Used for managing refresh operations
	/// </summary>
	/// <param name="token">The cancellation token for the request</param>
	/// <remarks>You probably don't need to modify the code in here</remarks>
	public async Task<bool> TriggerRefresh(CancellationToken token)
	{
		try
		{
			Refreshing = true;
			await _lock.WaitAsync(token);
			_logger.LogDebug("{Name} >> Starting refresh operation.", Name);
			var result = await Refresh(token);
			if (!result)
			{
				_logger.LogWarning("{Name} >> Refresh operation failed.", Name);
				return false;
			}

			_logger.LogDebug("{Name} >> Refresh operation completed successfully.", Name);
			return true;
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("{Name} >> Refresh operation was cancelled.", Name);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{Name} >> An error occurred during the refresh operation.", Name);
			return false;
		}
		finally
		{
			_lock.Release();
			Refreshing = false;
			LastRefresh = DateTimeOffset.UtcNow;
		}
	}

	/// <summary>
	/// This is used to clean up resources
	/// </summary>
	/// <remarks>You probably don't need to modify anything here</remarks>
	public void Dispose()
	{
		_lock.Dispose();
		GC.SuppressFinalize(this);
	}
}
