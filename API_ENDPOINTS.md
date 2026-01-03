# MessNote API - Danh sách Endpoints

## Base URL
`/api`

## 1. Users Controller (`/api/Users`)
- `GET /api/Users` - Lấy danh sách tất cả người dùng
- `GET /api/Users/{id}` - Lấy thông tin người dùng theo ID
- `POST /api/Users` - Tạo người dùng mới
- `PUT /api/Users/{id}` - Cập nhật thông tin người dùng
- `DELETE /api/Users/{id}` - Xóa người dùng

## 2. Campaigns Controller (`/api/Campaigns`)
- `GET /api/Campaigns` - Lấy danh sách tất cả chiến dịch
- `GET /api/Campaigns/{id}` - Lấy thông tin chiến dịch theo ID
- `POST /api/Campaigns` - Tạo chiến dịch mới
- `PUT /api/Campaigns/{id}` - Cập nhật chiến dịch
- `DELETE /api/Campaigns/{id}` - Xóa chiến dịch

## 3. Recipients Controller (`/api/Recipients`)
- `GET /api/Recipients?campaignId={guid}` - Lấy danh sách người nhận (có thể lọc theo chiến dịch)
- `GET /api/Recipients/{id}` - Lấy thông tin người nhận theo ID
- `POST /api/Recipients` - Tạo người nhận mới
- `PUT /api/Recipients/{id}` - Cập nhật người nhận
- `DELETE /api/Recipients/{id}` - Xóa người nhận

## 4. MessageTemplates Controller (`/api/MessageTemplates`)
- `GET /api/MessageTemplates?campaignId={guid}` - Lấy danh sách mẫu tin nhắn (có thể lọc theo chiến dịch)
- `GET /api/MessageTemplates/{id}` - Lấy thông tin mẫu tin nhắn theo ID
- `POST /api/MessageTemplates` - Tạo mẫu tin nhắn mới
- `PUT /api/MessageTemplates/{id}` - Cập nhật mẫu tin nhắn
- `DELETE /api/MessageTemplates/{id}` - Xóa mẫu tin nhắn

## 5. Schedules Controller (`/api/Schedules`)
- `GET /api/Schedules?campaignId={guid}` - Lấy danh sách lịch trình (có thể lọc theo chiến dịch)
- `GET /api/Schedules/{id}` - Lấy thông tin lịch trình theo ID
- `POST /api/Schedules` - Tạo lịch trình mới
- `PUT /api/Schedules/{id}` - Cập nhật lịch trình
- `DELETE /api/Schedules/{id}` - Xóa lịch trình

## 6. Checkins Controller (`/api/Checkins`)
- `GET /api/Checkins?campaignId={guid}` - Lấy danh sách điểm danh (có thể lọc theo chiến dịch)
- `GET /api/Checkins/{id}` - Lấy thông tin điểm danh theo ID
- `POST /api/Checkins` - Tạo điểm danh mới (tự động cập nhật LastCheckinTime trong Schedule)
- `PUT /api/Checkins/{id}` - Cập nhật điểm danh
- `DELETE /api/Checkins/{id}` - Xóa điểm danh

## 7. Histories Controller (`/api/Histories`)
- `GET /api/Histories?campaignId={guid}&recipientId={guid}` - Lấy danh sách lịch sử (có thể lọc theo chiến dịch và người nhận)
- `GET /api/Histories/{id}` - Lấy thông tin lịch sử theo ID
- `POST /api/Histories` - Tạo bản ghi lịch sử mới
- `DELETE /api/Histories/{id}` - Xóa bản ghi lịch sử

## Enums

### CampaignStatus
- `1` = Active
- `2` = Paused
- `3` = Completed
- `4` = Deleted

### MessageChannel
- `1` = SMS
- `2` = Email
- `3` = Facebook
- `4` = All

### ScheduleType
- `1` = Fixed
- `2` = Checkin

### RecurringType
- `1` = None
- `2` = Daily
- `3` = Weekly
- `4` = Monthly

### CheckinMethod
- `1` = Link
- `2` = SMSReply
- `3` = EmailButton
- `4` = AppPush

### HistoryEventType
- `1` = SendMessage
- `2` = Checkin
- `3` = Overdue

### HistoryStatus
- `1` = Success
- `2` = Failed
- `3` = Pending

## Swagger UI
Khi chạy ứng dụng, truy cập `/swagger` để xem tài liệu API đầy đủ với Swagger UI.

