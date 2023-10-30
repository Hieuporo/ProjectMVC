using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectMVC.Data;
using ProjectMVC.Models;
using ProjectMVC.Models.ViewModels;
using System.Security.Claims;

namespace ProjectMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        public readonly DataContext _db;
        public CartController(DataContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartViewModel shoppingCartViewModel = new()
            {
                ShoppingCartList = _db.ShoppingCarts.Where(u => u.ApplicationUserId == userId).Include(u => u.Product),
                OrderHeader = new()
            };
            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;
                shoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartViewModel);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _db.ShoppingCarts.Find(cartId);
            cartFromDb.Count += 1;
            _db.ShoppingCarts.Update(cartFromDb);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _db.ShoppingCarts.Find(cartId);
            if (cartFromDb.Count <= 1)
            {
                _db.ShoppingCarts.Remove(cartFromDb);

            }
            else
            {
                cartFromDb.Count -= 1;
                _db.ShoppingCarts.Update(cartFromDb);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _db.ShoppingCarts.Find(cartId);

            _db.ShoppingCarts.Remove(cartFromDb);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCartList = _db.ShoppingCarts.Where(u => u.ApplicationUserId == userId).Include(u => u.Product);

            ShoppingCartViewModel shoppingCartViewModel = new()
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new()
            };

            shoppingCartViewModel.OrderHeader.ApplicationUser = _db.ApplicationUsers.Find(userId);
            shoppingCartViewModel.OrderHeader.Name = shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartViewModel.OrderHeader.Address = shoppingCartViewModel.OrderHeader.ApplicationUser.Address;


            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;
                shoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartViewModel);
        }

        [HttpPost]
        public IActionResult CreateOrder()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

  

            ShoppingCartViewModel shoppingCartViewModel = new()
            {
                ShoppingCartList = _db.ShoppingCarts.Where(u => u.ApplicationUserId == userId).Include(u => u.Product),
                OrderHeader = new()
            };

            if(shoppingCartViewModel.ShoppingCartList.Count()== 0)
            {
                TempData["success"] = "Please add product to cart before checkout";
				return RedirectToAction("Index", "Home");
			}

            shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _db.ApplicationUsers.Find(userId) ;

            shoppingCartViewModel.OrderHeader.Address = applicationUser.Address;
            shoppingCartViewModel.OrderHeader.Name = applicationUser.Name;
            shoppingCartViewModel.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;

            foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;
                shoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            shoppingCartViewModel.OrderHeader.OrderStatus = "Pending";


            _db.OrderHeaders.Add(shoppingCartViewModel.OrderHeader);
			_db.SaveChanges();


            var listOrderDetail = new List<OrderDetail>();

			foreach (var cart in shoppingCartViewModel.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartViewModel.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };

				listOrderDetail.Add(orderDetail);
				var cartItem =_db.ShoppingCarts.Find(cart.Id);
                _db.ShoppingCarts.Remove(cartItem);
			}

            _db.OrderDetails.AddRange(listOrderDetail);
            _db.SaveChanges();
			TempData["success"] = "Order created successfully";
			return RedirectToAction("Index", "Home");
        }
    }
}
