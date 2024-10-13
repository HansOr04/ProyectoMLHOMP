using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMLHOMP.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

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

        // Propiedades adicionales para hosts
        public string HostBio { get; set; }

        public string HostLanguages { get; set; }

        // Propiedades para verificación de identidad
        public bool IdVerified { get; set; }

        public bool EmailVerified { get; set; }

        public bool PhoneVerified { get; set; }

        // Propiedad para el nombre completo
        public string FullName => $"{FirstName} {LastName}";

        // Propiedades adicionales para la creación y actualización de usuarios
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Propiedades adicionales que podrían ser útiles en la vista
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }
    }
}