using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;

namespace CmsShoppingCart.Models.ViewModels.Pages
{
    public class PageVm
    {
        public PageVm()
        {
            
        }

        public PageVm(PageDto pageDto)
        {
            Id = pageDto.Id;
            Title = pageDto.Title;
            Slug = pageDto.Slug;
            Body = pageDto.Body;
            Sorting = pageDto.Sorting;
            HasSideBar = pageDto.HasSideBar;
        }
        
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSideBar { get; set; }
    }
}