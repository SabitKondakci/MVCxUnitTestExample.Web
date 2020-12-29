using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MVCxUnitTestExample.Web.Models
{
    public partial class Product
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal? Price { get; set; }
        [Required]
        public int? Stock { get; set; }
        [Required]
        public string Color { get; set; }

    }
}
