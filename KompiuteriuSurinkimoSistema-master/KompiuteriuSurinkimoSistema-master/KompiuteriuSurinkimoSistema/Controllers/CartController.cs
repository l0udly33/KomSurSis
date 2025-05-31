using ComputerBuildSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComputerBuildSystem.Controllers
{
    public class CartController : Controller
    {

        private readonly PsaContext _context;

        public CartController(PsaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Open the cart of the user
        /// </summary>
        /// <returns>View with cart elements</returns>
        public IActionResult OpenCart()
        {
            var userId = 1; // hardcoded user id

            var cart = _context.Carts.FirstOrDefault(ca => ca.FkClient == userId); // get the cart of the user

            if (cart == null) // if cart is empty return epmty view
            {
                return View(null); // return empty view
            }

            List<CartElement> cartElements = _context.CartElements // if cart is not empty get the cart elements
                .Include(ce => ce.FkComputerPartNavigation)
                .Where(ce => ce.FkCart == cart.Id)
                .ToList();


            return View(cartElements); // return view with cart elements
        }
    }
}
