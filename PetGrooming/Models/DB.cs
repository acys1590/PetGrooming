using Microsoft.EntityFrameworkCore;    
namespace PetGrooming.Models;

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options)
    {
    }
}
