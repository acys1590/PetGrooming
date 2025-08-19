using Microsoft.EntityFrameworkCore;    
using System.ComponentModel.DataAnnotations;

namespace PetGrooming.Models;

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options)
    {

    }

public class Member : User
{
    [MaxLength(100)]
    public string PhotoURL { get; set; }
}
