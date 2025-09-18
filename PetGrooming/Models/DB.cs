using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetGrooming.Models;

// DbContext class
public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    // Define database tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;   // NEW
    public DbSet<Payment> Payments { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.AppointmentDate).HasDefaultValueSql("GETDATE()");

            // Doctor relationship
            entity.HasOne(p => p.Doctor)
                  .WithMany(d => d.Pets)
                  .HasForeignKey(p => p.DoctorId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Staff relationship
            entity.HasOne(p => p.Staff)
                  .WithMany(s => s.Pets)
                  .HasForeignKey(p => p.StaffId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Service relationship
            entity.HasOne(p => p.Service)
                  .WithMany()
                  .HasForeignKey(p => p.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Doctor entity
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.JoinDate).HasDefaultValueSql("GETDATE()");
            entity.Property(d => d.IsActive).HasDefaultValue(true);
        });

        // Configure Member entity
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Email); // Using Email as primary key
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Email); // Using Email as primary key
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Doctors
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = 1,
                Name = "Dr. Sarah Johnson",
                Specialization = "Small Animals",
                Phone = "123-456-7890",
                Email = "sarah.johnson@petcare.com",
                Address = "123 Main St, City",
                JoinDate = new DateTime(2020, 1, 15),
                IsActive = true,
                Notes = "Specializes in cats and small dogs"
            },
            new Doctor
            {
                Id = 2,
                Name = "Dr. Michael Chen",
                Specialization = "Large Animals",
                Phone = "123-456-7891",
                Email = "michael.chen@petcare.com",
                Address = "456 Oak Ave, City",
                JoinDate = new DateTime(2019, 6, 10),
                IsActive = true,
                Notes = "Expert in large breed dogs and exotic animals"
            }
        );

        // Services (full list)
        modelBuilder.Entity<Service>().HasData(
            // Grooming
            new Service { Id = 1, Name = "Basic Grooming", Price = 40m, DurationMinutes = 60, IsActive = true },
            new Service { Id = 2 , Name = "Full Grooming", Price = 80m, DurationMinutes = 90, IsActive = true },
            new Service { Id = 3, Name = "Bath Only", Price = 25m, DurationMinutes = 30, IsActive = true },
            new Service { Id = 4, Name = "Nail Trim", Price = 15m, DurationMinutes = 15, IsActive = true },

            // Doctor
            new Service { Id = 5, Name = "Vet Consultation", Price = 60m, DurationMinutes = 30, IsActive = true },
            new Service { Id = 6, Name = "General Health Check", Price = 80m, DurationMinutes = 45, IsActive = true },
            new Service { Id = 7, Name = "Vaccination", Price = 70m, DurationMinutes = 20, IsActive = true },
            new Service { Id = 8, Name = "Flea/Tick Treatment", Price = 50m, DurationMinutes = 30, IsActive = true },
            new Service { Id = 9, Name = "Minor Wound Care", Price = 90m, DurationMinutes = 45, IsActive = true },
            new Service { Id = 10, Name = "Blood Test (Basic)", Price = 120m, DurationMinutes = 45, IsActive = true },
            new Service { Id = 11, Name = "Spay/Neuter", Price = 250m, DurationMinutes = 120, IsActive = true, Description = "Base price; final amount may vary" },
            new Service { Id = 12, Name = "Dental Care", Price = 50m, DurationMinutes = 30, IsActive = true },

            // Custom
            new Service { Id = 13, Name = "Other / Custom Request", Price = 0m, DurationMinutes = 60, IsActive = true, Description = "Describe in Special Notes" }
        );
    }
} 

public class Doctor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Doctor name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "Name cannot contain numbers")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
    public string? Specialization { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime JoinDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    // Navigation property
    public virtual ICollection<Appointment> Pets { get; set; } = new List<Appointment>();
}


public class Staff
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Staff name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "Name cannot contain numbers")]
    public string Name { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    // Navigation property
    public virtual ICollection<Appointment> Pets { get; set; } = new List<Appointment>();
}



//Member model
public class Member
{
    [Key, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PhotoURL { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
}

// User model
public class User
{
    [Key, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Column("Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";
    [MaxLength(200)]
    public string? PhotoPath { get; set; }

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }

    public int FailedAttempts { get; set; } = 0;   // 连续失败次数
    public bool IsLocked { get; set; } = false;
}

// Appointment model
public class Appointment
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Pet Owner Name is required")]
    [StringLength(100, ErrorMessage = "Owner Name cannot exceed 100 characters")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "Name cannot contain numbers")]
    [Display(Name = "Pet Owner Name")]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pet Name is required")]
    [StringLength(100, ErrorMessage = "Pet Name cannot exceed 100 characters")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "Name cannot contain numbers")]
    [Display(Name = "Pet Name")]
    public string PetName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Pet Type")]
    public string PetType { get; set; } = string.Empty;

    public string PetBreed { get; set; } = string.Empty;

    [Range(0, 99, ErrorMessage = "Age must be between 0 and 99.")]
    public int? Age { get; set; }


    public string? Gender { get; set; }

    [Required, EmailAddress, StringLength(255)]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    [Display(Name = "Phone Number")]
    [RegularExpression(@"^01(?:[\s-]?\d){8,9}", ErrorMessage = "Please enter a valid mobile number (starts with 01, 10–11 digits)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Service Type")]
    public string? ServiceType { get; set; }   // Legacy, optional

    [Required]
    public int? ServiceId { get; set; }          // FK
    public virtual Service? Service { get; set; } = null!;


    [Required, Display(Name = "Appointment Date")]
    public DateTime AppointmentDate { get; set; }

    [StringLength(500)]
    [Display(Name = "Special Notes")]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }

    public int? StaffId { get; set; }
    public virtual Staff? Staff { get; set; }
    public bool IsApproved { get; set; }
}

// Service model
public class Service
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0m;

    public int DurationMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;

}
public class Payment
{
    [Key]
    public int Id { get; set; }

    public int AppointmentId { get; set; }   // FK to Appointment
    public int ServiceId { get; set; }       // FK to Service

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Method { get; set; } = string.Empty;  // Card, TNG, etc.

    [MaxLength(100)]
    public string? CardHolderName { get; set; }

    [MaxLength(30)]
    public string? CardMasked { get; set; }  // e.g. **** **** **** 1234

    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;  // OTP / QR / Ref No.

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}