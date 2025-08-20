using Microsoft.EntityFrameworkCore;    
using System.ComponentModel.DataAnnotations;

namespace PetGrooming.Models;

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options)
    {

    }


}
