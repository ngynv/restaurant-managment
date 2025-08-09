// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".cart-select");
    const totalLabel = document.getElementById("totalAmount");

    function formatCurrency(amount) {
        return amount.toLocaleString("vi-VN") + " VNĐ";
    }

    function calculateTotal() {
        let total = 0;
        checkboxes.forEach(function (cb) {
            if (cb.checked) {
                total += parseInt(cb.dataset.tongtien);
            }
        });
        totalLabel.innerText = formatCurrency(total);
    }

    checkboxes.forEach(cb => {
        cb.addEventListener("change", calculateTotal);
    });
    //Xử lý nút tăng giảm
    document.querySelectorAll(".btn-increase, .btn-decrease").forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();

            const input = this.parentElement.querySelector(".quantity-input");
            let value = parseInt(input.value);
            if (isNaN(value)) value = 1;

            if (this.classList.contains("btn-increase")) {
                value += 1;
            } else if (value > 1) {
                value -= 1;
            }
            const form = this.closest("form");
            const formData = new FormData(form);
            formData.set("soluong", value);

            fetch("/Cart/UpdateCart", {
                method: "POST",
                body: formData
            })
                .then(response => response.json()) // server trả về JSON gồm tổng tiền mới
                .then(data => {
                    if (data.success) {
                        fetch("/Cart/CartCountPartial")
                            .then(res => res.text())
                            .then(html => {
                                document.getElementById("cart-count").outerHTML = html;
                            });
                        input.value = value;

                        const row = form.closest("tr");
                        const cb = row.querySelector(".cart-select");
                        cb.dataset.tongtien = data.tongTienMoi; // cập nhật giá trị data-tongtien
                        const tongTienCell = row.querySelector("td:nth-last-child(2)");
                        tongTienCell.innerText = data.tongTienMoi.toLocaleString("vi-VN") + " VNĐ";
                        calculateTotal();

                    } else {
                        alert("Cập nhật thất bại!");
                    }
                });

        });
    });
    calculateTotal();


    // Xử lý nút lưu thông tin trong modal
    document.getElementById("edit-cart-form").addEventListener("submit", function (e) {
        e.preventDefault();

        const form = this;
        const formData = new FormData(form);

        fetch("/Cart/UpdateCartItem", {
            method: "POST",
            body: formData
        })
            .then(res => {
                if (!res.ok) throw new Error("Lỗi HTTP: " + res.status);
                return res.json();
            })
            .then(data => {
                console.log("✅ Phản hồi từ server:", data);
                if (data.success) {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editModal'));
                    modal.hide();
                    alert("Cập nhật thành công!");
                    location.reload();
                } else {
                    const message = data.message || "Không rõ nguyên nhân";
                    console.error("❌ Lỗi từ server:", message);
                    alert("Cập nhật thất bại: " + message);
                }
            })
            .catch(err => {
                console.error("❗Lỗi fetch:", err);
                alert("Có lỗi khi gửi dữ liệu lên server: " + err.message);
            });

    });


    // Xử lý nút sửa giỏ hàng
    document.querySelectorAll(".btn-edit").forEach(function (btn) {
        btn.addEventListener("click", function () {
            const cartItem = {
                idmonan: this.dataset.idmonan,
                idmonan2: this.dataset.idmonan2 || "",
                size: this.dataset.size,
                debanh: this.dataset.debanh,
                soluong: this.dataset.soluong,
                ghichu: this.dataset.ghichu,
                toppings: this.dataset.toppings || "",
                giacoban: parseInt(this.dataset.giacoban || 0),
                giasize: parseInt(this.dataset.giasize || 0),
                giadebanh: parseInt(this.dataset.giadebanh || 0),
            };

            // Gọi hàm hiển thị modal
            showEditModal(cartItem);
            // Gán hidden values gốc
            document.getElementById("edit-original-idmonan").value = this.dataset.idmonan;
            document.getElementById("edit-original-idmonan2").value = this.dataset.idmonan2 || "";
            document.getElementById("edit-original-size").value = this.dataset.size;
            document.getElementById("edit-original-debanh").value = this.dataset.debanh;
            document.getElementById("edit-original-toppings").value = this.dataset.toppings;

            // Gán hidden values mới (giữ nguyên món ăn hiện tại)
            document.getElementById("edit-idmonan").value = this.dataset.idmonan;
            document.getElementById("edit-idmonan2").value = this.dataset.idmonan2 || "";

            // Set dropdown
            document.getElementById("edit-size").value = this.dataset.size;
            document.getElementById("edit-debanh").value = this.dataset.debanh;

            // Set số lượng, ghi chú
            document.getElementById("edit-soluong").value = this.dataset.soluong;
            document.getElementById("edit-ghichu").value = this.dataset.ghichu;

            // Reset topping checkbox
            document.querySelectorAll(".edit-topping-checkbox").forEach(cb => cb.checked = false);
            const selectedToppings = (this.dataset.toppings || "").split(',');
            selectedToppings.forEach(tid => {
                const cb = document.querySelector(`#edit_top_${tid}`);
                if (cb) cb.checked = true;
            });

            // Cập nhật tổng tiền
            const giaCoBan = parseInt(this.dataset.giacoban || 0);
            const giaSize = parseInt(this.dataset.giasize || 0);
            const giaDeBanh = parseInt(this.dataset.giadebanh || 0);
            const giaTopping = selectedToppings.reduce((sum, tid) => {
                const cb = document.querySelector(`#edit_top_${tid}`);
                if (cb && cb.nextElementSibling.innerText.includes("+")) {
                    const priceText = cb.nextElementSibling.innerText.match(/\+(\d+)/);
                    if (priceText && priceText[1]) {
                        return sum + parseInt(priceText[1]);
                    }
                }
                return sum;
            }, 0);
            const soLuong = parseInt(this.dataset.soluong || 1);
            const tongTien = (giaCoBan + giaSize + giaDeBanh + giaTopping) * soLuong;
            document.getElementById("edit-total-price").innerText = tongTien.toLocaleString('vi-VN') + " VNĐ";

            // Mở modal
            const editModal = new bootstrap.Modal(document.getElementById('editModal'));
            editModal.show();
        });
    });
    function loadOptionsForEditModal(idmonan) {
        fetch(`/Cart/GetOptionsByMonAnId?idmonan=${idmonan}`)
            .then(res => res.json())
            .then(data => {
                // Load size
                const sizeSelect = document.getElementById("edit-size");
                sizeSelect.innerHTML = "";
                if (data.sizes.length > 0) {
                    sizeSelect.closest('.mb-3').style.display = 'block';
                    data.sizes.forEach(s => {
                        sizeSelect.innerHTML += `<option value="${s.idsize}">${s.ten}</option>`;
                    });
                } else {
                    sizeSelect.closest('.mb-3').style.display = 'none';
                }

                // Load đế bánh
                const debanhSelect = document.getElementById("edit-debanh");
                debanhSelect.innerHTML = "";
                if (data.debanhs.length > 0) {
                    debanhSelect.closest('.mb-3').style.display = 'block';
                    data.debanhs.forEach(d => {
                        debanhSelect.innerHTML += `<option value="${d.iddebanh}">${d.ten}</option>`;
                    });
                } else {
                    debanhSelect.closest('.mb-3').style.display = 'none';
                }

                // Load topping
                const toppingList = document.getElementById("edit-topping-list");
                toppingList.innerHTML = "";
                if (data.toppings.length > 0) {
                    toppingList.closest('.mb-3').style.display = 'block';
                    data.toppings.forEach(t => {
                        toppingList.innerHTML += `
                                            <div class="form-check">
                                                <input class="form-check-input edit-topping-checkbox" type="checkbox"
                                                       name="selectedToppingIds" value="${t.idtopping}" id="edit_top_${t.idtopping}">
                                                <label class="form-check-label" for="edit_top_${t.idtopping}">
                                                    ${t.ten} (+${t.gia} VNĐ)
                                                </label>
                                            </div>`;
                    });
                } else {
                    toppingList.closest('.mb-3').style.display = 'none';
                }
            });
    }

    function showEditModal(cartItem) {
        fetch(`/Cart/GetOptionsByMonAnId?idmonan=${cartItem.idmonan}`)
            .then(res => res.json())
            .then(data => {
                // Ẩn/hiện Size
                const sizeDiv = document.querySelector('#edit-size').closest('.mb-3');
                if (data.sizes.length > 0) {
                    sizeDiv.style.display = 'block';
                    const sizeSelect = document.getElementById('edit-size');
                    sizeSelect.innerHTML = '';
                    data.sizes.forEach(size => {
                        const opt = document.createElement('option');
                        opt.value = size.idsize;
                        opt.textContent = size.ten;
                        sizeSelect.appendChild(opt);
                    });
                } else {
                    sizeDiv.style.display = 'none';
                }

                // Ẩn/hiện Đế bánh
                const debanhDiv = document.querySelector('#edit-debanh').closest('.mb-3');
                if (data.debanhs.length > 0) {
                    debanhDiv.style.display = 'block';
                    const debanhSelect = document.getElementById('edit-debanh');
                    debanhSelect.innerHTML = '';
                    data.debanhs.forEach(d => {
                        const opt = document.createElement('option');
                        opt.value = d.iddebanh;
                        opt.textContent = d.ten;
                        debanhSelect.appendChild(opt);
                    });
                } else {
                    debanhDiv.style.display = 'none';
                }

                // Ẩn/hiện Topping
                const toppingDiv = document.querySelector('#edit-topping-list').closest('.mb-3');
                const toppingList = document.getElementById('edit-topping-list');
                toppingList.innerHTML = '';
                if (data.toppings.length > 0) {
                    toppingDiv.style.display = 'block';
                    data.toppings.forEach(t => {
                        const div = document.createElement('div');
                        div.className = 'form-check';
                        div.innerHTML = `
                                        <input class="form-check-input edit-topping-checkbox" type="checkbox"
                                               name="selectedToppingIds" value="${t.idtopping}" id="edit_top_${t.idtopping}">
                                        <label class="form-check-label" for="edit_top_${t.idtopping}">
                                            ${t.ten} (+${t.gia} VNĐ)
                                        </label>`;
                        toppingList.appendChild(div);
                    });
                } else {
                    toppingDiv.style.display = 'none';
                }

                // Mở modal
                const modal = new bootstrap.Modal(document.getElementById('editModal'));
                modal.show();
            });
    }
});
// =======================
// HIỆU ỨNG CHO INTRO
// =======================
document.addEventListener("DOMContentLoaded", function () {
    const section = document.querySelector('.intro-section');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                section.classList.add('show');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 });

    if (section) observer.observe(section);
});

