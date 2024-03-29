﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Models.Data
{
    [Table("tblProducts")]
    public class ProductDto
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string ImageName { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CategoryDto Category { get; set; }
    }
}