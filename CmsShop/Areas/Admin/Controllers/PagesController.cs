using System.Collections.Generic;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Linq;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            // Deklaracja listy PageVM
            List<PageVM> pagesList;


            using (Db db = new Db())
            {
                //inicjalizaacja listy
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();

            }

            //zwracamy do widoku
            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Sprawdzanie formularza
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                //inicjalizacja Page DTO
                PageDTO dto = new PageDTO();

                dto.Title = model.Title;


                //WALIDACJA w przypadku braku adreu przypisujemy tytuł
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                // WALIDACJA na dodanie tej samej strony po raz drugi
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres strony juz istnieje");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SysMsg"] = "Dodałeś nową stronę";

            return RedirectToAction("AddPage");
        }


        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                // pobieramy strone z bazy po ID 
                PageDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("strona nie istnieje");
                }

                model = new PageVM(dto);

            }

            return View(model);
        }

        // POST: Admin/Pages/EditPage
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {// pobranie id strony
                int id = model.Id;

                string slug = "home";

                PageDTO dto = db.Pages.Find(id);



                if (model.Slug != "home")
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
                // sprawdzamy czy strona jest unikalna, aby zapobiec edycji na juz istniejącą  
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Strona lub adres strony juz istnieje");
                }
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.HasSidebar = model.HasSidebar;
                dto.Body = model.Body;


                db.SaveChanges();
            }
            TempData["SysMsg"] = "Edytowałes stronę";

            return RedirectToAction("EditPage");
        }


        // GET: Admin/Pages/Details/1
        public ActionResult Details(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                //pobieramys trone o id
                PageDTO dto = db.Pages.Find(id);
                if (dto == null)
                {
                    return Content("strona o podanym id nie istnieje");
                }
                model =new PageVM(dto);
            }
            
            return View(model);
        }

        // GET: Admin/Pages/Delete/1
        public ActionResult Delete(int id)
            {
                using (Db db = new Db())
                {
                    PageDTO dto = db.Pages.Find(id);

                    db.Pages.Remove(dto);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }


        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            // Deklaracja SidebarVM
            SidebarVM model;

            using (Db db = new Db())
            {
                // Pobieramy SidebarDTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // Inicjalizacja modelu
                model = new SidebarVM(dto);
            }

            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                // pobieramy Sidebar DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // modyfikacja Sidebar
                dto.Body = model.Body;

                // Zapis na bazie
                db.SaveChanges();
            }

            // Ustawiamy komunikat o modyfikacji Sidebar
            TempData["SysMsg"] = "Zmodyfikowałeś Pasek Boczny";

            // Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}

