using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebsiteOrdering.Models;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Controllers
{
    public class LocationController : Controller
    {
        private readonly IGeoService _geoService;
        private readonly ILocationService _locationService;
        private readonly IOptions<OpenRouteServiceSettings> _orsOptions;
        private readonly ILogger<LocationController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public LocationController(ILocationService locationService,
            IOptions<OpenRouteServiceSettings> orsOptions, IGeoService geoService,
            ILogger<LocationController> logger, IHttpClientFactory httpClientFactory)
        {
            _locationService = locationService;
            _orsOptions = orsOptions;
            _geoService = geoService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        // Hiển thị danh sách trên bản đồ
        public async Task<IActionResult> Index()
        {
            ViewBag.OpenRouteApiKey = _orsOptions.Value.ApiKey;
            var locations = await _locationService.GetAllLocationsAsync();
            return View(locations);
        }
        public async Task<IActionResult> Details(string id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }
        // Form nhập vị trí
        public IActionResult Create()
        {
            return View();
        }

        // Lưu vị trí mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chinhanh createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }

            try
            {
                await _locationService.CreateLocationAsync(createDto);
                TempData["SuccessMessage"] = "Vị trí đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(createDto);
            }
        }
        public async Task<IActionResult> Edit(string id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            var updateDto = new Chinhanh
            {
                Tencnhanh = location.Tencnhanh,
                Diachicn = location.Diachicn,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            return View(updateDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Chinhanh updateDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateDto);
            }

            try
            {
                await _locationService.UpdateLocationAsync(id, updateDto);
                TempData["SuccessMessage"] = "Vị trí đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(updateDto);
            }
        }
        public async Task<IActionResult> Delete(string id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var success = await _locationService.DeleteLocationAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Vị trí đã được xóa thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy vị trí để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> SaveLocation([FromBody] Chinhanh createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                var location = await _locationService.CreateLocationAsync(createDto);
                return Ok(new
                {
                    success = true,
                    message = "Vị trí đã được lưu thành công!",
                    id = location.Idchinhanh,
                    location = location
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi lưu vị trí: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Index));
            }

            var locations = await _locationService.SearchLocationsByNameAsync(name);
            ViewBag.SearchTerm = name;
            return View("Index", locations);
        }
        [HttpGet]
        public async Task<IActionResult> GetByArea(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
        {
            try
            {
                var locations = await _locationService.GetLocationsByAreaAsync(minLat, maxLat, minLng, maxLng);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveUserSessionLocation([FromBody] LatLngViewModel userLoc)
        {
            HttpContext.Session.SetString("UserLat", userLoc.Latitude.ToString());
            HttpContext.Session.SetString("UserLng", userLoc.Longitude.ToString());
            HttpContext.Session.SetString("UserAddress", userLoc.Address ?? "");
            // Tìm chi nhánh gần nhất và lưu vào session
            var nearest = await _locationService.FindNearestBranchAsync((double)userLoc.Latitude, (double)userLoc.Longitude);
            if (nearest != null)
            {
                const double minutesPerKm = 1.5; // 40 km/h = 1.5 phút/km
                double distance = _locationService.GetDistance(userLoc.Latitude, userLoc.Longitude, (double)nearest.Latitude, (double)nearest.Longitude);
                int estimatedMinutes = (int)Math.Ceiling(distance * minutesPerKm);
                var sessionData = new UserLocationSessionViewModel
                {
                    NearestBranchId = nearest.Idchinhanh,
                    DistanceKm = Math.Round(distance, 2),
                    EstimatedMinutes = estimatedMinutes,
                    DeliveryMethod = userLoc.DeliveryMethod
                };
                //HttpContext.Session.SetString("NearestBranchId", nearest.Idchinhanh);
                HttpContext.Session.SetString("UserLocationInfo", JsonSerializer.Serialize(sessionData));
            }
            return Ok(new { success = true, message = "Lưu địa chỉ vào session thành công", });
        }
        [HttpGet]
        public IActionResult GetSessionLocation()
        {
            if (HttpContext.Session.TryGetValue("UserLat", out var latBytes) &&
                HttpContext.Session.TryGetValue("UserLng", out var lngBytes) &&
                HttpContext.Session.TryGetValue("UserAddress", out var addressBytes))
            {
                return Ok(new
                {
                    lat = double.Parse(System.Text.Encoding.UTF8.GetString(latBytes)),
                    lng = double.Parse(System.Text.Encoding.UTF8.GetString(lngBytes)),
                    address = System.Text.Encoding.UTF8.GetString(addressBytes)
                });
            }

            return NotFound();
        }
        [HttpPost("SetSessionLocation")]
        public async Task<IActionResult> SetSessionLocation([FromBody] LatLngViewModel model)
        {
            var address = await _geoService.ReverseGeocodeAsync(model.Latitude, model.Longitude);

            HttpContext.Session.SetString("UserLat", model.Latitude.ToString());
            HttpContext.Session.SetString("UserLng", model.Longitude.ToString());
            HttpContext.Session.SetString("UserAddress", address);
            // Tìm chi nhánh gần nhất và lưu vào session
            // Tìm chi nhánh gần nhất và lưu vào session
            var nearest = await _locationService.FindNearestBranchAsync(model.Latitude, model.Longitude);
            if (nearest != null)
            {
                const double minutesPerKm = 1.5; // 40 km/h = 1.5 phút/km
                double distance = _locationService.GetDistance(model.Latitude, model.Longitude, (double)nearest.Latitude, (double)nearest.Longitude);
                int estimatedMinutes = (int)Math.Ceiling(distance * minutesPerKm);
                var sessionData = new UserLocationSessionViewModel
                {
                    NearestBranchId = nearest.Idchinhanh,
                    DistanceKm = Math.Round(distance, 2),
                    EstimatedMinutes = estimatedMinutes
                };
                //HttpContext.Session.SetString("NearestBranchId", nearest.Idchinhanh);
                HttpContext.Session.SetString("UserLocationInfo", JsonSerializer.Serialize(sessionData));
            }
            return Ok(new
            {
                success = true,
                lat = model.Latitude,
                lng = model.Longitude,
                address
            });
        }
        // Enhanced Location Controller with routing support
        [HttpGet]
        public async Task<IActionResult> GetNearestStore()
        {
            var allLocations = await _locationService.GetAllLocationsAsync();
            if (!HttpContext.Session.TryGetValue("UserLat", out var latBytes) ||
                !HttpContext.Session.TryGetValue("UserLng", out var lngBytes))
            {
                return BadRequest(new { success = false, message = "Vị trí người dùng chưa được lưu." });
            }

            double userLat = double.Parse(System.Text.Encoding.UTF8.GetString(latBytes));
            double userLng = double.Parse(System.Text.Encoding.UTF8.GetString(lngBytes));

            Chinhanh? nearest = null;
            double minDist = double.MaxValue;

            foreach (var store in allLocations)
            {
                var dist = _locationService.GetDistance(userLat, userLng, (double)store.Latitude, (double)store.Longitude);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = store;
                }
            }

            if (nearest == null)
                return NotFound(new { success = false, message = "Không tìm thấy cửa hàng nào." });

            return Ok(new
            {
                success = true,
                store = nearest,
                distance = minDist,
                userLocation = new { latitude = userLat, longitude = userLng }
            });
        }
        // Get route between two points using external routing service
        [HttpPost]
        public async Task<IActionResult> GetRoute([FromBody] RouteRequest request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                var routeData = await GetRouteFromOpenRouteService(request);

                if (routeData != null)
                {
                    return Ok(new { success = true, route = routeData });
                }
                return BadRequest(new { success = false, message = "Lấy tuyến đường thất bại" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route");
                return StatusCode(500, new { success = false, message = "Lỗi khi tính toán đường đi." });
            }
        }
        private async Task<object?> GetRouteFromOpenRouteService(RouteRequest request, string preference = "fastest")
        {
            try
            {
                var apiKey = _orsOptions.Value.ApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("OpenRouteService API key not configured");
                    return null;
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                var requestBody = new
                {
                    coordinates = new[]
                    {
                new[] { request.StartLng, request.StartLat },
                new[] { request.EndLng, request.EndLat }
            },
                    preference = preference,
                    format = "geojson",
                    instructions = true,
                    language = "vi" // Vietnamese instructions
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "https://api.openrouteservice.org/v2/directions/driving-car/geojson",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<object>(responseContent);
                }

                _logger.LogWarning($"OpenRouteService returned {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenRouteService");
                return null;
            }
        }
        //PickUpMode
        [HttpGet]
        public async Task<IActionResult> GetAllBranchesWithDistance()
        {
            if (!HttpContext.Session.TryGetValue("UserLat", out var latBytes) ||
                !HttpContext.Session.TryGetValue("UserLng", out var lngBytes))
            {
                return BadRequest(new { success = false, message = "Chưa có vị trí người dùng." });
            }

            double userLat = double.Parse(System.Text.Encoding.UTF8.GetString(latBytes));
            double userLng = double.Parse(System.Text.Encoding.UTF8.GetString(lngBytes));
            var address = await _geoService.ReverseGeocodeAsync(userLat, userLng);

            var branches = await _locationService.GetAllLocationsAsync();
            const double minutesPerKm = 1.5;

            var results = branches.Select(store =>
            {
                double dist = _locationService.GetDistance(userLat, userLng, (double)store.Latitude, (double)store.Longitude);
                int estimate = (int)Math.Ceiling(dist * minutesPerKm);
                return new
                {
                    store.Idchinhanh,
                    store.Tencnhanh,
                    store.Diachicn,
                    Latitude = store.Latitude,
                    Longitude = store.Longitude,
                    DistanceKm = Math.Round(dist, 2),
                    EstimatedMinutes = estimate,
                    EstimatedTime = DateTime.Now.AddMinutes(estimate).ToString("HH:mm")
                };
            }).OrderBy(x => x.DistanceKm).ToList();

            return Ok(results);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBranches()
        {
            var branches = await _locationService.GetAllLocationsAsync();

            var results = branches.Select(store => new
            {
                store.Idchinhanh,
                store.Tencnhanh,
                store.Diachicn
            }).ToList();

            return Ok(results);
        }
        //Lưu Info PickUp Method
        [HttpPost]
        public IActionResult SaveSelectedStore([FromBody] SelectedStoreViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.BranchId))
                return BadRequest(new { success = false, message = "Thiếu dữ liệu" });

            var sessionData = new UserLocationSessionViewModel
            {
                NearestBranchId = model.BranchId,
                DistanceKm = Math.Round(model.DistanceKm, 2),
                EstimatedMinutes = model.EstimatedMinutes,
                DeliveryMethod = model.DeliveryMethod,
            };

            HttpContext.Session.SetString("UserLocationInfo", JsonSerializer.Serialize(sessionData));

            return Ok(new { success = true });
        }
        [HttpGet]
        public IActionResult SelectOrderType()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Index(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return RedirectToAction(nameof(SelectOrderType));
            }

            ViewBag.OrderType = type;
            ViewBag.OpenRouteApiKey = _orsOptions.Value.ApiKey;
            var locations = await _locationService.GetAllLocationsAsync();
            return View("Index", locations); // dùng View gốc
        }
    }
}
