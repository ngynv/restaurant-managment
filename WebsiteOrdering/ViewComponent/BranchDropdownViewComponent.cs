using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.ViewComponents
{
    public class BranchDropdownViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public BranchDropdownViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? selectedBranchId, bool showLabel = true)
        {
            var branches = _context.chinhanh
                .Select(b => new SelectListItem
                {
                    Value = b.Idchinhanh,
                    Text = b.Tencnhanh,
                })
                .ToList();

            ViewBag.SelectedBranchId = selectedBranchId;
            ViewBag.showLabel = showLabel;
            return View(branches);
        }
    }
}
