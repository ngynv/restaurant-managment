using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;
using WebsiteOrdering.Product.GetAllCategory;
using WebsiteOrdering.Product.GetAllCategoryById;
using WebsiteOrdering.Product.GetAllProducts;
using WebsiteOrdering.Product.GetProductById;
using WebsiteOrdering.Helper;
using WebsiteOrdering.ViewModels;
using WebsiteOrdering.Services;
using WebsiteOrdering.Areas.Repository; // Để dùng session extension (Get<T>)

namespace WebsiteOrdering.Controllers
{
    [Route("Products")]
    public class ProductsController : Controller
    {
        public readonly IMediator _mediator;
        private readonly AppDbContext _appDbContext;
        private readonly LuceneProductIndexer _luceneIndexer;
        private readonly IMonanRepository _monanRepository;

        public ProductsController(IMediator mediator, AppDbContext appDbContext,
            LuceneProductIndexer luceneIndexer, IMonanRepository monanRepository)
        {
            _mediator = mediator;
            _appDbContext = appDbContext;
            _luceneIndexer = luceneIndexer;
            _monanRepository = monanRepository;
        }

        //[HttpGet("")]
        //public async Task<IActionResult> Index(int page = 1, string categoryId = null)
        //{

        //    var categories = await _mediator.Send(new GetAllCategoriesQuery());
        //    ViewBag.Categories = categories;

        //    List<Monan> products;

        //    if (!string.IsNullOrEmpty(categoryId))
        //    {
        //        var allChildrenIds = GetAllChildCategoryIds(categories, categoryId);
        //        allChildrenIds.Add(categoryId); // Thêm chính nó

        //        products = new List<Monan>();

        //        foreach (var catId in allChildrenIds)
        //        {
        //            var prods = await _mediator.Send(new GetProductsByCategoiesQuery(catId));
        //            if (prods != null && prods.Count > 0)
        //            {
        //                products.AddRange(prods);
        //            }
        //        }

        //        ViewBag.SelectedCategory = categoryId;

        //        if (products.Count == 0)
        //        {
        //            TempData["Message"] = "Không tìm thấy sản phẩm thuộc loại đã chọn.";
        //        }
        //    }
        //    else
        //    {
        //        products = await _mediator.Send(new GetAllProductQuery());
        //        ViewBag.SelectedCategory = null;
        //    }

        //    // ✅ KHÔNG PHÂN TRANG – hiện tất cả
        //    ViewBag.CurrentPage = 1;
        //    ViewBag.TotalPages = 1;

        //    // ✅ LẤY GIỎ HÀNG TỪ SESSION
        //    var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
        //    ViewBag.CartItems = cart;

        //    return View(products);
        //}

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string categoryId = null, string categoryIds = null, string searchTerm = "")
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            List<Monan> products = new();
            // Có tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var exactMatchProducts = await _mediator.Send(new GetProductsByExactNameQuery(searchTerm));

                if (exactMatchProducts != null && exactMatchProducts.Any())
                {
                    products = exactMatchProducts.ToList();
                }
                else
                {
                    var luceneResults = _luceneIndexer.SearchWithScore(searchTerm, 100);
                    var matchedIds = luceneResults.Select(r => r.Id).ToList();
                    var allProducts = await _mediator.Send(new GetAllProductQuery());
                    products = allProducts.Where(p => matchedIds.Contains(p.Idmonan)).ToList();
                }

