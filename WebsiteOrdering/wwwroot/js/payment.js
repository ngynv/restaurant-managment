$(document).ready(function () {
    // Xử lý khi chọn phương thức thanh toán
    $('input[name="PaymentInfo"]').change(function () {
        var selectedPayment = $(this).val();
        if (selectedPayment === 'VNPay') {
            $('#vnpay-info').show();
        } else {
            $('#vnpay-info').hide();
        }
    });

    // Xử lý khi submit form thanh toán
    $('#checkout-form').submit(function (e) {
        var selectedPayment = $('input[name="PaymentInfo"]:checked').val();

        if (selectedPayment === 'VNPay') {
            // Hiển thị loading khi chuyển hướng đến VNPay
            $('#loading-modal').modal('show');
        }
    });
});