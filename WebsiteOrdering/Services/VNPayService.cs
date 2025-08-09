using WebsiteOrdering.Models;
using WebsiteOrdering.Utils;

namespace WebsiteOrdering.Services
{
    public class VNPayService
    {
        private readonly IConfiguration _config;

        public VNPayService(IConfiguration config)
        {
            _config = config;
        }
        public string CreatePaymentUrl(HttpContext context, VNPayRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", _config["VNPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VNPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VNPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); // VNPay yêu cầu số tiền * 100
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VNPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", VNPayUtils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _config["VNPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", model.Description);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Loại hàng hóa
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VNPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", tick); // Mã giao dịch
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            var paymentUrl = vnpay.CreateRequestUrl(_config["VNPay:BaseUrl"], _config["VNPay:HashSecret"]);
            return paymentUrl;
        }
        public VNPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VNPayLibrary();
            foreach (string s in collections.Keys)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(s, collections[s]);
                }
            }

            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VNPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VNPayResponseModel
                {
                    Success = false
                };
            }

            return new VNPayResponseModel
            {
                Success = vnp_ResponseCode == "00",
                PaymentMethod = "VNPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
