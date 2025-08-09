using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Helper
{
    public static class TrangThaiExtensions
    {
        public static string ToFriendly(this TrangThai trangThai)
        {
            return trangThai switch
            {
                TrangThai.Pending => "Chờ xác nhận",
                TrangThai.Paid => "Đã thanh toán",
                TrangThai.Preparing => "Đang chuẩn bị",
                TrangThai.Delivering => "Đang giao hàng",
                TrangThai.Completed => "Hoàn tất",
                TrangThai.Cancelled => "Đã hủy",
                TrangThai.Ordered => "Đã đặt món",
                TrangThai.Eating => "Đang dùng bữa",
                TrangThai.Confirmed => "Đã xác nhận",
                TrangThai.Came => "Đã đến quán",
                _ => "Không xác định"
            };
        }
    }
}
