using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postgres.Models
{
    
    public class Point
    {        
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
    }
}
