using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Domicilio
    {
        [Key]
        public int Num_Admin { get; set; }
        public string Estado { get; set; }
        public string Nome { get; set; }
        // outras propriedades, se houver
    }
}
