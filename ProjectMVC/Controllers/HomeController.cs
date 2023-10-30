using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectMVC.Data;
using ProjectMVC.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ProjectMVC.Controllers
{
    public class HomeController : Controller
    {

        private readonly DataContext _db;
        public HomeController(DataContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _db.Products.ToList();
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _db.Products.Find(productId),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _db.ShoppingCarts.Update(cartFromDb);
                _db.SaveChanges();
            }
            else
            {
                _db.ShoppingCarts.Add(shoppingCart);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index) , "Cart");
        }


    }
}