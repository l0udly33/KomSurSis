using ComputerBuildSystem.Models;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComputerBuildSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaypalAPI _payPalApi;
        private readonly PsaContext _context;

        public PaymentController(PaypalAPI payPalApi, PsaContext context)
        {
            _payPalApi = payPalApi;
            _context = context;
        }

        

        [HttpPost]
        public IActionResult Payment(decimal totalAmount)
        {
            var paymentResult = _payPalApi.PaymentStatus(totalAmount, Url.Action("CheckStatus", "Payment", null, Request.Scheme), Url.Action("CheckStatus", "Payment", null, Request.Scheme));
            if (paymentResult.IsSuccess)
            {
                return Redirect(paymentResult.RedirectUrl);
            }
            else
            {
                ModelState.AddModelError("", "Payment failed.");
            }
            ViewBag.TotalAmount = totalAmount;
            return View("~/Views/Order/Payment.cshtml");
        }

        [HttpGet]
        public IActionResult CheckStatus(string paymentId, string token, string PayerID)
        {
            var result = false;
            if (paymentId != null)
            {
                result = _payPalApi.ExecutePayment(paymentId, PayerID);
            }
            if (result)
            {
                var userId = 1;
                var stateCreated = 3;
                var client = _context.Clients.FirstOrDefault(c => c.IdUser == userId);
                var cart = _context.Carts.FirstOrDefault(ct => ct.FkClient == userId);


                var order = _context.Orders.FirstOrDefault();
                var payment = _context.Payments.FirstOrDefault();

                if(payment != null)
                {
                    _context.Payments.Remove(payment);
                    _context.SaveChanges();
                }

                if (order != null) {
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                }

                order = new Models.Order
                {
                    Date = DateTime.Now,
                    Sum = cart.Sum,
                    FkState = stateCreated,
                    FkCart = cart.Id,
                    FkClient = userId,
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                payment = new Models.Payment
                {
                    Date = DateTime.Now,
                    Sum = cart.Sum,
                    Iban = PayerID,
                    FkOrder = order.Id
                };

                _context.Payments.Add(payment);
                _context.SaveChanges();

                if (client != null)
                {
                    client.LoyaltyPoints += 100;
                    _context.SaveChanges();
                }

                var orderController = new OrderController(_context);
                orderController.UpdateOrder(order.Id);

                string Pavyko = "Payment successful 🤑";
                ViewBag.Success = Pavyko;
                return View("~/Views/Order/Payment.cshtml");
            }
            else
            {
                return View("~/Views/Order/CreateOrder.cshtml");
            }
        }
    }
}
