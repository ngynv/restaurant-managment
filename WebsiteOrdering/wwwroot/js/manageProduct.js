document.addEventListener("DOMContentLoaded", function () {
    const modalContainer = document.getElementById("monanModalContainer");

    // Hàm tái sử dụng để bind lại các sự kiện trong modal
    function bindMonanModalEvents() {
        const modalEl = document.getElementById("monanModal");
        if (!modalEl) return;

        // Preview ảnh
        const fileInput = modalEl.querySelector("#anhMoi");
        const previewImage = modalEl.querySelector("#previewImage");

        if (fileInput && previewImage) {
            fileInput.addEventListener("change", function () {
                const file = this.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        previewImage.src = e.target.result;
                        previewImage.style.display = "block";
                    };
                    reader.readAsDataURL(file);
                } else {
                    previewImage.src = "#";
                    previewImage.style.display = "none";
                }
            });
        }
    }

    // Mở modal từ server và hiển thị lên giao diện
    function openMonanModal(url) {
        fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(response => {
                if (!response.ok) throw new Error("Không thể tải form");
                return response.text();
            })
            .then(html => {
                modalContainer.innerHTML = html;

                const modalEl = document.getElementById("monanModal");
                const modal = new bootstrap.Modal(modalEl, {
                    backdrop: 'static',
                    keyboard: false
                });
                modal.show();

                bindMonanModalEvents(); // bind sự kiện sau khi DOM đã render
            })
            .catch(err => {
                console.error("Lỗi mở modal:", err);
                alert("Có lỗi xảy ra khi mở form!");
            });
    }

    // Gắn sự kiện cho nút "Thêm món ăn"
    const btnAdd = document.getElementById("btnAddMonan");
    if (btnAdd) {
        btnAdd.addEventListener("click", function () {
            openMonanModal(`/Admin/ProductsManagement/Create`);
        });
    }

    // Gắn sự kiện cho các nút "Sửa"
    document.querySelectorAll(".btn-edit-monan").forEach(button => {
        button.addEventListener("click", function () {
            const id = this.dataset.id;
            openMonanModal(`/Admin/ProductsManagement/Edit/${id}`);
        });
    });

    // Gửi form từ modal bằng fetch POST
    document.addEventListener("submit", function (e) {
        if (e.target && e.target.id === "formMonAn") {
            e.preventDefault();

            const form = e.target;
            const formData = new FormData(form);

            // Disable submit button để tránh double submit
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.textContent;
            submitBtn.disabled = true;
            submitBtn.textContent = 'Đang xử lý...';

            fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(async (response) => {
                    const contentType = response.headers.get("Content-Type") || "";

                    if (response.ok) {
                        if (contentType.includes("application/json")) {
                            // Response là JSON - thành công
                            const result = await response.json();
                            if (result.success) {
                                const modalEl = document.getElementById("monanModal");
                                const modal = bootstrap.Modal.getInstance(modalEl);
                                modal.hide();

                                // Reload trang để cập nhật danh sách
                                location.reload();
                                return;
                            }
                        }
                    }

                    // Response là HTML - có validation errors
                    const html = await response.text();
                    modalContainer.innerHTML = html;
                    bindMonanModalEvents(); // rebind sau khi render lại form với errors
                })
                .catch(err => {
                    console.error("Lỗi submit form:", err);
                    alert("Có lỗi xảy ra khi lưu dữ liệu!");
                })
                .finally(() => {
                    // Re-enable submit button
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.textContent = originalText;
                    }
                });
        }
    });
});