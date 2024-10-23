using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoMLHOMP.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(100)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es requerida")]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        public bool IsHost { get; set; }

        public bool IsVerified { get; set; }

        [Required]
        [StringLength(500)]
        public string Biography { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Languages { get; set; } = string.Empty;

        public string ProfileImageUrl { get; set; } = "/images/default-profile.jpg";

        // Navegación
        public virtual ICollection<Apartment>? ApartmentsOwned { get; set; }
        public virtual ICollection<Booking>? BookingsAsGuest { get; set; }
        public virtual ICollection<Review>? ReviewsWritten { get; set; }

        public string GetFullName() => $"{FirstName} {LastName}";

        public bool VerifyPassword(string password)
        {
            return HashPassword(password) == this.PasswordHash;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}