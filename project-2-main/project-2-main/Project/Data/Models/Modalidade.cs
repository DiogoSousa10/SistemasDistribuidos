﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Modalidade
    {
        [Key]
        public int Id { get; set; }
        public int Megas { get; set; }
        // outras propriedades, se houver
    }
}
