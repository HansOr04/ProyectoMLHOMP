using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;

    public class DataProyecto : DbContext
    {
        public DataProyecto (DbContextOptions<DataProyecto> options)
            : base(options)
        {
        }

        public DbSet<ProyectoMLHOMP.Models.Aparment> Aparment { get; set; } = default!;

public DbSet<ProyectoMLHOMP.Models.Booking> Booking { get; set; } = default!;

public DbSet<ProyectoMLHOMP.Models.Review> Review { get; set; } = default!;

public DbSet<ProyectoMLHOMP.Models.User> User { get; set; } = default!;
    }
