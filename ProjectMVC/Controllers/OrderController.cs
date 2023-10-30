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
	public class OrderController : Controller
	{
        private readonly DataContext _db;
        public OrderController(DataContext db)
        {
            _db = db;   
        }

        public IActionResult Index()
		{	

			return View();
		}

        public IActionResult Details(int orderId)
        {

            OrderViewModel orderViewModel = new()
            {
                OrderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == orderId),
                OrderDetails = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderId == orderId).ToList(),
            };

            return View(orderViewModel);
        }

        [Authorize (Roles = "Admin")]
        public IActionResult ConfirmOrder(int orderId)
        {
			var order = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId);
			order.OrderStatus = "Approved";
			_db.SaveChanges();

            return RedirectToAction("Index");
        }


        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objOrderHeaders;
			// display all orders if admin
			if (User.IsInRole("Admin"))
			{
				objOrderHeaders = _db.OrderHeaders.Include(u => u.ApplicationUser).ToList();
			}
			else // display only user order
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
				objOrderHeaders = _db.OrderHeaders.Include(u => u.ApplicationUser).Where(u => u.ApplicationUserId == userId);
			}

			switch (status)
			{
				case "pending":
					objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == "Pending");
					break;
				case "approved":
					objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == "Approved");
					break;
				default:
					break;

			}
			return Json(new { data = objOrderHeaders });
		}


		#endregion
	}
}
