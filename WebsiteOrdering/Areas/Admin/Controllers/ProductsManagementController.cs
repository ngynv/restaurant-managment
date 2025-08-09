using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteOrdering.Areas.Repository;
using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Models;
using WebsiteOrdering.Product.GetAllCategory;
using WebsiteOrdering.Product.GetAllCategoryById;
using WebsiteOrdering.Product.GetAllProducts;
using WebsiteOrdering.Services;

namespace WebsiteOrdering.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/ProductsManagement")]
    public class ProductsManagementController : Controller
    {
        private readonly IMonanRepository _monanRepository;
        private readonly ICategoryRepository _categoryRepository;
        public readonly IMediator _mediator;
        private readonly LuceneProductIndexer _luceneIndexer;
        public ProductsManagementController(IMonanRepository monanRepository, ICategoryRepository categoryRepository, 
            IMediator mediator, LuceneProductIndexer luceneProductIndexer)
        {
            _monanRepository = monanRepository;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _luceneIndexer = luceneProductIndexer;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string categoryId = null, string searchTerm = "", int pageSize = 7)
        {
            // Lấy tất cả categories cho dropdown filter
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.PageSize = pageSize;

            List<Monan> products;

            // Có tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Tìm kiếm chính xác trước
                var exactMatchProducts = await _mediator.Send(new GetProductsByExactNameQuery(searchTerm));
                if (exactMatchProducts != null && exactMatchProducts.Any())
                {
                    products = exactMatchProducts.ToList();
                }
                else
                {
                    // Sử dụng Lucene search nếu không có kết quả chính xác
                    var luceneResults = _luceneIndexer.SearchWithScore(searchTerm, 100);
                    var matchedIds = luceneResults.Select(r => r.Id).ToList();
                    var allProducts = await _monanRepository.GetAllAsync();
                    products = allProducts.Where(p => matchedIds.Contains(p.Idmonan)).ToList();
                }
            }
            else if (!string.IsNullOrEmpty(categoryId))
            {
                // Lọc theo category (bao gồm cả category con)
                var allChildrenIds = GetAllChildCategoryIds(categories, categoryId);
                allChildrenIds.Add(categoryId);

                products = new List<Monan>();
                foreach (var catId in allChildrenIds)
                {
                    var prods = await _mediator.Send(new GetProductsByCategoiesQuery(catId));
                    if (prods != null) products.AddRange(prods);
                }

                if (products.Count == 0)
                {
                    TempData["Message"] = "Không tìm thấy sản phẩm thuộc loại đã chọn.";
                }
            }
            else
            {
                // Lấy tất cả sản phẩm
                products = (await _monanRepository.GetAllAsync()).ToList();
            }

            // Phân trang
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Chuyển đổi sang ViewModel
            var viewModels = paginatedProducts.Select(m => new MonanIndexViewModel
            {
                Idmonan = m.Idmonan,
                Tenmonan = m.Tenmonan,
                Tenloaimonan = m.IdloaimonanNavigation?.Tenloaimonan,
                Anhmonan = m.Anhmonan,
                Giamonan = m.Giamonan,
                Trangthaiman = m.Trangthaiman
            }).ToList();

            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;

            return View(viewModels);
        }

        // Hàm đệ quy lấy tất cả category con của một category cha
        private List<string> GetAllChildCategoryIds(List<Loaimonan> categories, string parentId)
        {
            var result = new List<string>();
            if (categories == null || parentId == null)
                return result;

            var children = categories.Where(c => c.IdloaimanCha == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child.Idloaimonan);
                // Đệ quy lấy con của con
                result.AddRange(GetAllChildCategoryIds(categories, child.Idloaimonan));
            }
            return result;
        }
        [HttpGet("searchProducts")]
        public async Task<JsonResult> SearchProducts(string term)
        {
            var results = await _monanRepository.SearchProductsAsync(term);
            return Json(results);
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var loaiMonAn = await _categoryRepository.GetAllAsync();
            ViewBag.LoaiMonAn = new SelectList(loaiMonAn, "Idloaimonan", "Tenloaimonan");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var model = new MonanFormViewModel
                {
                    Idmonan = null,
                    Trangthaiman = "Còn hàng",
                    Anhmonan = null
                };
                return PartialView("_MonanFormPartial", model);
            }
            return BadRequest();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonanFormViewModel viewModel, IFormFile? anhMoi)
        {
            if (!ModelState.IsValid)
            {
                var loaiMonAn = await _categoryRepository.GetAllAsync();
                ViewBag.LoaiMonAn = new SelectList(loaiMonAn, "Idloaimonan", "Tenloaimonan", viewModel.Idloaimonan);
                return RedirectToAction(nameof(Index));
            }

            await _monanRepository.CreateAsync(viewModel, anhMoi);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var sanpham = await _monanRepository.GetByIdAsync(id);
            if (sanpham == null) return NotFound();

            var viewModel = new MonanFormViewModel
            {
                Idmonan = sanpham.Idmonan,
                Tenmonan = sanpham.Tenmonan,
                Idloaimonan = sanpham.Idloaimonan,
                Anhmonan = sanpham.Anhmonan,
                Giamonan = sanpham.Giamonan,
                Trangthaiman = sanpham.Trangthaiman,
                Mota = sanpham.Mota
            };

            var loaiMonAn = await _categoryRepository.GetAllAsync();
            ViewBag.LoaiMonAn = new SelectList(loaiMonAn, "Idloaimonan", "Tenloaimonan", viewModel.Idloaimonan);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MonanFormPartial", viewModel);
            }

            return BadRequest();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, MonanFormViewModel viewModel, IFormFile? anhMoi)
        {
            if (id != viewModel.Idmonan) return NotFound();

            if (!ModelState.IsValid)
            {
                var loaiMonAn = await _categoryRepository.GetAllAsync();
                ViewBag.LoaiMonAn = new SelectList(loaiMonAn, "Idloaimonan", "Tenloaimonan", viewModel.Idloaimonan);
                return PartialView("_MonanFormPartial", viewModel);
            }

            var success = await _monanRepository.UpdateAsync(id, viewModel, anhMoi);
            if (!success) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("Detail")]
        public async Task<IActionResult> Detail(string? id)
        {
            var monan = await _monanRepository.GetByIdAsync(id!);
            if (monan == null) return NotFound();

            var viewModel = new MonanDetailViewModel
            {
                Idmonan = monan.Idmonan,
                Tenmonan = monan.Tenmonan,
                Tenloaimonan = monan.IdloaimonanNavigation?.Tenloaimonan,
                Anhmonan = monan.Anhmonan,
                Giamonan = monan.Giamonan,
                Trangthaiman = monan.Trangthaiman,
                Mota = monan.Mota
            };

            return View(viewModel);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var monan = await _monanRepository.GetByIdAsync(id);
            if (monan == null) return NotFound();

            var viewModel = new MonanDeleteViewModel
            {
                Idmonan = monan.Idmonan,
                Tenmonan = monan.Tenmonan,
                Tenloaimonan = monan.IdloaimonanNavigation?.Tenloaimonan,
                Anhmonan = monan.Anhmonan,
                Giamonan = monan.Giamonan,
                Trangthaiman = monan.Trangthaiman
            };

            return View(viewModel);
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Do xóa sản phẩm có thể có điều kiện liên quan đến đơn hàng,
            return BadRequest("Hàm Delete chưa được tích hợp repository đầy đủ.");
        }

        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var monan = await _monanRepository.GetByIdAsync(id);
            if (monan == null) return NotFound();

            monan.Trangthaiman = monan.Trangthaiman == "Hiển thị" ? "Ẩn" : "Hiển thị";
            await _monanRepository.UpdateAsync(monan.Idmonan!, new MonanFormViewModel
            {
                Idmonan = monan.Idmonan,
                Tenmonan = monan.Tenmonan,
                Idloaimonan = monan.Idloaimonan,
                Giamonan = monan.Giamonan,
                Mota = monan.Mota,
                Trangthaiman = monan.Trangthaiman,
                Anhmonan = monan.Anhmonan
            }, null);

            return RedirectToAction(nameof(Index));
        }
    }
}
