
document.addEventListener("DOMContentLoaded", function () {
    // Lấy ô số người
    const soNguoiInput = document.querySelector('input[name="Songuoidat"]');
    if (soNguoiInput && (!soNguoiInput.value || parseInt(soNguoiInput.value) === 0)) {
        soNguoiInput.value = 1;
    }



    //Hiển thị khu vực theo chi nhánh
    document.getElementById("selectChinhanh").addEventListener("change", function () {
        var chinhanhId = this.value;
        fetch(`/Booking/GetKhuvucByChinhanh?idChinhanh=${chinhanhId}`)
            .then(response => response.json())
            .then(data => {
                const khuVucSelect = document.getElementById("selectKhuvuc");
                khuVucSelect.innerHTML = `<option value="">-- Chọn khu vực --</option>`;
                data.forEach(kv => {
                    khuVucSelect.innerHTML += `<option value="${kv}">${kv}</option>`;
                });


                // reset bàn
                document.getElementById("selectBan").innerHTML = `<option value="">-- Chọn bàn --</option>`;
            });
    });


    const selectedIdInput = document.getElementById("selectedIdban");
    if (selectedIdInput && selectedIdInput.value) {
        selectedBanId = selectedIdInput.value.trim();
        console.log("⭐ Gán selectedBanId lúc load DOM:", selectedBanId);
    }

    // Gọi setup sự kiện khu vực trước khi dispatch change
    setupKhuvucChangeEvent();

    // Nếu khu vực có giá trị => tự động gọi change để load bàn
    const khuVucSelect = document.getElementById("selectKhuvuc");
    if (khuVucSelect && khuVucSelect.value) {
        setTimeout(() => {
            khuVucSelect.dispatchEvent(new Event("change"));
        }, 100);
    }


    //Hiển thị số người đặt
    document.getElementById("Songuoidat").addEventListener("input", function () {
        const warning = document.getElementById("warning");
        warning.style.display = this.value > 10 ? "block" : "none";
        reloadBanList();
    });

    const agreeCheckbox = document.getElementById("agreeCheckbox");
    if (agreeCheckbox) {
        agreeCheckbox.addEventListener("change", function () {
            document.getElementById("confirmBtn").disabled = !this.checked;
        });
    }

    //Khai báo giờ và ngày
    const timeSelect = document.querySelector('select[name="Giobatdau"]');
    const ngayDatInput = document.querySelector('input[name="Ngaydat"]');
    //Hàm select giờ và ẩn giờ đã qua so với giờ thực
    function generateTimeOptions(currentTotalMins = null) {
        const start = 9 * 60; // 9:00
        const end = 21 * 60; // 21:00
        const step = 30;

        timeSelect.innerHTML = "";

        for (let mins = start; mins <= end; mins += step) {
            if (currentTotalMins !== null && mins <= currentTotalMins) continue;

            const hour = Math.floor(mins / 60);
            const minute = mins % 60;
            const hourStr = hour.toString().padStart(2, '0');
            const minuteStr = minute.toString().padStart(2, '0');
            const timeStr = `${hourStr}:${minuteStr}`;

            const option = document.createElement("option");
            option.value = timeStr;
            option.textContent = timeStr;
            timeSelect.appendChild(option);
        }

    }


    // Tự động set ngày hôm nay nếu chưa có
    if (ngayDatInput && (!ngayDatInput.value || ngayDatInput.value === "0001-01-01")) {
        const today = new Date().toISOString().split('T')[0];
        ngayDatInput.value = today;
    }

    // Lúc trang load, kiểm tra nếu ngày chọn là hôm nay
    const todayStr = new Date().toISOString().split('T')[0];
    if (ngayDatInput.value === todayStr) {
        const now = new Date();
        const currentMins = now.getHours() * 60 + now.getMinutes();
        generateTimeOptions(currentMins);
    } else {
        generateTimeOptions(); // ngày khác => đầy đủ
    }

    // Khi người dùng thay đổi ngày
    ngayDatInput.addEventListener("change", function () {
        const selectedDate = new Date(this.value);
        const today = new Date();
        if (selectedDate.toDateString() === today.toDateString()) {
            const now = new Date();
            const currentMins = now.getHours() * 60 + now.getMinutes();
            generateTimeOptions(currentMins);
        } else {
            generateTimeOptions();
        }
        reloadBanList();
    });

    document.querySelector('select[name="Giobatdau"]').addEventListener("change", reloadBanList);

    [
        'input[name="HoTenNguoiDung"]',
        'input[name="EmailNguoiDung"]',
        'input[name="SdtNguoiDung"]',
        'input[name="Songuoidat"]',
        'input[name="Ngaydat"]',
        'select[name="Giobatdau"]',
        '#selectChinhanh',
        '#selectKhuvuc'
    ].forEach(selector => {
        const el = document.querySelector(selector);
        if (el) {
            el.addEventListener("change", checkFormAndStartCountdown);
            el.addEventListener("input", checkFormAndStartCountdown);
        }
    });
    checkFormAndStartCountdown();

});

