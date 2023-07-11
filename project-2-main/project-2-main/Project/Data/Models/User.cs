using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        // outras propriedades, se houver

        public List<Reserva> Reserva { get; set; }
    }
}