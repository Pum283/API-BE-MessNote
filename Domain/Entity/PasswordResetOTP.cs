namespace Domain.Entity
{
    /// <summary>
    /// Mã OTP để reset mật khẩu
    /// </summary>
    public class PasswordResetOTP : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Email của người dùng yêu cầu reset mật khẩu
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mã OTP (6 chữ số)
        /// </summary>
        public string OTPCode { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian hết hạn của OTP (mặc định 10 phút)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Đã sử dụng OTP này chưa
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Số lần đã thử nhập sai OTP
        /// </summary>
        public int AttemptCount { get; set; } = 0;
    }
}