//Hàm setup bàn theo khu vực và lấy thông tin bàn đã đặt
//function setupKhuvucChangeEvent() {
//    document.getElementById("selectKhuvuc").addEventListener("change", function () {
//        var khuvuc = this.value;
//        var chinhanhId = document.getElementById("selectChinhanh").value;
//        var soNguoiDat = parseInt(document.getElementById("Songuoidat").value) || 1;
//        var selectedNgay = document.querySelector('input[name="Ngaydat"]').value;
//        var selectedGio = document.querySelector('select[name="Giobatdau"]').value;

//        const bgSrc = backgroundImages[khuvuc];
//        if (bgSrc) {
//            backgroundImg.src = bgSrc;
//            backgroundImg.onload = () => {
//                canvas.width = canvas.offsetWidth;
//                canvas.height = canvas.offsetHeight;
//                drawCanvas(banListGlobal); // vẽ nền mới
//            };
//        }
//        // Khi ảnh load xong sẽ vẽ canvas     
//        fetch(`/Booking/GetBanByKhuvuc?idChinhanh=${chinhanhId}&khuvuc=${khuvuc}`)
//            .then(response => response.json())
//            .then(banList => {
//                fetch(`/Booking/GetBanDaDat?ngay=${selectedNgay}&gio=${selectedGio}&idChinhanh=${chinhanhId}&idKhuvuc=${khuvuc}`)
//                    .then(res => res.json())
//                    .then(banDaDatList => {
//                        banList.forEach(b => b.songuoi = parseInt(b.songuoi));
//                        console.log("Dữ liệu bàn:", banList);
//                        console.log("Danh sách bàn đã đặt:", banDaDatList);
//                        console.log("Danh sách bàn đã đặt1:", banDaDatList.map(b => b.idban));
//                        console.log("Danh sách bàn hiện tại:", banList.map(b => b.idban));

//                        console.log("🎯 Gửi truy vấn GetBanDaDat với:", {
//                            ngay: selectedNgay,
//                            gio: selectedGio,
//                            chinhanh: chinhanhId,
//                            khuvuc: khuvuc
//                        });

//                        initBanList(banList, selectedNgay, selectedGio, chinhanhId, khuvuc, soNguoiDat, banDaDatList);
//                        // Gán lại selectedBanId từ hidden input
//                        const selectedIdInput = document.getElementById("selectedIdban");
//                        if (selectedIdInput && selectedIdInput.value) {
//                            selectedBanId = selectedIdInput.value.trim();
//                            console.log("⭐ Gán lại selectedBanId sau fetch:", selectedBanId);

//                            // 👉 Tìm bàn tương ứng và cập nhật text
//                            const selectedBan = banList.find(b => b.idban === selectedBanId);
//                            if (selectedBan) {
//                                document.getElementById("banInfo").innerText = "Đã chọn: " + selectedBan.tenban;
//                            }
//                        }

//                        // ✅ Vẽ canvas lại
//                        if (backgroundImg.complete) {
//                            drawCanvas(banListGlobal, selectedBanId);
//                        } else {
//                            backgroundImg.onload = () => drawCanvas(banListGlobal, selectedBanId);
//                        }
//                        document.getElementById("canvas").style.display = "block";


//                    });
//            });

