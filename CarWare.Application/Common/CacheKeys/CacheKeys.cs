namespace CarWare.Application.Common.Cache
{
    public static class CacheKeys
    {
        // Email Verification
        public static string EmailVerifyOtp(string otp)
            => $"email_verify_otp:{otp}";

        public static string EmailVerifyOtpAttempts(string userId)
            => $"email_verify_otp_attempts:{userId}";

        // Password Reset OTP
        public static string ResetOtp(string otp)
            => $"reset_otp:{otp}";

        public static string ResetOtpAttempts(string userId)
            => $"reset_otp_attempts:{userId}";

        // Reset Handle
        public static string ResetHandle(string handle)
            => $"reset_handle:{handle}";

        public static string ResetHandleUserId(string handle)
            => $"reset_handle_userId:{handle}";
    }
}