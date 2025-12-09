using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace midterm_assignment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // 監聽 http://localhost:5000
            builder.WebHost.UseUrls("http://localhost:5000");

            var app = builder.Build();

            // 啟用靜態檔案服務，預設會從 wwwroot 目錄提供檔案（index.html）
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // 提供一個簡單的狀態端點，不會攔截根目錄的靜態檔案
            app.MapGet("/status", () => Results.Text("AirQuality API running"));

            // GET /records?cols=sitename,aqi&limit=10
            app.MapGet("/records", (HttpRequest req) =>
            {
                var colsParam = req.Query["cols"].ToString();
                string[] cols;
                if (string.IsNullOrWhiteSpace(colsParam))
                    cols = new[] { "sitename", "county", "aqi" };
                else
                    cols = colsParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var limitParam = req.Query["limit"].ToString();
                int limit = 10;
                if (!string.IsNullOrWhiteSpace(limitParam) && int.TryParse(limitParam, out var tmp))
                    limit = tmp;

                try
                {
                    var rows = DatabaseHelper.GetTopRecordsColumns(cols, limit);
                    return Results.Json(rows);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message);
                }
            });

            // GET /records/{id}
            app.MapGet("/records/{id:int}", (int id) =>
            {
                var rec = DatabaseHelper.GetRecordById(id);
                return rec == null ? Results.NotFound() : Results.Json(rec);
            });

            // 若路由未匹配，回傳 wwwroot/index.html（方便 SPA 或直接打 /path）
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
