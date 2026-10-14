using System.ComponentModel.DataAnnotations;

namespace GymBoom.Models.ViewModels.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifrenizi girmeniz zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre belirlemeniz zorunludur.")]
    [StringLength(50, ErrorMessage = "Şifreniz en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    [Compare("NewPassword", ErrorMessage = "Yeni şifre ile şifre tekrarı birbiriyle eşleşmiyor.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
} 