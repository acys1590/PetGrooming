//using Microsoft.EntityFrameworkCore;
//using PetGroomingSystem.Models;

//namespace PetGroomingSystem.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }

//        public DbSet<Pet> Pets { get; set; }
//        public DbSet<Doctor> Doctors { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // Configure Pet entity
//            modelBuilder.Entity<Pet>(entity =>
//            {
//                entity.HasKey(p => p.Id);
//                entity.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");

//                // Configure relationship
//                entity.HasOne(p => p.Doctor)
//                      .WithMany(d => d.Pets)
//                      .HasForeignKey(p => p.DoctorId)
//                      .OnDelete(DeleteBehavior.SetNull);
//            });

//            // Configure Doctor entity
//            modelBuilder.Entity<Doctor>(entity =>
//            {
//                entity.HasKey(d => d.Id);
//                entity.Property(d => d.JoinDate).HasDefaultValueSql("GETDATE()");
//                entity.Property(d => d.IsActive).HasDefaultValue(true);
//            });

//            // Seed initial data
//            SeedData(modelBuilder);
//        }

//        private void SeedData(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Doctor>().HasData(
//                new Doctor
//                {
//                    Id = 1,
//                    Name = "Dr. Sarah Johnson",
//                    Specialization = "Small Animals",
//                    Phone = "123-456-7890",
//                    Email = "sarah.johnson@petcare.com",
//                    Address = "123 Main St, City",
//                    JoinDate = new System.DateTime(2020, 1, 15),
//                    IsActive = true,
//                    Notes = "Specializes in cats and small dogs"
//                },
//                new Doctor
//                {
//                    Id = 2,
//                    Name = "Dr. Michael Chen",
//                    Specialization = "Large Animals",
//                    Phone = "123-456-7891",
//                    Email = "michael.chen@petcare.com",
//                    Address = "456 Oak Ave, City",
//                    JoinDate = new System.DateTime(2019, 6, 10),
//                    IsActive = true,
//                    Notes = "Expert in large breed dogs and exotic animals"
//                }
//            );
//        }
//    }

//}

