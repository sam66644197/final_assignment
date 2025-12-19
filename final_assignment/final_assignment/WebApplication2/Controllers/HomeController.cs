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

        // index supports cols/limit, ids (by Id) and name parameter to search by sitename
        public IActionResult Index(string cols, int? limit, string ids, string name, int page = 1)
        {
            ViewData["Title"] = "Home Page";
            ViewData["Cols"] = cols ?? string.Empty;
            ViewData["Limit"] = limit ?? 10;

            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    // search by sitename (partial match)
                    // simple approach: load page of records and filter server-side (or create new DB query)
                    var matches = new List<AirQualityRecord>();
                    int pageSize = 50; int pageIndex = Math.Max(0, page - 1);
                    var (records, total) = DatabaseHelper.GetRecordsPage(pageIndex, pageSize);
                    foreach (var r in records)
                    {
                        if (!string.IsNullOrWhiteSpace(r.sitename) && r.sitename.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                            matches.Add(r);
                    }

                    ViewData["NameSearch"] = name;
                    ViewData["NameResults"] = matches;
                }
                else if (!string.IsNullOrWhiteSpace(ids))
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
                else if (!string.IsNullOrWhiteSpace(cols))
                {
                    var colsArr = cols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var rows = DatabaseHelper.GetTopRecordsColumns(colsArr, limit ?? 10);
                    ViewData["Rows"] = rows;
                }
                else
                {
                    // default: show paged all records
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

        // detail endpoint to return full record by DB id
        public IActionResult Detail(int id)
        {
            var rec = DatabaseHelper.GetRecordById(id);
            if (rec == null) return NotFound();
            return View(rec);
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
