using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SqlResourceMonitor.Web.Controllers;

using Models;
using Monitor;

public class HomeController(IDatabaseDataService _dbs) : Controller
{
    public IActionResult Index()
    {
        var names = _dbs.All.OrderBy(t => t.Name).ToArray();
        return View(names);
    }

    public IActionResult Server(string id)
    {
        var database = _dbs.GetByName(id);
        return View(database);
	}

    public IActionResult JsonData(string id)
    {
        var database = _dbs.GetByName(id);
        var tempDbSpace = database?.Space;
        return Json(tempDbSpace);
	}

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
