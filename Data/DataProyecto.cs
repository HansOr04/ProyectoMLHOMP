using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Data
{
    public class DataProyecto : DbContext
    {
        public DataProyecto(DbContextOptions<DataProyecto> options)
            : base(options)
        {
        }

        public DbSet<Apartment> Apartment { get; set; } = default!;
        public DbSet<Booking> Booking { get; set; } = default!;
        public DbSet<Review> Review { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para evitar múltiples rutas de cascada
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Apartment)
                .WithMany(a => a.Bookings)
                .HasForeignKey(b => b.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.GuestUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.Owner)
                .WithMany(u => u.OwnedApartments)
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Apartment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.WrittenReviews)
                .HasForeignKey(r => r.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de índices únicos para User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}