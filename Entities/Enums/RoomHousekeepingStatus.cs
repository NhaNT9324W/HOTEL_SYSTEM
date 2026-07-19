namespace Hotel_System.Entities.Enums
{
    /**
     * [UC19.2 – Update Room Status] / [UC20 – Room Management & Inspection]
     * Danh mục liệt kê các trạng thái vệ sinh vật lý và mức độ sẵn sàng vận hành của một phòng khách sạn.
     * Chỉ số này phối hợp chặt chẽ với trạng thái kinh doanh (RoomBookingStatus) để tạo thành bộ điều kiện tiên quyết, 
     * giúp Lễ tân quyết định gán phòng đón khách an toàn tại quầy (UC14) và giúp bộ phận buồng phòng điều phối công việc hiệu quả (UC19).
     */
    public enum RoomHousekeepingStatus
    {
        /** 
         * Phòng bẩn hoặc chưa được vệ sinh.
         * Hệ thống sẽ tự động kích hoạt trạng thái này ngay khi Tiếp tân hoàn tất thủ tục trả phòng Check-out cho khách (UC18). 
         * Phòng ở trạng thái DIRTY sẽ lập tức được đẩy vào hàng đợi tác vụ để Quản lý tiến hành phân gán nhân viên buồng phòng đến dọn dẹp (UC19).
         */
        DIRTY,

        /** 
         * Phòng đã được Nhân viên buồng phòng thực hiện quét dọn, thay mới vật tư và làm sạch cơ bản (UC19.2).
         * Trạng thái này ghi nhận hoàn thành phần việc thực địa của Room Staff và là bước đệm chuyển giao để Trưởng bộ phận hoặc Giám sát viên tiến hành kiểm tra nghiệm thu chất lượng phòng.
         */
        CLEAN,

        /** 
         * Phòng sạch đạt chuẩn, đã qua nghiệm thu chất lượng hoàn hảo và hoàn toàn sẵn sàng đón khách mới.
         * Đây là trạng thái vệ sinh duy nhất đạt điều kiện cốt lõi để Tiếp tân gán phòng thực hiện thủ tục Check-in trực tiếp cho khách hàng (UC14). 
         * Mọi hành vi xếp khách vào phòng chưa đạt trạng thái READY sẽ bị hệ thống ngăn chặn để bảo đảm trải nghiệm dịch vụ.
         */
        READY
    }
}