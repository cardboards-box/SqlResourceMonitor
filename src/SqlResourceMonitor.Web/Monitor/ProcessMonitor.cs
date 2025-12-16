namespace SqlResourceMonitor.Web.Monitor;

public class ProcessMonitor
{
	public string Username { get; set; } = string.Empty;

	public string Query { get; set; } = string.Empty;

	public DateTime StartTime { get; set; } = DateTime.MinValue;

	public const string QUERY = @"
SELECT 'test1' as Username, 'SELECT * FROM something' as Query, CURRENT_TIMESTAMP as StartTime UNION ALL
SELECT 'test2' as Username, 'SELECT * FROM something2' as Query, CURRENT_TIMESTAMP as StartTime UNION ALL
SELECT 'test3' as Username, 'SELECT * FROM something3' as Query, CURRENT_TIMESTAMP as StartTime;";
}