//    });
//}
function setupKhuvucChangeEvent() {
    document.getElementById("selectKhuvuc").addEventListener("change", function () {
        var khuvuc = this.value;
        var chinhanhId = document.getElementById("selectChinhanh").value;
        var soNguoiDat = parseInt(document.getElementById("Songuoidat").value) || 1;
        var selectedNgay = document.querySelector('input[name="Ngaydat"]').value;
        var selectedGio = document.querySelector('select[name="Giobatdau"]').value;

        const bgSrc = backgroundImages[khuvuc];
        if (bgSrc) {
            backgroundImg.src = bgSrc;
            backgroundImg.onload = () => {
                canvas.width = canvas.offsetWidth;
                canvas.height = canvas.offsetHeight;
                drawCanvas(banListGlobal);
            };
        }

        const urlGetBan = `/Booking/GetBanByKhuvuc?idChinhanh=${chinhanhId}&khuvuc=${khuvuc}`;
        const urlDaDat = `/Booking/GetBanDaDat?ngay=${selectedNgay}&gio=${selectedGio}&idChinhanh=${chinhanhId}&idKhuvuc=${khuvuc}`;
        const urlLock = `/Booking/GetBanLockTrongKhoang?idChinhanh=${chinhanhId}&idKhuvuc=${khuvuc}&ngay=${selectedNgay}&gio=${selectedGio}`;

        Promise.all([
            fetch(urlGetBan).then(res => res.json()),
            fetch(urlDaDat).then(res => res.json()),
            fetch(urlLock).then(res => res.json())
        ])
            .then(([banList, banDaDatList, banLockList]) => {
                banList.forEach(b => {
                    b.songuoi = parseInt(b.songuoi) || 1;

                    // đánh dấu nếu bàn đã đặt
                    if (banDaDatList.some(d => d.idban === b.idban)) {
                        b.isDisabled = true;
                    }

                    // đánh dấu nếu bàn bị lock
                    if (banLockList.includes(b.idban)) {
                        b.isLocked = true;
                        //  b.isDisabled = true;
                    }
                });

                initBanList(banList, selectedNgay, selectedGio, chinhanhId, khuvuc, soNguoiDat, banDaDatList);

                const selectedIdInput = document.getElementById("selectedIdban");
                if (selectedIdInput && selectedIdInput.value) {
                    selectedBanId = selectedIdInput.value.trim();
                    const selectedBan = banList.find(b => b.idban === selectedBanId);
                    if (selectedBan) {
                        document.getElementById("banInfo").innerText = "Đã chọn: " + selectedBan.tenban;
                    }
                }

                if (backgroundImg.complete) {
                    drawCanvas(banListGlobal = banList, selectedBanId);
                } else {
                    backgroundImg.onload = () => drawCanvas(banListGlobal = banList, selectedBanId);
                }

                document.getElementById("canvas").style.display = "block";
            });
    });
}


const backgroundImages = {
    "Ngoài trời": "/css/img/anhinh.jpg",
    "Trong nhà": "/css/img/trongnha.jpg",
    "Sân thượng": "/css/img/anhsanthuong.jpg"
};
//Hàm để hiển thị bàn 
const canvas = document.getElementById("canvas");
const ctx = canvas.getContext("2d");
const backgroundImg = new Image();  // Ảnh nền canvas (theo khu vực)
const chairImg = new Image();       // Ảnh ghế
chairImg.src = "/css/img/ghe1.png";
const chairWithPersonImg = new Image();
chairWithPersonImg.src = "/css/img/connguoixanh.png";
let soNguoiDatGlobal = 1;
let banListGlobal = [];
let selectedBanId = null;

function drawRotatedImage(ctx, image, x, y, angle, width, height) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angle);
    ctx.drawImage(image, -width / 2, -height / 2, width, height);
    ctx.restore();
}

