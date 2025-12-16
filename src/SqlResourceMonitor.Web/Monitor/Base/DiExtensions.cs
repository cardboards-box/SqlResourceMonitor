namespace SqlResourceMonitor.Web.Monitor.Base;

/// <summary>
/// Dependency injection extensions for the monitor
/// </summary>
public static class DiExtensions
{
	public static IServiceCollection AddDatabaseMonitors(this IServiceCollection services, IConfiguration _config)
	{
		var conStrings = _config.GetSection("ConnectionStrings")
			.AsEnumerable()
			.Where(t => !string.IsNullOrEmpty(t.Value))
			.Select(t => t.Key.Split(':').LastOrDefault())
			.Where(t => !string.IsNullOrEmpty(t))
			.Distinct();
		foreach (var name in conStrings)
			services.AddRefreshable<MonitoredDatabase>(name!);

		return services
			.AddTransient<IDatabaseDataService, DatabaseDataService>();
	}

	public static IServiceCollection AddRefreshable<T>(this IServiceCollection services, string name)
		where T : class, IRefreshable
	{
		return services.AddRefreshable(typeof(T), name);
	}

	public static IServiceCollection AddRefreshable(this IServiceCollection services, Type type, string name)
	{
		var refreshable = typeof(IRefreshable);
		if (!refreshable.IsAssignableFrom(type))
			throw new ArgumentException($"Type {type.FullName} does not implement IRefreshable");

		var constructor = type.GetConstructors().FirstOrDefault();
		if (constructor is null)
		{
			var instance = Activator.CreateInstance(type) 
				?? throw new ArgumentException($"Type {type.FullName} does not have a public constructor");
			services.AddSingleton(refreshable, instance);
			return services;
		}

		services.AddSingleton(refreshable, provider =>
		{
			var parameters = constructor.GetParameters()
				.Select(t =>
				{
					if (t.ParameterType == typeof(string))
						return name;

					return provider.GetRequiredService(t.ParameterType);
				})
				.ToArray();
			return Activator.CreateInstance(type, parameters)
				?? throw new ArgumentException($"Could not create instance of type {type.FullName}");
		});
		return services;
	}
}
