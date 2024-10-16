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
    }
