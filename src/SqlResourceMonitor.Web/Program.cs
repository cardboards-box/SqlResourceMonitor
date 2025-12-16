using Serilog;
using SqlResourceMonitor.Web.Monitor.Base;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Add the logging provider
builder.Services.AddLogging(c =>
{
    var config = new LoggerConfiguration()
        .WriteTo.File(Path.Combine("logs", "logs.txt"), rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .MinimumLevel.Information()
        .CreateLogger();
    c.AddSerilog(config);
});

//Register all of the database monitors from the configuration
builder.Services.AddDatabaseMonitors(builder.Configuration);

//Register the background service for refreshing
builder.Services.AddHostedService<RefreshService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
