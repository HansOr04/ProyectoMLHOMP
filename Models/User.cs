using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProyectoMLHOMP.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string Address { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string City { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = "";

        [Required]
        [Phone(ErrorMessage = "Número de teléfono inválido")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string PasswordHash { get; set; } = "";

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsHost { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        [Required]
        [StringLength(500)]
        public string Biography { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Languages { get; set; } = "";

        [Required]
        public string ProfileImageUrl { get; set; } = "/images/default-profile.jpg";

        // Relaciones
        public ICollection<Apartment> OwnedApartments { get; set; } = new List<Apartment>();
        public ICollection<Review> WrittenReviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public string GetFullName() => $"{FirstName} {LastName}";
    }
}
