
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //deklaracja listy Kategorii ktore bedziemy wyświetlać
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x)).ToList();

            }

            return View(categoryVMList);
        }

        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;
            using (Db db = new Db())
            {
                //sprawdz czy unikalne
                if (db.Categories.Any(x => x.Name == catName))
                    return "tutulzajety";

                //init DTO
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                // zapis do bazy
                db.Categories.Add(dto);
                db.SaveChanges();

                // pobierz id
                id = dto.Id.ToString();

            }

            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // inicjalizacja licznika
                int count = 1;

                // deklaracja DTO
                CategoryDTO dto;

                // sortowanie kategorii
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    // zapis na bazie
                    db.SaveChanges();

                    count++;
                }
            }

            return View();
        }

        //GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)

        {
            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.Find(id);
                db.Categories.Remove(dto);

                db.SaveChanges();

            }

            return RedirectToAction("Categories");
        }

        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model)
        {
            // sprawdzamy model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // sprawdzenie czy nazwa produktu jest unikalna
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta!");
                    return View(model);
                }
            }

            // deklaracja product id
            int id;

            // dodawanie produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);    //linq
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // pobranie id dodanego produktu
                id = product.Id;
            }

            // ustawiamy komunikat 
            TempData["SysMsg"] = "Dodałeś produkt";
            
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //delarujemy listę produktów
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // inicjalizacja listy produktów
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // lista kategori do dropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawiamy wybrana ketegorię
                ViewBag.SelectedCat = catId.ToString();
            }
            // ustawienie stronnicowanie
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //zwracamy widok z lista produktów
            return View(listOfProductVM);

        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // deklaracja productVM
            ProductVM model;

            using (Db db = new Db())
            {
                // pobieramy produkt do edycji
                ProductDTO dto = db.Products.Find(id);

                // sprawdzenie czy produkt istnieje
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieja");
                }

                // inicjalizacja modelu
                model = new ProductVM(dto);

                // lista kategorii
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                
            }
            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model)
        {
            // pobranie id produktu
            int id = model.Id;

            // pobranie kategorii dla listy rozwijanej
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            // sprawdzamy model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzenie unikalnosci nazwy produktu
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }

            // Edycja produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            // ustawienie TempData
            TempData["SysMsg"] = "Edytowałeś produkt";

            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // usuniecie produktu z bazy
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            
            return RedirectToAction("Products");
        }
    }
}