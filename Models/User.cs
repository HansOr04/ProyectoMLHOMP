using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMLHOMP.Models
{
    public class User
    {
        [Key]
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
        public ICollection<Apartment> Apartments { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Booking> Bookings { get; set; }

        // Métodos útiles
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}

