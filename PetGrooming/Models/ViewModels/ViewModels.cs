using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PetGroomingSystem.Models.ViewModels
{
    // View Models ----------------------------------------------------------------

#nullable disable warnings

    public class LoginVM
    {
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }


    public class RegisterVM
    {
        [StringLength(100)]
        [EmailAddress]
        [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(100, MinimumLength = 5)]
        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string Confirm { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        // 上传文件用
        public IFormFile Photo { get; set; }
    }



    public class UpdatePasswordVM
    {
        [Required(ErrorMessage = "Current password is required.")]
        [StringLength(100, MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string Current { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string New { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [StringLength(100, MinimumLength = 5)]
        [Compare("New", ErrorMessage = "New password and confirmation do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string Confirm { get; set; }
    }

    public class UpdateProfileVM
    {
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // 用于接收裁剪后的 Base64 图片
        public string? Photo { get; set; }
        public string? PhotoURL { get; set; }

        // 修改密码相关
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "New password must be at least 5 characters.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        // 其他资料
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
    }





    public class ResetPasswordVM
    {
        [Required]
        public string Token { get; set; }  // 邮件链接里的 token

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be at least 5 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }


    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
    }


    //public class ProfileVM
    //{
    //    [Required, MaxLength(100)]
    //    public string Name { get; set; }

    //    [Required, EmailAddress]
    //    public string Email { get; set; }

    //    [DataType(DataType.Password)]
    //    public string? Password { get; set; }   // 允许为空，不改就不填

    //    [DataType(DataType.Password)]
    //    [Compare("Password", ErrorMessage = "密码不一致")]
    //    public string? Confirm { get; set; }

    //    public IFormFile? Photo { get; set; }

    //    public string? ExistingPhoto { get; set; } // 显示当前照片用

    //    public string? Address { get; set; }
    //    public DateTime? DateOfBirth { get; set; }
    //    public int? Age { get; set; }
    //}
}