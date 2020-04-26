using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsShoppingCart.Models.Data;

namespace CmsShoppingCart.Models.ViewModels.Shop
{
    public class CategoryVm
    {
        public CategoryVm()
        {
            
        }

        public CategoryVm(CategoryDto row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Sorting = row.Sorting;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sorting { get; set; }
    }
}