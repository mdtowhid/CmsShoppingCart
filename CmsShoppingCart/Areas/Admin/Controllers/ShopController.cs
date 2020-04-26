using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        private CmsDbContext db = new CmsDbContext();

        public ActionResult Categories()
        {
            var categories = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVm(x));
            return View(categories);
        }

        [HttpPost]
        public string AddNewCategory(string catNameValue)
        {
            var check = db.Categories.Any(x => x.Name == catNameValue);
            if (check)
            {
                return "titleTaken";
            }
            CategoryDto dto = new CategoryDto();
            dto.Name = catNameValue;
            dto.Slug = catNameValue.Replace(" ", "-");
            dto.Sorting = 100;

            db.Categories.Add(dto);
            db.SaveChanges();

            var id = dto.Id.ToString();

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            int count = 1;
            CategoryDto dto;

            foreach (var catId in id)
            {
                dto = db.Categories.Find(catId);
                dto.Sorting = count;

                db.SaveChanges();
                count++;
            }

        }

        public ActionResult DeleteCategory(int id)
        {
            CategoryDto dto = db.Categories.Find(id);
            if (dto != null)
            {
                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        public void UpdateCategoryName(string oldValue, string newValue, int id)
        {
            var check = db.Categories.Find(id);

            if (check != null)
            {
                check.Name = newValue;
            }
            db.SaveChanges();
        }

        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVm> products;
            var pageNumber = page ?? 1;
            products =
                db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVm(x))
                    .ToList();

            ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            //set selected paged list
            ViewBag.SelectedCategoryId = catId.ToString();

            var onePageOfProducts = products.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(products);
        }


        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVm model = new ProductVm { Categories = new SelectList(db.Categories.ToList(), "Id", "Name") };
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVm model, HttpPostedFileBase file)
        {
            int id;
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                return View(model);
            }

            if (db.Products.Any(x => x.Name == model.Name))
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                ModelState.AddModelError("", "This Product Name Already Exist.");
                return View(model);
            }
            else
            {
                ProductDto product = new ProductDto();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDto categoryDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                if (categoryDto != null)
                {
                    product.CategoryName = categoryDto.Name;
                }
                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            TempData["SuccessMessage"] = "You have added a product!";

            #region Upload Image

            //Create neccessary directories
            var orginialDirectory = new DirectoryInfo(String.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString1 = Path.Combine(orginialDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orginialDirectory.ToString(),
                "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check the file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                //Get file extension
                var ext = file.ContentType.ToLower();
                //Verify extension
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" &&
                    ext != "image/x-png" && ext != "image/png")
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The image was not uploded. File format not suuported");
                    return View(model);
                }
                //Init Image Name
                string imageName = file.FileName;

                //Save image name to dto
                ProductDto productDto = db.Products.Find(id);
                if (productDto != null)
                {
                    productDto.ImageName = imageName;
                    db.SaveChanges();
                }

                //Set orginial and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                //Save orginial
                file.SaveAs(path);
                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Redirect...
            return RedirectToAction("AddProduct");
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductDto product = db.Products.Find(id);
            if (product == null)
            {
                ModelState.AddModelError("", "The product doesn't exist.");
                return View();
            }

            var model = new ProductVm(product);
            model.Categories = new SelectList(db.Categories, "Id", "Name");

            //get all gallery images
            model.GalleryImages =
                Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVm model, HttpPostedFileBase file)
        {
            int id = model.Id;
            model.Categories = new SelectList(db.Categories, "Id", "Name");
            model.GalleryImages =
                Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Make sure product name unique

            if (db.Products.Where(x => x.Id == id).Any(x => x.Name == model.Name))
            {
                ModelState.AddModelError("", "That product name is taken");
                return View(model);
            }

            //update product
            ProductDto dto = db.Products.Find(id);
            if (dto != null)
            {
                dto.Name = model.Name;
                dto.Slug = model.Slug;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                model.ImageName = model.ImageName;

                CategoryDto catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Product Edited Successfully";

            #region Upload Image

            //Check the file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                //Get file extension
                var ext = file.ContentType.ToLower();
                //Verify extension
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" &&
                    ext != "image/x-png" && ext != "image/png")
                {
                    ModelState.AddModelError("", "The image was not uploded. File format not suuported");
                    return View(model);
                }

                //set upload directory path

                var orginialDirectory = new DirectoryInfo(String.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                var pathString1 = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete file from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (FileInfo file3 in di1.GetFiles())
                {
                    file3.Delete();
                }

                //Init Image Name
                string imageName = file.FileName;

                //Save image name to dto
                ProductDto productDto = db.Products.Find(id);
                if (productDto != null)
                {
                    productDto.ImageName = imageName;
                    db.SaveChanges();
                }

                //Set orginial and thumb image paths
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);
                //Save orginial
                file.SaveAs(path);
                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }

            var orginialDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginialDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);
            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop through files
            foreach (string fileName in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[fileName];
                if (file != null && file.ContentLength > 0)
                {
                    var orginialDirectory = new DirectoryInfo(String.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                    var pathString1 = Path.Combine(orginialDirectory.ToString(),
                        "Products\\" + id.ToString() + "\\Gallery");
                    var pathString2 = Path.Combine(orginialDirectory.ToString(),
                        "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Set orginial and thumb image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //Save orginial
                    file.SaveAs(path);

                    //Create and save thumb
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 =
                Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
    }
}
















