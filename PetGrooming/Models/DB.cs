using Microsoft.EntityFrameworkCore;
using PetGroomingSystem.Models;


namespace PetGrooming.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options) { }

    // 定义数据表
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;

    // 如果你还有其他模型，比如 Test.cs
    public DbSet<Test> Tests { get; set; } = null!;
}
