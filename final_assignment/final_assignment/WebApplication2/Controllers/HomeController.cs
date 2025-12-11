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

        public IActionResult Index(string cols, int? limit, int? id, string ids, int page = 1)
        {
            ViewData["Title"] = "Home Page";
            ViewData["Cols"] = cols ?? string.Empty;
            ViewData["Limit"] = limit ?? 10;

            try
            {
                if (!string.IsNullOrWhiteSpace(ids))
                {
                    var idList = new List<AirQualityRecord>();
                    var parsed = new List<int>();
                    var notParsed = new List<string>();
                    var notFound = new List<int>();

                    var parts = ids.Split(new[] { ',', ';', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (int.TryParse(p.Trim(), out var n) && n > 0)
                        {
                            parsed.Add(n);
                            var rec = DatabaseHelper.GetRecordById(n);
                            if (rec != null) idList.Add(rec);
                            else notFound.Add(n);
                        }
                        else
                        {
                            notParsed.Add(p);
                        }
                    }

                    ViewData["IdRecords"] = idList;
                    ViewData["ParsedIds"] = parsed;
                    ViewData["NotFoundIds"] = notFound;
                    ViewData["InvalidIdParts"] = notParsed;
                }
                else if (id.HasValue && id.Value > 0)
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
                else
                {
                    // no query provided, load paged records to show on homepage (page is 1-based)
                    const int pageSize = 50;
                    var (records, total) = DatabaseHelper.GetRecordsPage(Math.Max(0, page - 1), pageSize);
                    ViewData["AllRecordsPage"] = records;
                    ViewData["AllRecordsTotal"] = total;
                    ViewData["AllRecordsPageIndex"] = page;
                    ViewData["AllRecordsPageSize"] = pageSize;
                    ViewData["AllRecordsTotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
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
