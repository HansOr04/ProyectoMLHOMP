using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMLHOMP.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime RegistrationDate { get; set; }

        public bool IsHost { get; set; }

        public bool IsVerified { get; set; }

        [StringLength(500)]
        public string Biography { get; set; }

        [StringLength(100)]
        public string Languages { get; set; }

        public string ProfileImageUrl { get; set; }

        // Relaciones
        public ICollection<Apartment> OwnedApartments { get; set; }
        public ICollection<Review> WrittenReviews { get; set; }
        public ICollection<Booking> Bookings { get; set; }

        public User()
        {
            RegistrationDate = DateTime.UtcNow;
            OwnedApartments = new List<Apartment>();
            WrittenReviews = new List<Review>();
            Bookings = new List<Booking>();
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}