using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        public ActionResult Index()
        {
            List<PageVm> pages;

            using (CmsDbContext db = new CmsDbContext())
            {
                pages = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVm(x)).ToList();
            }

            return View(pages);
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            int count = 1;
            PageDto dto;

            foreach (var pageId in id)
            {
                dto = db.Pages.Find(pageId);
                dto.Sorting = count;

                db.SaveChanges();
                count++;
            }

        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPage(PageVm pageVm)
        {
            using (CmsDbContext db = new CmsDbContext())
            {
                string slug;
                PageDto pageDto = new PageDto();

                pageDto.Title = pageVm.Title;

                if (!ModelState.IsValid)
                {
                    return View();
                }

                if (string.IsNullOrWhiteSpace(pageVm.Slug))
                {
                    slug = pageVm.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = pageVm.Slug.Replace(" ", "-").ToLower();
                }

                if (db.Pages.Any(x => x.Title == pageVm.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title already exist");
                    return View();
                }

                pageDto.Slug = slug;
                pageDto.Body = pageVm.Body;
                pageDto.HasSideBar = pageVm.HasSideBar;
                pageDto.Sorting = 100;

                db.Pages.Add(pageDto);
                db.SaveChanges();

                TempData["SuccessMessage"] = "You have added a new Page";
                return RedirectToAction("AddPage");
            }
        }


        CmsDbContext db = new CmsDbContext();

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVm model;
            using (db)
            {
                PageDto dto = db.Pages.Find(id);
                if (dto==null)
                {
                    return Content("The page doesn't exist.");
                }
                model = new PageVm(dto);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (CmsDbContext db = new CmsDbContext())
            {
                var slug = "Home";
                int id = model.Id;

                PageDto dto = db.Pages.Find(id);
                dto.Title = model.Title;

                if (model.Slug != "Home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "This slug or title already exist.");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;

                if (db.SaveChanges() > 0)
                {
                    TempData["SuccessMessage"] = "You have Edited this page!";
                    return RedirectToAction("EditPage");
                }
            }
            TempData["SuccessMessage"] = "There is an error while updating this page";
            return View(model);
        }

        //sidebar

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVm model;
            SidebarDto dto = db.Sidebars.Find(1);

            model = new SidebarVm(dto);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVm model)
        {
            SidebarDto dto = db.Sidebars.Find(1);
            dto.Body = model.Body;

            db.SaveChanges();

            TempData["SuccessMessage"] = "You have edited this page!";

            return RedirectToAction("EditSidebar");
        }
    }
}