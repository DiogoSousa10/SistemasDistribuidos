using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Modalidades_DomiciliosId { get; set; }

        public User User { get; set; }
        public Modalidades_Domicilio Modalidades_Domicilios { get; set; }
    }
}