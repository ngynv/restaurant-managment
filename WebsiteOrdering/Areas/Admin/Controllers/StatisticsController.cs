using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.Areas.Repository;
using WebsiteOrdering.Areas.ViewModelAdmin;

namespace WebsiteOrdering.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Statistics")]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsRepository _statisticsRepo;

        public StatisticsController(IStatisticsRepository statisticsRepo)
        {
            _statisticsRepo = statisticsRepo;
        }
        [HttpGet("")]
        public async Task<IActionResult> RevenueChart()
        {
            var totalRevenue = await _statisticsRepo.GetTotalRevenueAsync();
            ViewBag.TotalRevenue = totalRevenue;
            return View();
        }
        [HttpGet("GetCurrentMonthRevenueData")]
        public async Task<IActionResult> GetCurrentMonthRevenueData(string? branchId, string? type)
        {
            var now = DateTime.Now;
            var data = await _statisticsRepo.GetDailyRevenueInMonthAsync(now.Month, now.Year, branchId, type);
            return Json(data);
        }
        //Lọc từ ngày đến ngày
        [HttpPost("GetRevenueByDateRange")]
        public async Task<IActionResult> GetRevenueByDateRange([FromBody] DateRangeRequest request, string? branchId, string? type)
        {
            // Đảm bảo lấy hết ngày đến (giờ 23:59:59)
            var from = request.FromDate.Date;
            var to = request.ToDate.Date.AddDays(1).AddTicks(-1);

            var result = await _statisticsRepo.GetRevenueByDateRangeChartAsync(from, to, branchId, type);
            return Json(result);
        }
        //Lọc theo tháng
        [HttpPost("GetRevenueByMonth")]
        public async Task<IActionResult> GetRevenueByMonth([FromBody] MonthYearRequest request, string? branchId, string? type)
        {
            var result = await _statisticsRepo.GetDailyRevenueInMonthAsync(request.Month, request.Year, branchId, type);
            return Json(result);
        }
        //Lọc theo năm
        [HttpPost("GetRevenueByYear")]
        public async Task<IActionResult> GetRevenueByYear(int year, string? branchId, string? type)
        {
            var result = await _statisticsRepo.GetMonthlyRevenueInYearAsync(year, branchId, type);
            return Json(result);
        }
        //Thống kê món ăn
        [HttpGet("ThongKeMonAn")]
        public async Task<IActionResult> ThongKeMonAn()
        {
            var thongKeList = await _statisticsRepo.GetThongKeMonAnAsync();

            // Top 10 bán chạy
            var top10Sales = thongKeList
                .Where(m => m.SoLuongBan > 0)
                .OrderByDescending(m => m.SoLuongBan)
                .Take(10)
                .ToList();

            // Top 10 ghép nhiều
            var top10Combos = thongKeList
                .Where(m => m.SoLanDuocGhep > 0)
                .OrderByDescending(m => m.SoLanDuocGhep)
                .Take(10)
                .ToList();

            // Top 1 trong top 10 bán chạy
            var top1 = top10Sales.FirstOrDefault();
            var bestSellerName = top1?.TenMon ?? "Không có";
            var bestSellerSold = top1?.SoLuongBan ?? 0;
            var bestSellerCombo = top1?.SoLanDuocGhep ?? 0;

            // Tổng hợp
            ViewBag.TotalDishes = thongKeList.Count;
            ViewBag.BestSeller = bestSellerName;
            ViewBag.TotalSales = thongKeList.Sum(m => m.SoLuongBan);
            ViewBag.TotalCombos = thongKeList.Sum(m => m.SoLanDuocGhep);

            // Dữ liệu biểu đồ (truyền về View dưới dạng JSON string để dùng trực tiếp trong JS)
            ViewBag.SalesLabels = System.Text.Json.JsonSerializer.Serialize(top10Sales.Select(m => m.TenMon));
            ViewBag.SalesData = System.Text.Json.JsonSerializer.Serialize(top10Sales.Select(m => m.SoLuongBan));

            ViewBag.ComboLabels = System.Text.Json.JsonSerializer.Serialize(top10Combos.Select(m => m.TenMon));
            ViewBag.ComboData = System.Text.Json.JsonSerializer.Serialize(top10Combos.Select(m => m.SoLanDuocGhep));

            return View();
        }
    }
}
