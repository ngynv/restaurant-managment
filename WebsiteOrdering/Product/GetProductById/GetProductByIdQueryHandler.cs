using MediatR;
using Microsoft.EntityFrameworkCore;

using WebsiteOrdering.Models;
using WebsiteOrdering.Product.GetAllProducts;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Product.GetProductById
{
    public class GetProductByIdQueryHandler :IRequestHandler<GetProductsByIdQuery, Monan>
    {
        private readonly AppDbContext _appDbContext;
        public GetProductByIdQueryHandler (AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Monan?> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _appDbContext.SanPhams
                .Include(p => p.IdloaimonanNavigation)
                .Where(p => p.Idmonan == request.Id)
                .Select(p => new Monan
                {
                    Idmonan = p.Idmonan,

                    Tenmonan = p.Tenmonan,
                    Mota = p.Mota,
                    Giamonan = p.Giamonan,
                    Trangthaiman = p.Trangthaiman,
                    Anhmonan = p.Anhmonan,

                    SoLuong = 1,
                    Idloaimonan = p.Idloaimonan,
                    //Hiển thị size theo loại món ăn

                    ListGiaSizes = _appDbContext.ListGiaSizes
                    .Where(g => g.Idloaimonan == p.Idloaimonan)
                    .Select(g => new Listgiasize
                    {
                        Idsize = g.Idsize,
                        Giasize = g.Giasize,
                        IdsizeNavigation = new Size
                        {
                            Idsize = g.IdsizeNavigation.Idsize,
                            Tensize = g.IdsizeNavigation.Tensize
                        }
                    }).ToList(),


                    //Hiển thị đế bánh
                    DeBanh = _appDbContext.debanh
                    .Select(d => new Debanh
                    {
                        Iddebanh = d.Iddebanh,
                        Tendebanh = d.Tendebanh,
                        Giadebanh = d.Giadebanh
                    }).ToList(),


                    //Hiển thị topping theo loại món ăn
                    Toppings = _appDbContext.Topping
                     .Where(t => t.Idloaimonan == p.Idloaimonan)
                     .Select(t => new Topping
                     {
                         Idtopping = t.Idtopping,
                         Tentopping = t.Tentopping,
                         Giatopping = t.Giatopping
                     }).ToList(),
                })
                .FirstOrDefaultAsync(cancellationToken);



            //Hàm để lấy được pizza ghép

            if (product != null && product.Idloaimonan == "LMA01")
            {
                // Lấy tất cả các món pizza khác (có cùng loại LMA01 nhưng khác ID)
                var pizzaGhepList = await _appDbContext.SanPhams
                    .Where(sp => sp.Idloaimonan == "LMA01" && sp.Idmonan != product.Idmonan)
                    .Select(sp => new SanPhamViewModel
                    {
                        IDMONAN2 = sp.Idmonan,
                        TENMONAN2 = sp.Tenmonan,
                        GIACOBAN2 = (sp.Giamonan + product.Giamonan) / 2,
                        ANHMONAN2 = sp.Anhmonan
                    })
                    .ToListAsync(cancellationToken);

                product.PizzaGhep = pizzaGhepList;
            }


            return product;
        }
        //public async Task<Monan?> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken)
        //{
        //    var p = await _appDbContext.SanPhams
        //        .Include(p => p.IdloaimonanNavigation)
        //        .FirstOrDefaultAsync(p => p.Idmonan.Trim() == request.Id.Trim(), cancellationToken);
        //    Console.WriteLine($"Handler nhận idMon: {request.Id}");

        //    if (p == null)
        //        return null;

        //    var mon = new Monan
        //    {
        //        Idmonan = p.Idmonan,
        //        Tenmonan = p.Tenmonan,
        //        Mota = p.Mota,
        //        Giamonan = p.Giamonan,
        //        Trangthaiman = p.Trangthaiman,
        //        Anhmonan = p.Anhmonan,
        //        SoLuong = 1,
        //        Idloaimonan = p.Idloaimonan,
        //    };

        //    // Load collection phụ
        //    mon.ListGiaSizes = await _appDbContext.ListGiaSizes
        //        .Where(g => g.Idloaimonan == p.Idloaimonan)
        //        .Select(g => new Listgiasize
        //        {
        //            Idsize = g.Idsize,
        //            Giasize = g.Giasize,
        //            IdsizeNavigation = new Size
        //            {
        //                Idsize = g.IdsizeNavigation.Idsize,
        //                Tensize = g.IdsizeNavigation.Tensize
        //            }
        //        })
        //        .ToListAsync(cancellationToken);

        //    mon.DeBanh = await _appDbContext.debanh
        //        .Select(d => new Debanh
        //        {
        //            Iddebanh = d.Iddebanh,
        //            Tendebanh = d.Tendebanh,
        //            Giadebanh = d.Giadebanh
        //        })
        //        .ToListAsync(cancellationToken);

        //    mon.Toppings = await _appDbContext.Topping
        //        .Where(t => t.Idloaimonan == p.Idloaimonan)
        //        .Select(t => new Topping
        //        {
        //            Idtopping = t.Idtopping,
        //            Tentopping = t.Tentopping,
        //            Giatopping = t.Giatopping
        //        })
        //        .ToListAsync(cancellationToken);

        //    // Pizza ghép
        //    if (mon.Idloaimonan == "LMA01")
        //    {
        //        var pizzaGhepList = await _appDbContext.SanPhams
        //            .Where(sp => sp.Idloaimonan == "LMA01" && sp.Idmonan != mon.Idmonan)
        //            .Select(sp => new SanPhamViewModel
        //            {
        //                IDMONAN2 = sp.Idmonan,
        //                TENMONAN2 = sp.Tenmonan,
        //                GIACOBAN2 = (sp.Giamonan + mon.Giamonan) / 2,
        //                ANHMONAN2 = sp.Anhmonan
        //            })
        //            .ToListAsync(cancellationToken);

        //        mon.PizzaGhep = pizzaGhepList;
        //    }

        //    return mon;
        //}

    }
}
