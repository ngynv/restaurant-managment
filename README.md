# 🍕 Restaurant Ordering & Reservation Website

Ứng dụng web đặt món cho nhà hàng chuyên các món:
- Pizza
- Mì Ý
- Salad
- Món khai vị (súp bí đỏ, bánh mì bơ tỏi,...)
- Bít tết
- Món tráng miệng (sữa chua, trái cây,...)
- Nước uống (có cồn & không cồn)

---
## 🌐 Giới thiệu tổng quan

**Pizza Ordering Web App** là nền tảng đặt món trực tuyến hỗ trợ đặt hàng, đặt bàn, thanh toán online và quản lý vận hành nhà hàng. Hệ thống gồm 3 vai trò:

- **Customer (Khách hàng)**: đặt món, theo dõi đơn hàng, đặt bàn, cập nhật thông tin cá nhân...
- **Staff (Nhân viên)**: tạo đơn hàng tại quầy, xử lý yêu cầu đặt bàn theo chi nhánh.
- **Admin (Quản trị viên)**: quản lý toàn bộ hệ thống, sản phẩm, cửa hàng, đơn hàng(Lọc theo chi nhánh) và người dùng.

### 🧑‍🍳 Các tính năng chính

#### 🧾 Khách hàng:
- Đăng ký, đăng nhập bằng Gmail, Google, SĐT
- Xem menu, chi tiết món ăn (hỗ trợ Pizza nguyên và Pizza ghép)
- Chọn size, topping theo từng loại sản phẩm
- Thêm món vào giỏ hàng, đặt hàng và thanh toán:
  - Tiền mặt hoặc thanh toán online(VnPay)
- Theo dõi trạng thái đơn hàng, hủy đơn nếu cần
- Chọn nhận tại nhà hoặc đến cửa hàng(Sử dụng OpenStreetMap)
- Xem bản đồ các cửa hàng gần
- Đặt bàn theo thời gian và số người
- Xem, chỉnh sửa, hủy yêu cầu đặt bàn
- Xem lịch sử đơn hàng, lịch sử đặt bàn
- Cập nhật thông tin cá nhân
- Tìm kiếm món ăn thông minh(Lucene.NET)

#### 🧑‍💼 Nhân viên:
- Tạo đơn hàng tại cửa hàng
- Quản lý yêu cầu đặt bàn, khóa bàn theo yêu cầu
- Xác nhận đơn, gửi email thông báo

#### 🛠️ Quản trị viên:
- Quản lý sản phẩm
  - Thêm, sửa, lọc, tìm kiếm
- Quản lý đơn hàng (cập nhật trạng thái, gửi thông báo)
- Quản lý yêu cầu đặt bàn
- Thống kê doanh thu, sản phẩm bán chạy

---
## 🚀 Hướng dẫn sử dụng

### 1. ⚙️ Cấu hình chuỗi kết nối CSDL

Mở file `appsettings.json` và sửa phần `ConnectionStrings`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PizzaOrdering;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

📌 **Lưu ý:**
- Nếu dùng SQL Server Authentication:

  ```json
  "DefaultConnection": "Server=localhost;Database=PizzaOrdering;User Id=sa;Password=yourpassword;TrustServerCertificate=True;"
  ```

- Đảm bảo SQL Server đang chạy và có quyền tạo database.

🔐 Lưu ClientId và ClientSecret của Google:

dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
---

### 2. 🛠️ Cập nhật database bằng Entity Framework Core

Mở terminal tại thư mục chứa `.csproj` và chạy:

```bash
dotnet ef database update
```

Lệnh này sẽ:
- Tự động tạo database (nếu chưa có)
- Tạo bảng dựa theo các migration đã có

> ⚠️ Nếu chưa có migration nào, bạn có thể tạo bằng:
> ```bash
> dotnet ef migrations add InitialCreate
> ```

---

### 3. ▶️ Chạy ứng dụng

```bash
dotnet run
```

Mở trình duyệt và truy cập `https://localhost:port` để bắt đầu sử dụng ứng dụng.

---

## ✅ Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server
- Entity Framework Core CLI (`dotnet tool install --global dotnet-ef`)