function drawCanvas(banList, hoveredBan = null) {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    // Vẽ ảnh nền trước nếu đã load
    if (backgroundImg.complete) {
        ctx.drawImage(backgroundImg, 0, 0, canvas.width, canvas.height);
    }
    banList.forEach(ban => {
        ban.x = parseFloat(ban.x) || 0;
        ban.y = parseFloat(ban.y) || 0;
        ban.songuoi = parseInt(ban.songuoi) || 1;

        drawBan(ban, hoveredBan);
    });
}
function drawBan(ban, hoveredBan = null) {
    const { x = 0, y = 0, idban, tenban } = ban;
    const songuoi = parseInt(ban.songuoi);
    const isLocked = ban.isLocked || false;
    const isDisabled = ban.isDisabled || false;



    // Màu bàn
    //if (isDisabled) {
    //    ctx.fillStyle = "#a9a9a9"; // Xám
    //} else if (String(selectedBanId) === String(ban.idban)) {
    //    ctx.fillStyle = "#ffff99"; // Vàng nhạt
    //} else {
    //    ctx.fillStyle = "#ffffff"; // Trắng
    //}
    //if (String(selectedBanId) === String(ban.idban)) {
    //    ctx.fillStyle = "#ffff99"; // Vàng nhạt
    //} else if (isDisabled) {
    //    ctx.fillStyle = "#a9a9a9"; // Xám
    //} else {
    //    ctx.fillStyle = "#ffffff"; // Trắng
    //}
    const isSelected = String(selectedBanId) === String(ban.idban);

    let color = "#ffffff";

    switch (true) {
        case isLocked:
            color = "#dc3545"; // 🔴 Đỏ nếu bị lock
            break;
        case isSelected:
            color = "#ffff99"; // 💛 vàng nếu đang chọn
            break;
        case isDisabled:
            color = "#a9a9a9"; // ❌ xám nếu đã đặt
            break;
        default:
            color = "#ffffff"; // trắng
            break;
    }

    ctx.fillStyle = color;

    // Vẽ bàn tròn
    ctx.beginPath();
    ctx.arc(x, y, 30, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = "#333";
    ctx.lineWidth = 1;
    ctx.stroke();

    // Tên bàn
    ctx.fillStyle = "#000";
    ctx.font = "14px Arial";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(tenban, x, y);

    // Vẽ ghế
    const radius = 50;
    for (let i = 0; i < songuoi; i++) {
        const angle = (2 * Math.PI / songuoi) * i;
        const gx = x + radius * Math.cos(angle);
        const gy = y + radius * Math.sin(angle);
        const deg = angle + Math.PI / 2;
        //drawRotatedImage(ctx, chairImg, gx, gy, deg, 40, 40);
        if (isDisabled) {
            drawRotatedImage(ctx, chairWithPersonImg, gx, gy, deg, 40, 40);
        } else {
            drawRotatedImage(ctx, chairImg, gx, gy, deg, 40, 40);
        }
    }

    // Tooltip khi hover
    if (hoveredBan && hoveredBan.idban === ban.idban) {
        drawTooltip(ctx, x, y - 60, "Đã đặt");
    }
}


//Hàm click chọn bàn đó
canvas.addEventListener("click", function (evt) {
    const rect = canvas.getBoundingClientRect();
    //const clickX = evt.clientX - rect.left;
    //const clickY = evt.clientY - rect.top;

    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    const clickX = (evt.clientX - rect.left) * scaleX;
    const clickY = (evt.clientY - rect.top) * scaleY;

    console.log("Bạn click tại:", clickX, clickY);
    console.log("Danh sách bàn hiện tại:", banListGlobal);

    for (let ban of banListGlobal) {
        const dx = clickX - ban.x;
        const dy = clickY - ban.y;
        const distance = Math.sqrt(dx * dx + dy * dy);

        console.log(`Kiểm tra bàn ${ban.idban} tại (${ban.x}, ${ban.y}) - khoảng cách: ${distance}`);

        if (distance < 40) {
            if (ban.isDisabled) {
                console.log("Bàn này bị disable nên không thể chọn:", ban.idban);
                return;
            }
            if (soNguoiDatGlobal > ban.songuoi) {
                alert("❗ Số người đặt bàn vượt quá sức chứa của bàn này. Vui lòng chọn bàn khác.");
                return;
            }
            selectedBanId = ban.idban;

            document.getElementById("selectedIdban").value = ban.idban;
            document.getElementById("banInfo").innerText = "Đã chọn: " + ban.tenban;

            console.log("Đã chọn bàn:", ban.tenban);

            drawCanvas(banListGlobal);
            checkFormAndStartCountdown();
            break;
        }
    }
});
canvas.addEventListener("mousemove", function (evt) {
    const rect = canvas.getBoundingClientRect();
    //const mouseX = evt.clientX - rect.left;
    //const mouseY = evt.clientY - rect.top;

    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    const mouseX = (evt.clientX - rect.left) * scaleX;
    const mouseY = (evt.clientY - rect.top) * scaleY;

    let hoveredBan = null;

    for (let ban of banListGlobal) {
        const dx = mouseX - ban.x;
        const dy = mouseY - ban.y;
        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance < 40 && ban.isDisabled) {
            hoveredBan = ban;
            break;
        }
    }

    drawCanvas(banListGlobal, hoveredBan);
});
function drawTooltip(ctx, x, y, text) {
    ctx.fillStyle = 'black';
    ctx.globalAlpha = 0.8;
    ctx.fillRect(x - 40, y - 25, 80, 25);
    ctx.globalAlpha = 1;

    ctx.fillStyle = 'white';
    ctx.font = '13px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(text, x, y - 10);

    // vẽ mũi nhọn nhỏ bên dưới label
    ctx.beginPath();
    ctx.moveTo(x - 5, y);
    ctx.lineTo(x + 5, y);
    ctx.lineTo(x, y + 6);
    ctx.closePath();
    ctx.fillStyle = 'black';
    ctx.fill();
}

