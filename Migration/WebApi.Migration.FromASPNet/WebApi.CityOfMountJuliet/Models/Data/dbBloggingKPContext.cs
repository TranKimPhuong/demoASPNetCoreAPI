using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.CityOfMountJuliet.Models.Data
{
    public class dbBloggingKPContext : DbContext
    {
        public dbBloggingKPContext(DbContextOptions<dbBloggingKPContext> options)
            : base(options)
        {
        }
        //public DbSet<Student> Student { get; set; }
        //public DbSet<Enrollment> Enrollment { get; set; }
        //public DbSet<Course> Course { get; set; }
    }
}
