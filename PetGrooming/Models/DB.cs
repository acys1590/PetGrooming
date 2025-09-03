using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetGrooming.Models;

// DbContext class - moved PhotoURL property out of here as it doesn't belong
public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    // Define database tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    //public DbSet<Pet> Pets { get; set; } 
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Staff> Staffs { get; set; }

    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Pet entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.AppointmentDate).HasDefaultValueSql("GETDATE()");
            
            // Configure relationship
            entity.HasOne(p => p.Doctor)
                  .WithMany(d => d.Pets)
                  .HasForeignKey(p => p.DoctorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Doctor entity
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.JoinDate).HasDefaultValueSql("GETDATE()");
            entity.Property(d => d.IsActive).HasDefaultValue(true);
        });

        //Configure Member entity
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
    }
}

// Doctor model
public class Doctor
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Specialization { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public DateTime JoinDate { get; set; }

    public bool IsActive { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    public virtual ICollection<Appointment> Pets { get; set; } = new List<Appointment>();
}

public class Staff
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public DateTime JoinDate { get; set; }

    public bool IsActive { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public virtual ICollection<Appointment> Pets { get; set; } = new List<Appointment>();

}

// Pet model
//public class Pet
//{
//    public int Id { get; set; }

//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; } = string.Empty;

//    [Required]
//    [StringLength(50)]
//    public string Species { get; set; } = string.Empty;

//    [StringLength(50)]
//    public string? Breed { get; set; }

//    public int? Age { get; set; }

//    [StringLength(20)]
//    public string? Gender { get; set; }

//    [StringLength(20)]
//    public string? Service { get; set; }


//    [StringLength(100)]
//    public string? OwnerName { get; set; }

//    [StringLength(15)]
//    public string? OwnerPhone { get; set; }

//    [EmailAddress]
//    [StringLength(100)]
//    public string? OwnerEmail { get; set; }

//    public DateTime AppointmentDate { get; set; }

//    public DateTime AppointmentTime { get; set; }

//    [StringLength(500)]
//    public string? Notes { get; set; }

//    // Foreign Key
//    public int? DoctorId { get; set; }

//    // Navigation Property
//    public virtual Doctor? Doctor { get; set; }
//}

 //Member model
public class Member
{
    [Key]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PhotoURL { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class User
{
    [Key]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";

    [MaxLength(200)]
    public string? PhotoPath { get; set; }

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
}


//Appointment

public class Appointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Pet Owner Name is required")]
    [StringLength(100, ErrorMessage = "Owner Name cannot exceed 100 characters")]
    [Display(Name = "Pet Owner Name")]
    [Column(TypeName = "nvarchar(100)")]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pet Name is required")]
    [StringLength(100, ErrorMessage = "Pet Name cannot exceed 100 characters")]
    [Display(Name = "Pet Name")]
    [Column(TypeName = "nvarchar(100)")]
    public string PetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pet Type is required")]
    [StringLength(50, ErrorMessage = "Pet Type cannot exceed 50 characters")]
    [Display(Name = "Pet Type")]
    [Column(TypeName = "nvarchar(50)")]
    public string PetType { get; set; } = string.Empty;

    public string PetBreed { get; set; } = string.Empty;

     public int? Age { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Email Address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [Display(Name = "Email Address")]
    [Column(TypeName = "nvarchar(255)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
    [Display(Name = "Phone Number")]
    [Column(TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Service Type is required")]
    [StringLength(50, ErrorMessage = "Service Type cannot exceed 50 characters")]
    [Display(Name = "Service Type")]
    [Column(TypeName = "nvarchar(50)")]
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Appointment Date is required")]
    [Display(Name = "Appointment Date")]
    [Column(TypeName = "datetime2")]
    public DateTime AppointmentDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [Display(Name = "Special Notes")]
    [Column(TypeName = "nvarchar(500)")]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    // Foreign Key
    public int? DoctorId { get; set; }

    // Navigation Property
    public virtual Doctor? Doctor { get; set; }
}
