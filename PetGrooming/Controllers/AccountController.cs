using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace PetGroomingSystem.Controllers
{
    public class AccountsController : Controller
    {
        private readonly DB db;
        private readonly HelperBase hp;

        public object MailboxAddress { get; private set; }
        public object MimeKit { get; private set; }

        public AccountsController(DB db, HelperBase hp)
        {
            this.db = db;
            this.hp = hp;
        }

        #region Register
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterVM vm)
        {
            if (db.Users.Any(u => u.Email == vm.Email))
                ModelState.AddModelError("Email", "Email already registered");

            if (vm.Photo != null)
            {
                var err = hp.ValidatePhoto(vm.Photo);
                if (!string.IsNullOrEmpty(err))
                    ModelState.AddModelError("Photo", err);
            }

            // ===== 如果选择 Admin，需要验证专属密码 =====
            if (vm.Role == "Admin")
            {
                if (vm.AdminSecret != "ADMIN12345") // 固定密码
                {
                    ModelState.AddModelError("AdminSecret", "Invalid Admin Secret Password");
                }
            }

            if (ModelState.IsValid)
            {
                string? photoPath = vm.Photo != null ? hp.SavePhoto(vm.Photo, "photos") : null;

                var hasher = new PasswordHasher<User>();
                var user = new User
                {
                    Email = vm.Email,
                    Name = vm.Name,
                    PasswordHash = hasher.HashPassword(null!, vm.Password),
                    Role = vm.Role,  // 存 Admin 或 Member
                    PhotoPath = photoPath
                };
                db.Users.Add(user);

                // Member 额外存进 Members 表
                if (vm.Role == "Member")
                {
                    var member = new Member
                    {
                        Email = vm.Email,
                        Name = vm.Name,
                        PhotoURL = photoPath
                    };
                    db.Members.Add(member);
                }

                db.SaveChanges();
                TempData["Info"] = "Registration successful. Please login.";
                return RedirectToAction("Login");
            }

            return View(vm);
        }
        #endregion



        #region Login/Logout

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // ✅ 验证 fake "I'm not a robot"
            if (!vm.NotRobot)
            {
                ModelState.AddModelError("", "Please confirm you are not a robot.");
                return View(vm);
            }

            // 查找用户
            var user = db.Users.FirstOrDefault(x => x.Email == vm.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            // 验证密码
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(vm);
            }

            // 建立 Claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(claims, "Login");
            var principal = new ClaimsPrincipal(identity);

            // 登录
            await HttpContext.SignInAsync(principal);

            // 根据角色跳转
            switch (user.Role)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");   // 管理员
                case "Member":
                    return RedirectToAction("Index", "Member");  // 会员
                default:
                    return RedirectToAction("Index", "Home");    // 默认
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            TempData["Info"] = "You have been logged out.";
            return RedirectToAction("Login");
        }

        #endregion



        #region Update Profile / Password
        [Authorize(Roles = "Member")]
        public IActionResult UpdateProfile()
        {
            var m = db.Members.Find(User.Identity!.Name);
            if (m == null) return RedirectToAction("Index", "Main");

            return View(new UpdateProfileVM
            {
                Email = m.Email,
                Name = m.Name,
                PhotoURL = m.PhotoURL,
                Address = m.Address,
                DateOfBirth = m.DateOfBirth,
                Age = m.Age
            });
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult UpdateProfile(UpdateProfileVM vm)
        {
            var u = db.Users.Find(User.Identity!.Name);
            var m = db.Members.Find(User.Identity!.Name);
            if (u == null || m == null) return RedirectToAction("Index", "Main");

            if (ModelState.IsValid)
            {
                u.Name = vm.Name;
                m.Name = vm.Name;

                m.Address = vm.Address;
                m.DateOfBirth = vm.DateOfBirth;
                m.Age = vm.Age;

                // 更新头像
                if (vm.Photo != null)
                {
                    hp.DeletePhoto(u.PhotoPath, "photos");
                    hp.DeletePhoto(m.PhotoURL, "photos");

                    var newPhoto = hp.SavePhoto(vm.Photo, "photos");
                    u.PhotoPath = newPhoto;
                    m.PhotoURL = newPhoto;
                }

                // 修改密码（如果用户输入了）
                if (!string.IsNullOrEmpty(vm.CurrentPassword) && !string.IsNullOrEmpty(vm.NewPassword))
                {
                    var hasher = new PasswordHasher<User>();
                    if (hasher.VerifyHashedPassword(u, u.PasswordHash, vm.CurrentPassword) == PasswordVerificationResult.Success)
                    {
                        u.PasswordHash = hasher.HashPassword(u, vm.NewPassword);
                        TempData["Info"] = "Profile and password updated successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        return View(vm);
                    }
                }
                else
                {
                    TempData["Info"] = "Profile updated successfully.";
                }

                db.SaveChanges();
                return RedirectToAction("Index", "Main");
            }

            return View(vm);
        }

        [Authorize(Roles = "Member")]
        public IActionResult UpdatePassword() => View();

        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult UpdatePassword(UpdatePasswordVM vm)
        {
            var u = db.Users.FirstOrDefault(x => x.Email == User.Identity!.Name);
            if (u == null) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(vm);

            var hasher = new PasswordHasher<User>();
            if (string.IsNullOrEmpty(vm.Current) ||
                hasher.VerifyHashedPassword(u, u.PasswordHash, vm.Current) != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("Current", "Current password is incorrect.");
                return View(vm);
            }

            u.PasswordHash = hasher.HashPassword(u, vm.New);
            db.SaveChanges();
            TempData["Info"] = "Password updated successfully.";
            return RedirectToAction("UpdateProfile");
        }
        #endregion


        #region Forgot / Reset Password
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = db.Users.FirstOrDefault(u => u.Email == vm.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email not found");
                return View(vm);
            }

            // 生成 Token
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.Now.AddHours(1);
            db.SaveChanges();

            // 构建重置链接
            var resetLink = Url.Action("ResetPassword", "Accounts", new { token = user.ResetToken }, Request.Scheme);

            // 发送邮件
            SendEmail(user.Email, "Reset Your Password", $"Click this link to reset your password: <a href='{resetLink}'>Reset Password</a>");

            ViewBag.Message = "Reset link sent. Check your email.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired token.";
                return RedirectToAction("Login");
            }

            var vm = new ResetPasswordVM { Token = token };
            return View(vm);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = db.Users.FirstOrDefault(u => u.ResetToken == vm.Token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired token.";
                return RedirectToAction("Login");
            }

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            db.SaveChanges();

            TempData["Info"] = "Password has been reset successfully.";
            return RedirectToAction("Login");
        }
        #endregion

        #region Email Sending
        #endregion
        private void SendEmail(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();

            // 发件人
            email.From.Add(new MailboxAddress("Pet Grooming System", "devoneu061030@gmail.com"));

            // 收件人
            email.To.Add(new MailboxAddress("", toEmail));

            email.Subject = subject;

            // 这里必须用 MimeKit.TextPart
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlBody
            };

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("devoneyc-wm24@student.tarc.edu.my", "pllrjpturrpzhqjj");//这个要小心不能泄露 不然别人能用我email发信息
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }
    }
}
//Install - Package MailKit 你们需要下载
//Install - Package MimeKit