let iddatbanCurrent = null;

const idInput = document.getElementById("iddatbanCurrent");
if (idInput && idInput.value) {
    iddatbanCurrent = idInput.value.trim();
}
let originalNgay = document.querySelector('input[name="Ngaydat"]').value;
let originalGio = document.querySelector('select[name="Giobatdau"]').value;


//Xử lý cách thông tin nhập vào so sánh với dữ liệu và để hiển thị màu của bàn cho đúng
function initBanList(banList, selectedNgay, selectedGio, selectedChinhanh, selectedKhuvuc, soNguoiDat, banDaDatList) {
    const selectedGioStart = selectedGio;
    const selectedGioEnd = addHoursToTime(selectedGioStart, 2);
    soNguoiDatGlobal = soNguoiDat;


    banListGlobal = banList.map(ban => {
        //const isDisabled = banDaDatList.some(dadat => {
        //    const sameId = dadat.idban === ban.idban;
        //    const sameDate = dadat.ngay === selectedNgay;
        //    const sameBranch = dadat.idchinhanh === selectedChinhanh;
        //    const sameArea = dadat.idkhuvuc === selectedKhuvuc;

        //    const selectedStart = timeToMinutes(selectedGioStart);
        //    const selectedEnd = timeToMinutes(selectedGioEnd);
        //    const dadatStart = timeToMinutes(dadat.gio);
        //    const dadatEnd = dadatStart + 120;

        //    const isTimeOverlap =
        //        (selectedStart >= dadatStart && selectedStart < dadatEnd) ||
        //        (selectedEnd > dadatStart && selectedEnd <= dadatEnd) ||
        //        (selectedStart <= dadatStart && selectedEnd >= dadatEnd);
        //    const trangThaiHopLe = dadat.Trangthaidatban != "Đã hủy";
        //    const result = sameId && sameDate && sameBranch && sameArea && isTimeOverlap && trangThaiHopLe;

        //    if (result) {
        //        console.log(`⛔ Bàn ${ban.idban} bị disable do trùng lịch:`, { selectedStart, selectedEnd, dadatStart, dadatEnd, trangThaiHopLe });
        //    }

        //    return result;
        //});
        const isChangedDateOrTime = (selectedNgay !== originalNgay) || (selectedGioStart !== originalGio);
        //const isDisabled = banDaDatList.some(dadat => {
        //    const sameId = dadat.idban === ban.idban;
        //    const sameDate = dadat.ngay === selectedNgay;
        //    const sameBranch = dadat.idchinhanh === selectedChinhanh;
        //    const sameArea = dadat.idkhuvuc === selectedKhuvuc;

        //    const selectedStart = timeToMinutes(selectedGioStart);
        //    const selectedEnd = timeToMinutes(selectedGioEnd);
        //    const dadatStart = timeToMinutes(dadat.gio);
        //    const dadatEnd = dadatStart + 120;

        //    const isTimeOverlap =
        //        (selectedStart >= dadatStart && selectedStart < dadatEnd) ||
        //        (selectedEnd > dadatStart && selectedEnd <= dadatEnd) ||
        //        (selectedStart <= dadatStart && selectedEnd >= dadatEnd);

        //    const trangThaiHopLe = dadat.Trangthaidatban !== "Đã hủy";
        //    const isCurrentBooking = dadat.iddatban === iddatbanCurrent;
        //    if (!isChangedDateOrTime && dadat.iddatban === iddatbanCurrent) return false;

        //    return sameId && sameDate && sameBranch && sameArea && isTimeOverlap && trangThaiHopLe && !isCurrentBooking;
        //});
        const isDisabled = banDaDatList.some(dadat => {
            const sameId = dadat.idban === ban.idban;
            const sameDate = dadat.ngay === selectedNgay;
            const sameBranch = dadat.idchinhanh === selectedChinhanh;
            const sameArea = dadat.idkhuvuc === selectedKhuvuc;

            const selectedStart = timeToMinutes(selectedGioStart);
            const selectedEnd = timeToMinutes(selectedGioEnd);
            const dadatStart = timeToMinutes(dadat.gio);
            const dadatEnd = dadatStart + 120;

            const isTimeOverlap =
                (selectedStart >= dadatStart && selectedStart < dadatEnd) ||
                (selectedEnd > dadatStart && selectedEnd <= dadatEnd) ||
                (selectedStart <= dadatStart && selectedEnd >= dadatEnd);

            const trangThaiHopLe = dadat.Trangthaidatban !== "Đã hủy";
            const isCurrentBooking = dadat.iddatban === iddatbanCurrent;

            // 🟢 Nếu ngày giờ chưa đổi → không disable bàn của mình
            if (!isChangedDateOrTime && isCurrentBooking) return false;

            return sameId && sameDate && sameBranch && sameArea && isTimeOverlap && trangThaiHopLe;
        });


        console.log("✅ Xử lý bàn:", ban.idban, "→ isDisabled:", isDisabled);

        return {
            ...ban,
            x: parseFloat(ban.x),
            y: parseFloat(ban.y),
            songuoi: parseInt(ban.songuoi),
            isDisabled: isDisabled
        };
    });
    // selectedBanId = document.getElementById("selectedIdban").value.trim();

    // Gán lại selectedBanId từ hidden input
    //const selectedIdInput = document.getElementById("selectedIdban");
    //if (selectedIdInput && selectedIdInput.value) {
    //    selectedBanId = selectedIdInput.value.trim();
    //    console.log("⭐ Gán lại selectedBanId sau fetch:", selectedBanId);

    //    const selectedBan = banListGlobal.find(b => String(b.idban) === String(selectedBanId));

    //    if (selectedBan) {
    //        document.getElementById("banInfo").innerText = "Đã chọn: " + selectedBan.tenban;
    //    } else {
    //        document.getElementById("banInfo").innerText = "Chưa chọn bàn";
    //    }

    //}
    //const selectedIdInput = document.getElementById("selectedIdban");
    //if (selectedIdInput && selectedIdInput.value) {
    //    selectedBanId = selectedIdInput.value.trim();
    //    console.log("⭐ Gán lại selectedBanId sau fetch:", selectedBanId);

    //    // 🟢 Luôn lấy từ banList (đang truyền vào initBanList), không lấy từ banListGlobal
    //    const selectedBan = banList.find(b => String(b.idban).trim() === String(selectedBanId).trim());

    //    if (selectedBan && !selectedBan.isDisabled) {
    //        document.getElementById("banInfo").innerText = "Đã chọn: " + selectedBan.tenban;
    //    } else {
    //        selectedBanId = null;
    //        selectedIdInput.value = "";
    //        document.getElementById("banInfo").innerText = "Chưa chọn bàn";

    //        if (selectedBan && selectedBan.isDisabled) {
    //            alert("Bàn bạn đã chọn trước đó đã có người đặt vào khung giờ bạn chọn, vui lòng chọn bàn khác.");
    //        }
    //    }
    //}
    const selectedIdInput = document.getElementById("selectedIdban");
    if (selectedIdInput && selectedIdInput.value) {
        let tempSelectedId = selectedIdInput.value.trim();
        console.log("⭐ Gán lại selectedBanId sau fetch:", tempSelectedId);

        const selectedBan = banList.find(b => String(b.idban).trim() === String(tempSelectedId).trim());

        if (selectedBan && !selectedBan.isDisabled) {
            selectedBanId = tempSelectedId;
            document.getElementById("banInfo").innerText = "Đã chọn: " + selectedBan.tenban;
        } else {
            selectedBanId = null;
            selectedIdInput.value = "";
            document.getElementById("banInfo").innerText = "Chưa chọn bàn";

            if (isChangedDateOrTime && selectedBan && selectedBan.isDisabled) {
                alert("Bàn bạn đã chọn trước đó đã có người đặt vào khung giờ mới, vui lòng chọn bàn khác.");
            }
        }
    }




    if (backgroundImg.complete) {
        canvas.width = canvas.offsetWidth;
        canvas.height = canvas.offsetHeight;
        drawCanvas(banListGlobal);
    } else {
        backgroundImg.onload = () => {
            canvas.width = canvas.offsetWidth;
            canvas.height = canvas.offsetHeight;
            drawCanvas(banListGlobal)
        };
    }
}

