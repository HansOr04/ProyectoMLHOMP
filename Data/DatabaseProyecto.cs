using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Data
{
    public class DatabaseProyecto : DbContext
    {
        public DatabaseProyecto(DbContextOptions<DatabaseProyecto> options)
            : base(options)
        {
        }

        public DbSet<ProyectoMLHOMP.Models.Apartment> Apartment { get; set; } = default!;
        public DbSet<ProyectoMLHOMP.Models.Booking> Booking { get; set; } = default!;
        public DbSet<ProyectoMLHOMP.Models.Review> Review { get; set; } = default!;
        public DbSet<ProyectoMLHOMP.Models.User> User { get; set; } = default!;
    }
}