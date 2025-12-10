using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;
using System;
using System.Collections.Generic;
using midterm_assignment;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string cols, int? limit, int? id)
        {
            ViewData["Title"] = "Home Page";
            ViewData["Cols"] = cols ?? string.Empty;
            ViewData["Limit"] = limit ?? 10;

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var record = DatabaseHelper.GetRecordById(id.Value);
                    ViewData["Record"] = record;
                }
                else if (!string.IsNullOrWhiteSpace(cols))
                {
                    var colsArr = cols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var rows = DatabaseHelper.GetTopRecordsColumns(colsArr, limit ?? 10);
                    ViewData["Rows"] = rows;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query error");
                ViewData["Error"] = ex.Message;
            }

            return View();
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
}
