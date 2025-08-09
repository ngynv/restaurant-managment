using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Controllers
{
    [Authorize]
    public class PersonalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;

        public PersonalController(AppDbContext context, IAccountRepository accountRepository, IOrderRepository orderRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
        }
        
        public async Task<IActionResult> Personal(string? tab)
        {
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profileModel = new UpdateProfileViewModel
            {
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            var bookings = await _context.Datbans
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .Where(d => d.Idngdung == user.Id)
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();

            var orders = await _orderRepository.GetOrdersByUserIdAsync(user.Id);

            var model = new PersonalPageViewModel
            {
                Profile = profileModel,
                ListDatBan = bookings,
                ListDonHang = orders
            };

            ViewBag.SelectedTab = tab ?? "info";

            return View("Personal", model);
        }

        // Load lịch sử đặt bàn (dùng cho Ajax)
        [HttpGet]
        public async Task<IActionResult> PartialBookingHistoryPartial()
        {
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var bookings = await _context.Datbans
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .Where(d => d.Idngdung == user.Id)
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();

            return PartialView("_PartialBookingHistory", bookings);
        }

        // Load lịch sử đơn hàng (dùng cho Ajax)
        [HttpGet]
        public async Task<IActionResult> PartialOrderHistoryPartial()
        {
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var orders = await _orderRepository.GetOrdersByUserIdAsync(user.Id);

            return PartialView("_PartialOrderHistory", orders);
        }
    }
}