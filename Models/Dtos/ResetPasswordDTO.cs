namespace LibraryAPI.Models.Dtos
{
    public class ResetPasswordDTO
    {
        public string? Email { get; set; }
        public string? ResetPasswordToken { get; set; } 
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
