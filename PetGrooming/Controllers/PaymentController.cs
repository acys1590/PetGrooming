using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using QRCoder;

namespace PetGroomingSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DB _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(DB context, IConfiguration config, ILogger<PaymentController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // Show payment page
        public IActionResult Index(int serviceId, int appointmentId)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null) return NotFound("⚠️ Service not found.");

            ViewBag.ServiceId = service.Id;
            ViewBag.ServiceName = service.Name;
            ViewBag.Price = service.Price;
            ViewBag.AppointmentId = appointmentId;

            return View();
        }

        // Card payment page
        public IActionResult CardPayment(int serviceId, int appointmentId)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null) return NotFound("⚠️ Service not found.");

            // <-- fetch service for hidden inputs
            ViewBag.Service = service;
            ViewBag.AppointmentId = appointmentId;

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // Handle payment submission
        [HttpPost]
        public IActionResult Index(
        string paymentMethod,
        string? cardType,
        string? cardHolderName,
        string? cardNumber,
        string? expiryDate,
        string? cvv,
        string? cardPassword,
        string? phoneNumber,
        int serviceId,
        int appointmentId)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null) return NotFound("⚠️ Service not found.");

            if (paymentMethod == "Card")
            {
                // Save all previous input to TempData
                TempData["CardType"] = cardType;
                TempData["CardHolderName"] = cardHolderName;
                TempData["CardNumber"] = cardNumber;
                TempData["ExpiryDate"] = expiryDate;
                TempData["CVV"] = cvv;
                TempData["CardPassword"] = cardPassword;

                // Check required fields
                if (string.IsNullOrWhiteSpace(cardType) ||
                    string.IsNullOrWhiteSpace(cardHolderName) ||
                    string.IsNullOrWhiteSpace(cardNumber) ||
                    string.IsNullOrWhiteSpace(expiryDate) ||
                    string.IsNullOrWhiteSpace(cvv) ||
                    string.IsNullOrWhiteSpace(cardPassword))
                {
                    TempData["Error"] = "⚠️ Please fill in all card details.";
                    return RedirectToAction("CardPayment", new { serviceId, appointmentId });
                }

                // CVV must be exactly 3 digits
                if (!System.Text.RegularExpressions.Regex.IsMatch(cvv, @"^\d{3}$"))
                {
                    TempData["Error"] = "⚠️ CVV must be exactly 3 digits.";
                    return RedirectToAction("CardPayment", new { serviceId, appointmentId });
                }

                // Optionally, you can add more format checks for card number, expiry, etc.
                // But no further validation is required per your request.

                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();

                HttpContext.Session.SetString("PaymentOTP", otp);
                HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString());
                HttpContext.Session.SetInt32("AppointmentId", appointmentId);
                HttpContext.Session.SetInt32("ServiceId", serviceId);
                HttpContext.Session.SetString("CardHolder", cardHolderName);
                HttpContext.Session.SetString("CardMasked", "**** **** **** " + cardNumber[^4..]);
                HttpContext.Session.SetString("CardType", cardType);

                try
                {
                    SendEmail(
                        User.Identity?.Name ?? "hore-wm24@student.tac.edu.my",
                        "Payment Verification Code",
                        $"<h3>Your OTP Code is: <b>{otp}</b></h3><p>This code will expire in 5 minutes.</p>"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send OTP email.");
                    TempData["Error"] = "⚠️ Failed to send OTP. Please try again.";
                    return RedirectToAction("CardPayment", new { serviceId, appointmentId });
                }

                return RedirectToAction("VerifyOtp");
            }

            // TNG eWallet logic remains unchanged
            if (paymentMethod == "TNG")
            {
                var paymentLink = Url.Action("TngSuccess", "Payment", new { serviceId = service.Id, appointmentId }, Request.Scheme);

                using var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(paymentLink, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);
                var qrBytes = qrCode.GetGraphic(20);

                var qrBase64 = Convert.ToBase64String(qrBytes);

                ViewBag.QrCode = $"data:image/png;base64,{qrBase64}";
                ViewBag.PaymentLink = paymentLink;
                ViewBag.AppointmentId = appointmentId;
                ViewBag.Price = service.Price;
                ViewBag.Message = $"📱 Scan this QR code to complete RM{service.Price:0.00} payment.";

                return View("TngQr");
            }

            TempData["Error"] = "⚠️ Invalid payment method.";
            return RedirectToAction("CardPayment", new { serviceId, appointmentId });
        }

        // TNG QR Success
        public IActionResult TngSuccess(int serviceId, int appointmentId)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null) return NotFound("⚠️ Service not found.");

            var payment = new Payment
            {
                AppointmentId = appointmentId,
                ServiceId = serviceId,
                Amount = service.Price,
                Method = "TNG",
                CardHolderName = "TNG User",
                CardMasked = "TNG QR",
                Token = "tng_" + Guid.NewGuid().ToString("N"),
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            ViewBag.ServiceName = service.Name;
            ViewBag.Price = service.Price;
            ViewBag.AppointmentId = appointmentId;

            ViewBag.Message = $"✅ TNG payment of RM{service.Price:0.00} for {service.Name} was successful!";
            ViewBag.AppointmentId = appointmentId;
            return View("Success");
        }

        // OTP Verification
        public IActionResult VerifyOtp() => View();

        [HttpPost]
        public IActionResult VerifyOtp(string otpInput)
        {
            if (string.IsNullOrWhiteSpace(otpInput) || !System.Text.RegularExpressions.Regex.IsMatch(otpInput, @"^\d{6}$"))
            {
                ViewBag.Error = "⚠️ Invalid OTP format. Must be 6 digits.";
                return View();
            }

            var storedOtp = HttpContext.Session.GetString("PaymentOTP");
            var expiry = HttpContext.Session.GetString("OtpExpiry");
            var appointmentId = HttpContext.Session.GetInt32("AppointmentId") ?? 0;
            var serviceId = HttpContext.Session.GetInt32("ServiceId") ?? 0;
            var cardHolder = HttpContext.Session.GetString("CardHolder");
            var cardMasked = HttpContext.Session.GetString("CardMasked");
            var cardType = HttpContext.Session.GetString("CardType");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(expiry))
            {
                ViewBag.Error = "⚠️ OTP expired or invalid. Please restart payment.";
                return View();
            }

            if (DateTime.UtcNow > DateTime.Parse(expiry))
            {
                ViewBag.Error = "⚠️ OTP expired. Please request a new one.";
                return View();
            }

            if (otpInput != storedOtp)
            {
                ViewBag.Error = "❌ Invalid OTP. Please try again.";
                return View();
            }

            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null) return NotFound("⚠️ Service not found.");

            var payment = new Payment
            {
                AppointmentId = appointmentId,
                ServiceId = serviceId,
                Amount = service.Price,
                Method = cardType!,
                CardHolderName = cardHolder!,
                CardMasked = cardMasked!,
                Token = "otp_" + Guid.NewGuid().ToString("N"),
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            HttpContext.Session.Clear();

            ViewBag.Message = $"✅ {cardType} payment of RM{service.Price:0.00} for {service.Name} was successful!";
            ViewBag.AppointmentId = appointmentId;
            return View("Success");
        }

        // Resend OTP
        public IActionResult ResendOtp()
        {
            var serviceId = HttpContext.Session.GetInt32("ServiceId") ?? 0;
            var appointmentId = HttpContext.Session.GetInt32("AppointmentId") ?? 0;
            var cardHolder = HttpContext.Session.GetString("CardHolder");
            var cardType = HttpContext.Session.GetString("CardType");

            if (serviceId == 0 || appointmentId == 0 || string.IsNullOrEmpty(cardHolder))
            {
                ViewBag.Error = "⚠️ Session expired. Please restart the payment process.";
                return RedirectToAction("Index", new { serviceId, appointmentId });
            }

            var newOtp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("PaymentOTP", newOtp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddMinutes(5).ToString());

            try
            {
                SendEmail(
                    User.Identity?.Name ?? "hore-wm24@student.tac.edu.my",
                    "Payment Verification Code (Resent)",
                    $"<h3>Your new OTP Code is: <b>{newOtp}</b></h3><p>This code will expire in 5 minutes.</p>"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP email.");
                ViewBag.Error = "⚠️ Failed to resend OTP. Please try again.";
                return View("VerifyOtp");
            }

            ViewBag.Message = "✅ A new OTP has been sent to your email.";
            return View("VerifyOtp");
        }

        private IActionResult PaymentError(string msg, Service service, int appointmentId)
        {
            ViewBag.Message = msg;
            ViewBag.ServiceId = service.Id;
            ViewBag.ServiceName = service.Name;
            ViewBag.Price = service.Price;
            ViewBag.AppointmentId = appointmentId;
            return View("Index");
        }

        public IActionResult Success() => View();

        // Email Sender
        private void SendEmail(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Pet Grooming System", _config["SMTP:Sender"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(_config["SMTP:Host"], int.Parse(_config["SMTP:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_config["SMTP:Sender"], _config["SMTP:SecretKey"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
