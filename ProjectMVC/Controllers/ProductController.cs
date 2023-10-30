using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectMVC.Data;
using ProjectMVC.Models;

namespace ProjectMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _db;
        private readonly IWebHostEnvironment _webHostEvironment;
        public ProductController(DataContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEvironment = webHostEnvironment;
        }

		[Authorize(Roles = "Admin")]
		public IActionResult Index()
        {
            List<Product> objProductList = _db.Products.ToList();

            return View(objProductList);
        }

		[Authorize(Roles = "Admin")]
		public IActionResult Upsert(int? id)
        {

            var product = new Product();
            // create 
            if (id == null || id == 0)
            {
                return View(product);
            }// update
            else
            {
                product = _db.Products.Find(id);
                return View(product);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Upsert(Product obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {   // save image and delete old image if it exists
                string wwwRootPath = _webHostEvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (obj.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.ImageUrl = @"\images\product\" + fileName;

                }// create
                if (obj.Id == 0)
                {
                    _db.Products.Add(obj);
                }
                else // update
                {
                    _db.Products.Update(obj);
                }
                _db.SaveChanges();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(int id)
        {
            List<Product> objProductList = _db.Products.ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _db.Products.Find(id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while dectecting" });
            }

            var oldImagePath = Path.Combine(_webHostEvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _db.Products.Remove(productToBeDeleted);
            _db.SaveChanges();

            return Json(new { success = true, message = "Delete Successful" });
        }


        #endregion
    }
}
