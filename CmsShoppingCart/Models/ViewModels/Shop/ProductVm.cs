﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;

namespace CmsShoppingCart.Models.ViewModels.Shop
{
    public class ProductVm
    {
        public ProductVm()
        {
        }

        public ProductVm(ProductDto row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Description = row.Description;
            Price = row.Price;
            CategoryName = row.CategoryName;
            CategoryId = row.CategoryId;
            ImageName = row.ImageName;
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }

        [Required]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public string ImageName { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<string> GalleryImages { get; set; }
    }
}