                ViewBag.SelectedCategory = null;
            }
            else if (!string.IsNullOrEmpty(categoryId))
            {
                var allChildrenIds = GetAllChildCategoryIds(categories, categoryId);
                allChildrenIds.Add(categoryId);

                foreach (var catId in allChildrenIds)
                {
                    var prods = await _mediator.Send(new GetProductsByCategoiesQuery(catId));
                    if (prods != null && prods.Count > 0)
                    {
                        products.AddRange(prods);
                    }
                }

                ViewBag.SelectedCategory = categoryId;

                if (products.Count == 0)
                {
                    TempData["Message"] = "Không tìm thấy sản phẩm thuộc loại đã chọn.";
                }
            }
            else
            {
                products = await _mediator.Send(new GetAllProductQuery());
                ViewBag.SelectedCategory = null;
            }

            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = 1;

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            ViewBag.CartItems = cart;

            return View(products);
        }


        // ✅ Hàm đệ quy lấy tất cả category con của một category cha
        private List<string> GetAllChildCategoryIds(List<Loaimonan> categories, string parentId)
        {
            var result = new List<string>();
            if (categories == null || parentId == null)
                return result;

            var children = categories.Where(c => c.IdloaimanCha == parentId).ToList();

            foreach (var child in children)
            {
                result.Add(child.Idloaimonan);
                result.AddRange(GetAllChildCategoryIds(categories, child.Idloaimonan)); // đệ quy
            }

            return result;
        }

        // ✅ Hiển thị chi tiết sản phẩm
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var product = await _mediator.Send(new GetProductsByIdQuery(id));
            if (product == null)
                return NotFound();

            // Thêm field phụ để gán cha
            if (product.Idloaimonan == "LMA08" || product.Idloaimonan == "LMA09")
            {
                product.Idloaimonan = "LMA07"; // như bạn đã làm
            }
            else if (product.Idloaimonan == "LMA10" || product.Idloaimonan == "LMA11" || product.Idloaimonan == "LMA12" || product.Idloaimonan == "LMA13" || product.Idloaimonan == "LMA14")
            {
                product.Idloaimonan = "LMA08"; // gán cha
            }
            else if (product.Idloaimonan == "LMA15" || product.Idloaimonan == "LMA16")
            {
                product.Idloaimonan = "LMA09"; // gán cha
            }

            return View(product);
        }

        // ✅ THÊM ACTION CHUẨN BỊ DỮ LIỆU
        [HttpPost("PrepareEdit")]
        public IActionResult PrepareEdit(
            string id, string idmonan2, string size, string debanh,
            string ghichu, int soluong, List<string> toppings)
        {
            TempData["edit"] = true;
            TempData["idmonan2"] = idmonan2;
            TempData["size"] = size;
            TempData["debanh"] = debanh;
            TempData["ghichu"] = ghichu;
            TempData["soluong"] = soluong;
            TempData["toppings"] = string.Join(",", toppings ?? new List<string>());

            return RedirectToAction("Edit", new { id });
        }

        // ✅ THÊM ACTION Edit — lặp logic cha con y như Detail
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _mediator.Send(new GetProductsByIdQuery(id));
            if (product == null)
                return NotFound();

            // 🚫 Lặp lại logic cha-con y chang Detail
            if (product.Idloaimonan == "LMA08" || product.Idloaimonan == "LMA09")
            {
                product.Idloaimonan = "LMA07";
            }
            else if (product.Idloaimonan == "LMA10" || product.Idloaimonan == "LMA11" || product.Idloaimonan == "LMA12" || product.Idloaimonan == "LMA13" || product.Idloaimonan == "LMA14")
            {
                product.Idloaimonan = "LMA08";
            }
            else if (product.Idloaimonan == "LMA15" || product.Idloaimonan == "LMA16")
            {
                product.Idloaimonan = "LMA09";
            }

            if (TempData.ContainsKey("edit"))
            {
                ViewBag.IsEdit = true;
                ViewBag.idmonan2 = TempData["idmonan2"]?.ToString();
                ViewBag.size = TempData["size"]?.ToString();
                ViewBag.debanh = TempData["debanh"]?.ToString();
                ViewBag.ghichu = TempData["ghichu"]?.ToString();
                ViewBag.soluong = TempData["soluong"]?.ToString();
                ViewBag.toppings = TempData["toppings"]?.ToString()
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            }
            else
            {
                ViewBag.IsEdit = false;
            }

            // ⚡ Vẫn trả View "Detail"
            return View("Detail", product);
        }
        // Action API cho tìm kiếm AJAX
        [HttpGet("searchProducts")]
        public JsonResult SearchProducts(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var searchResults = _luceneIndexer.SearchWithScore(term, 10);
            // Tách ID ra để EF xử lý được
            var resultIds = searchResults.Select(r => r.Id).ToList();

            var products = _appDbContext.SanPhams
                .Where(p => resultIds.Contains(p.Idmonan))
                .Select(p => new
                {
                    id = p.Idmonan,
                    name = p.Tenmonan,
                })
                .ToList();
            return Json(products);
        }
    }
}