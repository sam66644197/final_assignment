using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Chart()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetStations()
        {
            try
            {
                var records = ReadRecordsFromJson();
                var stations = records
                    .Select(r => r.GetProperty("sitename").GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                return Json(stations);
            }
            catch
            {
                return Json(new List<string>());
            }
        }

        [HttpGet]
        public IActionResult GetPm25Data(string station)
        {
            if (string.IsNullOrEmpty(station)) return Json(new { labels = new string[0], values = new double[0] });
            try
            {
                var records = ReadRecordsFromJson();
                var record = records.FirstOrDefault(r => string.Equals(r.GetProperty("sitename").GetString(), station, StringComparison.OrdinalIgnoreCase));
                if (record.ValueKind == JsonValueKind.Undefined) return Json(new { labels = new string[0], values = new double[0] });

                double pm25 = 0, pm25Avg = 0;
                double.TryParse(record.GetProperty("pm2.5").GetString() ?? "0", out pm25);
                double.TryParse(record.GetProperty("pm2.5_avg").GetString() ?? "0", out pm25Avg);

                var labels = new[] { "pm2.5", "pm2.5_avg" };
                var values = new[] { pm25, pm25Avg };
                return Json(new { labels, values });
            }
            catch
            {
                return Json(new { labels = new string[0], values = new double[0] });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private JsonElement[] ReadRecordsFromJson()
        {
            var fileName = "空氣品質指標.json";
            // 搜尋目前目錄及父目錄中的 APP_Data
            var dir = Directory.GetCurrentDirectory();
            for (int i = 0; i < 6; i++)
            {
                var candidate = Path.Combine(dir, "APP_Data", fileName);
                if (System.IO.File.Exists(candidate))
                {
                    var txt = System.IO.File.ReadAllText(candidate);
                    using var doc = JsonDocument.Parse(txt);
                    if (doc.RootElement.TryGetProperty("records", out var records) && records.ValueKind == JsonValueKind.Array)
                    {
                        return records.EnumerateArray().ToArray();
                    }
                }
                dir = Directory.GetParent(dir)?.FullName ?? dir;
            }
            // 如果找不到檔案，回傳空陣列
            return Array.Empty<JsonElement>();
        }
    }
}