// =======================
// HIỆU ỨNG PARALLAX SCROLL
// =======================
window.addEventListener("scroll", function () {
    const leftImg = document.querySelector("#parallax-left img");
    const rightImg = document.querySelector("#parallax-right img");

    if (!leftImg || !rightImg) return;

    const leftRect = leftImg.getBoundingClientRect();
    const rightRect = rightImg.getBoundingClientRect();
    const windowHeight = window.innerHeight;

    // Kiểm tra xem có trong khung nhìn không
    if (leftRect.bottom > 0 && leftRect.top < windowHeight) {
        const offset = (leftRect.top + leftRect.height / 2 - windowHeight / 2);
        const moveLeft = offset * -0.1; // Lên
        const moveRight = offset * 0.1; // Xuống
        leftImg.style.transform = `translateY(${moveLeft}px)`;
        rightImg.style.transform = `translateY(${moveRight}px)`;
    }
});
// =======================
// HIỆU ỨNG XOAY CHO SHAPE
// =======================
document.addEventListener("DOMContentLoaded", function () {
    const shape = document.getElementById("quote-shape");
    const quoteText = document.getElementById("quote-text");

    if (!shape || !quoteText) return;

    const quotes = [
        `"Mì tươi hấp dẫn, biến hóa muôn hình vạn vị."`,
        `“Từ lửa, bột và đam mê.”`,
        `"Một niềm vui để thưởng thức chậm rãi."`,
    ];

    const scales = [1.3, 1, 1];             // Scale mỗi bước
    const rotationSteps = [-100, -180, -80]; // Các bước quay một chiều (âm)

    let state = 0;
    let rotationAngle = 0; // Tổng góc xoay cộng dồn

    setInterval(() => {
        // Cộng thêm góc theo bước (luôn âm)
        rotationAngle += rotationSteps[state];

        // Cập nhật transform
        shape.style.transform = `rotate(${rotationAngle}deg) scale(${scales[state]})`;

        // Cập nhật quote
        quoteText.textContent = quotes[state];

        // Sang bước tiếp theo
        state = (state + 1) % quotes.length;
    }, 5000);
});

