namespace Hotel_System.Entities.Enums
{
    /**
     * [UC18 – Manage Payments and Invoices] / [UC18.3 – Select Payment Method]
     * Danh mục liệt kê các phương thức thanh toán hợp pháp được hệ thống khách sạn chấp nhận khi khách hàng thực hiện quyết toán chi phí hóa đơn.
     * Chỉ số này tham gia trực tiếp vào quy trình xử lý giao dịch tài chính tại quầy lễ tân khi khách hàng làm thủ tục trả phòng (Check-out) hoặc thực hiện đặt cọc giữ chỗ.
     */
    public enum PaymentMethod
    {
        /** 
         * Phương thức thanh toán bằng tiền mặt trực tiếp tại quầy tiếp tân.
         * Giao dịch được xác nhận hoàn tất ngay khi Tiếp tân nhận đủ tiền mặt vật lý và ghi nhận vào két tiền mặt thuộc ca làm việc.
         */
        CASH,

        /** 
         * Thanh toán bằng thẻ ngân hàng (Thẻ tín dụng Credit hoặc Thẻ ghi nợ Debit) thông qua thiết bị POS tại quầy.
         * Yêu cầu Tiếp tân kiểm tra hóa đơn thanh toán thành công từ máy POS và lưu vết mã tham chiếu giao dịch ngân hàng lên hệ thống.
         */
        CARD,

        /** 
         * Thanh toán thông qua hình thức chuyển khoản ngân hàng trực tuyến (Ủy nhiệm chi hoặc quét mã VietQR).
         * Thường áp dụng cho các giao dịch đặt cọc từ xa hoặc khách hàng doanh nghiệp; đòi hỏi bộ phận Kế toán đối soát biến động số dư trước khi phê duyệt.
         */
        BANK_TRANSFER,

        /** 
         * Thanh toán qua các cổng ví điện tử liên kết tích hợp (ví dụ: MoMo, ZaloPay, VNPay).
         * Giao diện hệ thống sẽ tự động sinh mã QR động theo giá trị của từng hóa đơn để khách hàng quét nhanh, hỗ trợ đồng bộ trạng thái tức thời qua Webhook.
         */
        E_WALLET
    }
}