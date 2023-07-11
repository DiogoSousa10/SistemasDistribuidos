using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Modalidades_Domicilio
    {
        [Key]
        public int Id { get; set; }
        public int DomiciliosNum_Admin { get; set; }
        public int ModalidadesId { get; set; }
        // outras propriedades, se houver

        public Domicilio Domicilios { get; set; }
        public Modalidade Modalidades { get; set; }
    }
}
