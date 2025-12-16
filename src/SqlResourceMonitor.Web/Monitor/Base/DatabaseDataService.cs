namespace SqlResourceMonitor.Web.Monitor;

using Base;

/// <summary>
/// A service for accessing monitored database data
/// </summary>
public interface IDatabaseDataService
{
	/// <summary>
	/// All of the databases that are being monitored
	/// </summary>
	IEnumerable<IMonitoredDatabase> All { get; }

	/// <summary>
	/// Gets a specific monitored database by name
	/// </summary>
	/// <param name="name">The name of the database</param>
	/// <returns>The monitored database</returns>
	IMonitoredDatabase? GetByName(string name);
}

internal class DatabaseDataService(
	IEnumerable<IRefreshable> _refreshables) : IDatabaseDataService
{
	public IEnumerable<IMonitoredDatabase> All => _refreshables.OfType<IMonitoredDatabase>();

	public IMonitoredDatabase? GetByName(string name)
	{
		return All.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}
}
