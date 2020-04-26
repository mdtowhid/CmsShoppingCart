using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CmsShoppingCart.Models.ViewModels.Pages;

namespace CmsShoppingCart.Models.Data
{
    public class CmsDbContext : DbContext
    {
        public DbSet<PageDto> Pages { get; set; }
        public DbSet<SidebarDto> Sidebars { get; set; }
        public DbSet<CategoryDto> Categories { get; set; }
        public DbSet<ProductDto> Products { get; set; }

        public System.Data.Entity.DbSet<CmsShoppingCart.Models.ViewModels.Shop.ProductVm> ProductVms { get; set; }

        public System.Data.Entity.DbSet<CmsShoppingCart.Models.ViewModels.Pages.PageVm> PageVms { get; set; }
    }
}