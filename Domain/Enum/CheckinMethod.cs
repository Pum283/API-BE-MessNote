namespace Domain.Enums
{
    /// <summary>
    /// Phương thức điểm danh
    /// </summary>
    public enum CheckinMethod
    {
        Link = 1,            // Click link xác nhận
        SMSReply = 2,        // Reply SMS với từ khóa
        EmailButton = 3,     // Nhấn nút trong email
        AppPush = 4          // Push notification trên app
    }
}

