using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProyectoMLHOMP.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public bool IsHost { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }

        public string ProfilePictureUrl { get; set; }

        public bool IsVerified { get; set; }

        // Propiedades de navegación
        public virtual ICollection<Apartment> OwnedApartments { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        // Propiedades adicionales para hosts
        public string HostBio { get; set; }
        public string HostLanguages { get; set; }

        // Propiedades para verificación de identidad
        public bool IdVerified { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }

        // Constructor
        public User()
        {
            RegistrationDate = DateTime.UtcNow;
            IsHost = false;
            IsVerified = false;
            IdVerified = false;
            EmailVerified = false;
            PhoneVerified = false;
            OwnedApartments = new List<Apartment>();
            Bookings = new List<Booking>();
            Reviews = new List<Review>();
        }

        // Método para obtener el nombre completo
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}