function timeToMinutes(timeStr) {
    const [h, m] = timeStr.split(":").map(Number);
    return h * 60 + m;
}


function addHoursToTime(timeStr, hoursToAdd) {
    const [h, m] = timeStr.split(":").map(Number);
    let totalMinutes = h * 60 + m + hoursToAdd * 60;

    const maxMinutes = 23 * 60 + 59; // 23:59
    if (totalMinutes > maxMinutes) {
        totalMinutes = maxMinutes;
    }

    const newH = Math.floor(totalMinutes / 60);
    const newM = totalMinutes % 60;
    return `${newH.toString().padStart(2, "0")}:${newM.toString().padStart(2, "0")}`;
}

let countdownInterval;
let countdownTime = 5 * 60; // 5 phút = 300 giây
let isCountdownStarted = false;


//Hàm để mở modal dialog đặt bàn
function openConfirmation() {
    console.log("Hàm openConfirmation được gọi");

    // Lấy phần tử input
    const tenNguoiDatInput = document.querySelector('input[name="HoTenNguoiDung"]');
    const emailInput = document.querySelector('input[name="EmailNguoiDung"]');
    const sdtInput = document.querySelector('input[name="SdtNguoiDung"]');
    const soNguoiInput = document.querySelector('input[name="Songuoidat"]');
    const ngayDatInput = document.querySelector('input[name="Ngaydat"]');
    const gioBatDauSelect = document.querySelector('select[name="Giobatdau"]');

    const chiNhanh = document.getElementById("selectChinhanh");
    const khuVuc = document.getElementById("selectKhuvuc");
    const ban = document.getElementById("selectedIdban");

    // Kiểm tra null
    if (!tenNguoiDatInput || !emailInput || !sdtInput || !soNguoiInput || !ngayDatInput || !gioBatDauSelect) {
        alert("Thiếu thông tin trong form. Vui lòng kiểm tra lại.");
        return;
    }

    const tenNguoiDat = tenNguoiDatInput.value.trim();
    const email = emailInput.value.trim();
    const sdt = sdtInput.value.trim();
    const soNguoi = soNguoiInput.value;
    const ngayDat = ngayDatInput.value;
    const gioBatDau = gioBatDauSelect.value;

    // Kiểm tra giá trị rỗng
    if (!tenNguoiDat || !email || !sdt || !soNguoi || !ngayDat || !gioBatDau) {
        alert("Vui lòng điền đầy đủ thông tin trước khi đặt bàn.");
        return;
    }

    if (!chiNhanh || !chiNhanh.value) {
        alert("Vui lòng chọn chi nhánh.");
        return;
    }

    if (!khuVuc || !khuVuc.value) {
        alert("Vui lòng chọn khu vực sau khi chọn chi nhánh.");
        return;
    }

    if (!ban || !ban.value) {
        alert("Vui lòng chọn bàn sau khi chọn khu vực.");
        return;
    }

    // Reset checkbox và nút confirm
    const checkbox = document.getElementById("agreeCheckbox");
    if (checkbox) checkbox.checked = false;

    const confirmBtn = document.getElementById("confirmBtn");
    if (confirmBtn) confirmBtn.disabled = true;

    // Mở modal xác nhận

    const modal = document.getElementById("confirmModal");
    if (modal) {
        //  BẮT ĐẦU ĐẾM GIỜ 5 PHÚT NẾU CHƯA BẮT ĐẦU
        if (!isCountdownStarted) {
            startCountdownTimer();
            isCountdownStarted = true;
        }

        modal.style.display = "block";
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            const modalInstance = new bootstrap.Modal(modal);
            modalInstance.show();
        } else {
            $('#confirmModal').modal('show');
        }
    }
}
function checkFormAndStartCountdown() {
    if (isCountdownStarted) return;

    // Kiểm tra xem có đang ở chế độ Đặt bàn mới không
    const datbanBtn = document.querySelector('button[onclick="openConfirmation()"]');
    if (!datbanBtn) return; // Nếu không tồn tại nút đặt bàn (tức đang ở chế độ cập nhật) thì không chạy

    const ten = document.querySelector('input[name="HoTenNguoiDung"]');
    const email = document.querySelector('input[name="EmailNguoiDung"]');
    const sdt = document.querySelector('input[name="SdtNguoiDung"]');
    const songuoi = document.querySelector('input[name="Songuoidat"]');
    const ngay = document.querySelector('input[name="Ngaydat"]');
    const gio = document.querySelector('select[name="Giobatdau"]');
    const chinhanh = document.getElementById("selectChinhanh");
    const khuvuc = document.getElementById("selectKhuvuc");
    const idban = document.getElementById("selectedIdban");

    const allFilled = ten?.value && email?.value && sdt?.value && songuoi?.value
        && ngay?.value && gio?.value && chinhanh?.value && khuvuc?.value && idban?.value;

    if (allFilled) {
        isCountdownStarted = true;
        startCountdownTimer();
        console.log("✅ Tự động bắt đầu đếm giờ vì đủ điều kiện!");
    }
}

