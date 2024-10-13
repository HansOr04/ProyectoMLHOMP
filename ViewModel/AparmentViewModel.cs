using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMLHOMP.ViewModels
{
    public class ApartmentViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "El precio por noche debe estar entre 0 y 10000.")]
        [DataType(DataType.Currency)]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        [Range(1, 10, ErrorMessage = "El número de dormitorios debe estar entre 1 y 10.")]
        public int Bedrooms { get; set; }

        [Range(1, 10, ErrorMessage = "El número de baños debe estar entre 1 y 10.")]
        public int Bathrooms { get; set; }

        [Range(1, 20, ErrorMessage = "La ocupación máxima debe estar entre 1 y 20.")]
        public int MaxOccupancy { get; set; }

        public bool IsAvailable { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public string OwnerName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Propiedades adicionales que pueden ser útiles en la vista
        public string FormattedPrice => PricePerNight.ToString("C");

        public string ShortDescription => Description.Length > 100 ? Description.Substring(0, 97) + "..." : Description;

        public string FullAddress => $"{Address}, {City}, {Country}";

        public string AvailabilityStatus => IsAvailable ? "Disponible" : "No disponible";

        public string FormattedCreatedDate => CreatedAt.ToString("dd MMM yyyy");

        public string FormattedUpdatedDate => UpdatedAt?.ToString("dd MMM yyyy") ?? "No actualizado";

        // Propiedades para mostrar información resumida
        public string Summary => $"{Bedrooms} habitaciones • {Bathrooms} baños • Máx. {MaxOccupancy} personas";

        // Propiedad para facilitar la visualización en listas o tarjetas
        public string ThumbnailUrl => ImageUrl ?? "/images/default-apartment.jpg";

        // Método para verificar si el apartamento ha sido actualizado
        public bool HasBeenUpdated => UpdatedAt.HasValue && UpdatedAt.Value != CreatedAt;
    }
}