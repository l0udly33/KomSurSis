using ComputerBuildSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Mysqlx.Session;


namespace ComputerBuildSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly PsaContext _context;

        public OrderController(PsaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult OpenOrder(decimal totalAmount)
        {
            ViewBag.TotalAmount = totalAmount;
            return View("Payment");
        }

        public void UpdateOrder(int Id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == Id);

            order.FkState = 1;
            _context.SaveChanges();
        }
    }
}

