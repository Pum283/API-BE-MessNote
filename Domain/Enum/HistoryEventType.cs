namespace Domain.Enums
{
    /// <summary>
    /// Loại sự kiện lịch sử
    /// </summary>
    public enum HistoryEventType
    {
        SendMessage = 1,     // Gửi tin nhắn
        Checkin = 2,         // Điểm danh
        Overdue = 3          // Quá hạn
    }
}