function startCountdownTimer() {
    clearInterval(countdownInterval); // Xoá nếu đã chạy rồi

    let timeLeft = countdownTime;
    const datbanBtn = document.querySelector('button[onclick="openConfirmation()"]');

    countdownInterval = setInterval(() => {
        const minutes = Math.floor(timeLeft / 60).toString().padStart(2, '0');
        const seconds = (timeLeft % 60).toString().padStart(2, '0');

        datbanBtn.innerHTML = `Đặt bàn (<span style="color:red">${minutes}:${seconds}s</span>)`;

        timeLeft--;

        if (timeLeft < 0) {
            clearInterval(countdownInterval);
            alert("Bạn đã hết thời gian giữ bàn. Vui lòng chọn lại.");
            location.reload(); // Reload lại trang
        }
    }, 1000);
}
function closeModal() {
    const modal = document.getElementById("confirmModal");
    if (modal) {
        modal.style.display = "none";

        // Nếu dùng Bootstrap 5
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) modalInstance.hide();
        } else {
            $('#confirmModal').modal('hide');
        }
    }
}

//Hàm nhấn nút đồng ý đặt bàn và đóng modal
function submitBooking() {
    const checkbox = document.getElementById("agreeCheckbox");
    if (!checkbox || !checkbox.checked) {
        alert("Bạn phải đồng ý với điều khoản trước.");
        return;
    }

    // Ngưng đếm giờ và đóng modal
    clearInterval(countdownInterval);
    closeModal();

    // Gọi đúng form bằng ID
    const form = document.getElementById("bookingForm");
    if (form) {
        form.submit();
    } else {
        console.error("Không tìm thấy form bookingForm để submit.");
    }
}

