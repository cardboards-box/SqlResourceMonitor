namespace SqlResourceMonitor.Web.Monitor;

public class TempDbSpace
{
	public double Free { get; set; } = 0;

	public double UserObjects { get; set; } = 0;

	public double InternalObjects { get; set; } = 0;

	public double VersionStore { get; set; } = 0;

	public double TotalAllocated { get; set; } = 0;

	public const string QUERY = @"
SELECT 
    SUM(unallocated_extent_page_count) * 1.0 / 128 AS [Free],
    SUM(user_object_reserved_page_count) * 1.0 / 128 AS [UserObjects],
    SUM(internal_object_reserved_page_count) * 1.0 / 128 AS [InternalObjects],
    SUM(version_store_reserved_page_count) * 1.0 / 128 AS [VersionStore],
    (
		SUM(unallocated_extent_page_count) + 
		SUM(user_object_reserved_page_count) + 
		SUM(internal_object_reserved_page_count) + 
		SUM(version_store_reserved_page_count)
	) * 1.0 / 128 AS [TotalAllocated]
FROM 
    tempdb.sys.dm_db_file_space_usage;";
}
