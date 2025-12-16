using Dapper;
using Microsoft.Data.SqlClient;

namespace SqlResourceMonitor.Web.Monitor;

using Base;

/// <summary>
/// Represents a monitored database instance
/// </summary>
public interface IMonitoredDatabase
{
	/// <summary>
	/// The user-readable name of the database
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Whether or not the database's data is refreshing
	/// </summary>
	bool Refreshing { get; }

	/// <summary>
	/// The last time the item was refreshed
	/// </summary>
	DateTimeOffset LastRefresh { get; }

	/// <summary>
	/// The amount of allocated tempDB space
	/// </summary>
	TempDbSpace Space { get; }
}

/// <summary>
/// Represents a monitored database instance
/// </summary>
public class MonitoredDatabase(
	ILogger<MonitoredDatabase> _logger,
	IConfiguration _config,
	string _name) : Refreshable(_logger, _name), IMonitoredDatabase
{
	public TempDbSpace Space { get; private set; } = new();

	/// <summary>
	/// Used for refreshing the databse resources.
	/// </summary>
	/// <param name="token">The cancellation token for the request</param>
	/// <returns>Whether or not the refresh operation was successful</returns>
	/// <remarks>You can put all of your code to refresh things here.</remarks>
	public override async Task<bool> Refresh(CancellationToken token)
	{
		var conString = _config.GetConnectionString(Name);
		if (string.IsNullOrEmpty(conString))
		{
			_logger.LogWarning("No connection string found for database {Name}", Name);
			return false;
		}

		using var con = new SqlConnection(conString);
		await con.OpenAsync(token);

		//Get the tempdb space usage
		Space = await con.QueryFirstOrDefaultAsync<TempDbSpace>(TempDbSpace.QUERY) ?? new();
        

        return true;
	}
}