//Hàm để k phải loading mới nhập dữ liệu mới , hàm này giúp để thay đổi dữ liệu liên tục k cần loading
function reloadBanList() {
    var khuvuc = document.getElementById("selectKhuvuc").value;
    var chinhanhId = document.getElementById("selectChinhanh").value;
    var soNguoiDat = parseInt(document.getElementById("Songuoidat").value) || 1;
    var selectedNgay = document.querySelector('input[name="Ngaydat"]').value;
    var selectedGio = document.querySelector('select[name="Giobatdau"]').value;

    if (!khuvuc || !chinhanhId || !selectedNgay || !selectedGio) return;

    fetch(`/Booking/GetBanByKhuvuc?idChinhanh=${chinhanhId}&khuvuc=${khuvuc}`)
        .then(response => response.json())
        .then(banList => {
            fetch(`/Booking/GetBanDaDat?ngay=${selectedNgay}&gio=${selectedGio}&idChinhanh=${chinhanhId}&idKhuvuc=${khuvuc}`)
                .then(res => res.json())
                .then(banDaDatList => {
                    banList.forEach(b => b.songuoi = parseInt(b.songuoi));
                    initBanList(banList, selectedNgay, selectedGio, chinhanhId, khuvuc, soNguoiDat, banDaDatList);
                    canvas.width = canvas.offsetWidth;
                    canvas.height = canvas.offsetHeight;
                    drawCanvas(banListGlobal);
                });
        });
}