// =======================
// HIỆU ỨNG CHO AWARD
// =======================
document.addEventListener("DOMContentLoaded", function () {
    const awardContainer = document.querySelector('.award-container');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                awardContainer.classList.add('show');
                observer.unobserve(entry.target); // chỉ chạy một lần
            }
        });
    }, {
        threshold: 0.3
    });

    if (awardContainer) observer.observe(awardContainer);
});
document.addEventListener("DOMContentLoaded", function () {
    const awardContainer = document.querySelector('.award-container');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                awardContainer.classList.add('show');
                observer.unobserve(entry.target); // chỉ chạy 1 lần
            }
        });
    }, {
        threshold: 0.3
    });

    if (awardContainer) observer.observe(awardContainer);
});

// =======================
// HIỆU ỨNG CHO PARAGRAPH
// =======================
document.addEventListener("DOMContentLoaded", function () {
    const paragraphSection = document.querySelector('.paragraph-section');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                paragraphSection.classList.add('show');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.3 });

    if (paragraphSection) observer.observe(paragraphSection);
});

// =========================
// HIỆU ỨNG CHO SPLIDE SLIDE
// =========================
document.addEventListener('DOMContentLoaded', function () {
    const carousel = document.querySelector('#svg-carousel');
    if (!carousel) return;

    const splide = new Splide(carousel, {
        type: 'loop',
        autoplay: true,
        interval: 5000,
        speed: 800,
        arrows: true,
        pagination: true,
        updateOnMove: true,
        pauseOnHover: true,
        resetProgress: false,
        focus: 'center',
        padding: '18%',
        gap: '1rem',
    });

    function updateOverlays() {
        const slides = carousel.querySelectorAll('.splide__slide');

        slides.forEach(slide => {
            const overlay = slide.querySelector('.svg-overlay');
            if (!overlay) return;

            // Nếu là slide active ở chính giữa
            if (slide.classList.contains('is-active')) {
                overlay.setAttribute('fill-opacity', '0');
            } else {
                overlay.setAttribute('fill-opacity', '0.5');
            }
        });
    }

    splide.on('mounted move moved updated', () => {
        requestAnimationFrame(updateOverlays);
    });

    splide.mount();
});
