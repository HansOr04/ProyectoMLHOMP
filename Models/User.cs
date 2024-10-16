namespace ProyectoMLHOMP.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsHost { get; set; }
        public bool IsVerified { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Relaciones
        public List<Aparment> OwnedApartments { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<Review> WrittenReviews { get; set; }
    }
}
