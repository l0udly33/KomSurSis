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
    public class PartController : Controller
    {

        private readonly PsaContext _context; // Database context
        private readonly BenchmarkAPI _benchmarkAPI; // Benchmark API

        /// <summary>
        /// Constructor
        /// </summary>
        public PartController(PsaContext context, BenchmarkAPI benchmarkAPI)
        {
            _context = context;
            _benchmarkAPI = benchmarkAPI;
        }

        public IActionResult OpenPartList()
        {
            var parts = _context.ComputerParts.ToList(); // Get all parts from the database

            var sessionIds = HttpContext.Session.GetString("GPUcompare"); // Get the ids of the parts that are being compared
            List<int> buildIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>(); // Convert the string to a list of integers

            List<ComputerPart> compareGPU = parts.Where(p => buildIds.Contains(p.Id)).ToList(); // Get the parts that are being compared

            HttpContext.Session.SetString("CompareGPU", JsonSerializer.Serialize(compareGPU)); // Save the parts that are being compared to the session

            ViewBag.CompareGPU = compareGPU; // Send the parts that are being compared to the view
            return View("PartList", parts);
        }

        public IActionResult Filter(string type, List<ComputerPart> parts)
        {
            if (string.IsNullOrEmpty(type))
            {
                parts = _context.ComputerParts.ToList(); // Get base part
            }
            else
            {
                parts = _context.ComputerParts.Where(p => p.Type == type).ToList(); // Get specific part with type
            }

            return View("PartList", parts);
        }


        public IActionResult OpenAddPart()
        {
            return View("AddPart");
        }

        // POST: Handle the form submission
        [HttpPost]
        public IActionResult AddPart(IFormCollection form)
        {

            if (!ValidatePartData(form)) // Validate the form data
            {
                ModelState.AddModelError("ErrorMessage", "No empty fields allowed."); // Add an error message
                return View(form);
            }

            var type = form["Type"]; // Get the type of the part
            var newPart = new ComputerPart // Create a new part
            {
                Name = form["Name"],
                Manufacturer = form["Manufacturer"],
                Model = form["Model"],
                Type = type,
                Price = double.Parse(form["Price"]),
                Quantity = int.Parse(form["Quantity"]),
                Description = form["Description"]
            };

            _context.ComputerParts.Add(newPart); // Add the part to the database

            switch (type) // Add the part to the correct table based on the type
            {
                case "GPU":
                    var gpu = new Gpu
                    {
                        Memory = int.Parse(form["Memory"]),
                        MemoryType = form["MemoryType"],
                        MemoryFrequency = int.Parse(form["Frequency"]),
                        Connection = form["Connection"],
                        RamQuantity = int.Parse(form["RamQuantity"]),
                        RamType = form["RamType"],
                        Power = int.Parse(form["Power"]),
                        Dimensions = form["Dimensions"],
                        IdNavigation = newPart
                    };
                    _context.Gpus.Add(gpu);
                    break;
                case "CPU":
                    var cpu = new Cpu
                    {
                        Memory = form["Memory"],
                        Connection = form["Connection"],
                        Cores = int.Parse(form["Cores"]),
                        Frequency = int.Parse(form["Frequency"]),
                        Power = int.Parse(form["Power"]),
                        IdNavigation = newPart
                    };
                    _context.Cpus.Add(cpu);
                    break;
                case "RAM":
                    var ram = new Ram
                    {
                        Type = form["RamType"],
                        Frequency = int.Parse(form["Frequency"]),
                        Voltage = int.Parse(form["Voltage"]),
                        Amount = int.Parse(form["Amount"]),
                        IdNavigation = newPart
                    };
                    _context.Rams.Add(ram);
                    break;
                case "Case":
                    var computerCase = new Case
                    {
                        Standarts = form["Standard"],
                        Dimensions = form["Dimensions"],
                        Color = form["Color"],
                        IdNavigation = newPart
                    };
                    _context.Cases.Add(computerCase);
                    break;
                case "PSU":
                    var psu = new Psu
                    {
                        Power = int.Parse(form["Power"]),
                        SizeStandart = form["Size"],
                        IdNavigation = newPart
                    };
                    _context.Psus.Add(psu);
                    break;
                case "Motherboard":
                    var motherboard = new Motherboard
                    {
                        MaximumMemoryFrequency = int.Parse(form["MaxFrequency"]),
                        MemoryStandart = form["RamStandard"],
                        MaximumAmountOfMemory = int.Parse(form["MaxMemory"]),
                        CpuSocket = form["CpuSocket"],
                        GpuSocket = form["GpuSocket"],
                        MemoryConnection = form["MemoryConnection"],
                        SizeStandart = form["SizeStandard"],
                        IdNavigation = newPart
                    };
                    _context.Motherboards.Add(motherboard);
                    break;
                case "Hard disk":
                    var hdd = new HardDisk
                    {
                        Type = form["DiskType"],
                        Capacity = form["Capacity"],
                        ReadingSpeed = int.Parse(form["ReadingSpeed"]),
                        WritingSpeed = int.Parse(form["WritingSpeed"]),
                        Connection = form["Connection"],
                        IdNavigation = newPart
                    };
                    _context.HardDisks.Add(hdd);
                    break;
            }

            _context.SaveChanges(); // Save the changes to the database
            return RedirectToAction("OpenPartList");
        }

        /// <summary>
        /// Validate the form data
        /// </summary>
        /// <param name="form">Form to valide part of</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidatePartData(IFormCollection form)
        {
            foreach (var key in form.Keys)
            {
                if (form[key] == "") // Check if any of the fields are empty
                    return false;
            }

            return true;
        }

        public IActionResult OpenPartInfo(int id)
        {
            var part = _context.ComputerParts
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.Ram)
                .Include(p => p.Motherboard)
                .Include(p => p.Psu)
                .Include(p => p.Case)
                .Include(p => p.HardDisk)
                .FirstOrDefault(x => x.Id == id); // Get the part with all part types from the database

            return View("PartInfo", part);
        }

        public IActionResult DeletePart(int id)
        {
            var part = _context.ComputerParts.Find(id); // Find the part in the database
            if (part == null)
            {
                return NotFound(); // Return not found if the part is not found
            }

            return Json(new { confirm = true, message = "Are you sure you want to delete this part?" }); // Return a confirmation message
        }

        public IActionResult Delete(int id)
        {
            _context.ComputerParts.Where(p => p.Id == id).ExecuteDelete(); // Delete the part from the database

            _context.SaveChanges();

            return RedirectToAction("OpenPartList");
        }

        public IActionResult OpenEditPart(int id)
        {
            var part = _context.ComputerParts
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.Ram)
                .Include(p => p.Motherboard)
                .Include(p => p.Psu)
                .Include(p => p.Case)
                .Include(p => p.HardDisk)
                .FirstOrDefault(m => m.Id == id); // Get the part with all part types from the database


            return View("EditPart", part);
        }

        [HttpPost]
        public IActionResult EditPart(ComputerPart part, IFormCollection form)
        {
            if (!ValidatePartData(form)) // Validate the form data
            {
                ModelState.AddModelError("ErrorMessage", "No empty fields allowed."); // Add an error message
                return View(part);
            }

            _context.Update(part); // Update the part in the database
            _context.SaveChanges();

            return RedirectToAction("OpenPartList"); // Redirect to the part list
        }

        public IActionResult GPUcompare(string id)
        {
            int partId = int.Parse(id);
            var part = _context.ComputerParts
                   .Include(p => p.Gpu)
                   .FirstOrDefault(p => p.Id == partId); // Get the part with the GPU type from the database

            if (part != null) // If the part is found
            {
                var sessionIds = HttpContext.Session.GetString("GPUcompare");
                List<int> compareIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>(); // Convert the string to a list of integers
                if (!compareIds.Contains(part.Id) && compareIds.Count() < 5)
                {
                    compareIds.Add(part.Id);
                    HttpContext.Session.SetString("GPUcompare", String.Join(",", compareIds)); // Save the ids of the parts that are being compared to the session
                }
            }

            return RedirectToAction("OpenPartList"); // Redirect to the part list
        }

        public IActionResult OpenGPU()
        {
            var sessionCriteria = HttpContext.Session.GetString("Criteria");
            if (sessionCriteria != null)
            {
                HttpContext.Session.Remove("Criteria");
            }
            var compareGpuJson = HttpContext.Session.GetString("CompareGPU");
            // Get the parts that are being compared from the session
            List<ComputerPart> compareGPU = compareGpuJson != null ? JsonSerializer.Deserialize<List<ComputerPart>>(compareGpuJson) : new List<ComputerPart>(); 

            ViewBag.CompareGPU = compareGPU; // Send the parts that are being compared to the view
            return View("GPUCompare");
        }

        public IActionResult BenchmarkOverallGPU()
        {
            return RedirectToAction("AddCriteria", new { source = "BenchmarkOverallGPU" });
        }

        public IActionResult BenchmarkPerEuroGPU()
        {
            return RedirectToAction("AddCriteria", new { source = "BenchmarkPerEuroGPU" });
        }


        public IActionResult PriceLimitGPU(decimal? priceLimit)
        {

            return RedirectToAction("AddCriteria", new { source = "PriceLimitGPU", priceLimit });
        }

        public IActionResult BenchmarkLimitGPU(decimal? benchmarkLimit)
        {
            return RedirectToAction("AddCriteria", new { source = "BenchmarkLimitGPU", benchmarkLimit });
        }


        public IActionResult AddCriteria(string source, decimal? priceLimit = null, decimal? benchmarkLimit = null)
        {
            var sessionCriteria = HttpContext.Session.GetString("Criteria");
            List<string> criteria = sessionCriteria != null ? sessionCriteria.Split(',').ToList() : new List<string>();

            if (source == "BenchmarkOverallGPU" && !criteria.Contains("benchmarkOverall"))
            {
                criteria.Add("benchmarkOverall");
            }
            if (source == "BenchmarkPerEuroGPU" && !criteria.Contains("benchmarkPerEuro"))
            {
                criteria.Add("benchmarkPerEuro");
            }

            if (priceLimit.HasValue)
            {
                criteria.Add($"priceLimit:{priceLimit}");
            }
            if (benchmarkLimit.HasValue)
            {
                criteria.Add($"benchmarkLimit:{benchmarkLimit}");
            }

            HttpContext.Session.SetString("Criteria", string.Join(",", criteria));

            var compareGpuJson = HttpContext.Session.GetString("CompareGPU");
            List<ComputerPart> compareGPU = compareGpuJson != null ? JsonSerializer.Deserialize<List<ComputerPart>>(compareGpuJson) : new List<ComputerPart>();

            ViewBag.CompareGPU = compareGPU;


            ViewBag.Criteria = criteria;

            return View("GPUCompare");
        }

        public IActionResult GPUResults()
        {
            var compareGpuJson = HttpContext.Session.GetString("CompareGPU");
            List<ComputerPart> compareGPU = compareGpuJson != null ? JsonSerializer.Deserialize<List<ComputerPart>>(compareGpuJson) : new List<ComputerPart>();

            var sessionCriteria = HttpContext.Session.GetString("Criteria");
            List<string> criteria = sessionCriteria != null ? sessionCriteria.Split(',').ToList() : new List<string>();

            var parts = compareGPU;

            var deviceNames = parts.Select(p => p.Name).ToList();
            var benchmarkScores = _benchmarkAPI.CompareGPUStatus(deviceNames);

            List<ComputerPart> missing = CheckGPUStatus(parts, benchmarkScores);
            if (missing.Count > 0)
            {
                ViewBag.Error = missing;
                ViewBag.CompareGPU = parts;
                return View("GPUCompare");
            }

            bool start = false;
            if (criteria.Count > 0)
            {
                start = true;
            }
            var price = new Regex(@"priceLimit:\d+");
            var benchmark = new Regex(@"benchmarkLimit:\d+");
            List<ComputerPart> benchmarkOverall = new List<ComputerPart>();
            List<ComputerPart> benchmarkPerEuro = new List<ComputerPart>();
            List<ComputerPart> intermediateOption = new List<ComputerPart>();
            Dictionary<int, List<ComputerPart>> outputData = new Dictionary<int, List<ComputerPart>>();
            while (start)
            {
                int flag = 0;
                if (criteria.Contains("benchmarkOverall"))
                {
                    benchmarkOverall = BenchmarkOverall(parts, benchmarkScores);
                    flag = 1;
                    outputData = SaveDataForOutput(benchmarkOverall, flag);
                }
                else if (criteria.Contains("benchmarkPerEuro"))
                {
                    benchmarkPerEuro = BenchmarkPerEuro(parts, benchmarkScores);
                    flag = 2;
                    outputData = SaveDataForOutput(benchmarkPerEuro, flag);
                }
                else if (criteria.Any(c => price.IsMatch(c)) && criteria.Any(c => benchmark.IsMatch(c)))
                {
                    intermediateOption = IntermediateOption(parts, benchmarkScores, criteria);
                    flag = 3;
                    outputData = SaveDataForOutput(intermediateOption, flag);
                }
                start = CheckCriteriaResults(flag, criteria);
            }

            ViewBag.CompareGPU = parts;
            ViewBag.BenchmarkScores = benchmarkScores;
            ViewBag.OutputData = outputData;
            HttpContext.Session.Remove("Criteria");
            return View("GPUCompare");
        }

        public List<ComputerPart> CheckGPUStatus(List<ComputerPart> parts, Dictionary<string, int> benchmarkScores)
        {
            List<ComputerPart> missing = new List<ComputerPart>();
            foreach (var part in parts)
            {
                if (!benchmarkScores.ContainsKey(part.Name))
                {
                    missing.Add(part);
                }
            }
            return missing;
        }

        public List<ComputerPart> BenchmarkOverall(List<ComputerPart> parts, Dictionary<string, int> benchmarkScores)
        {
            List<ComputerPart> benchmarkOveralls = new List<ComputerPart>();
            foreach (var part in parts)
            {
                part.Benchmark = benchmarkScores[part.Name] + "";
                benchmarkOveralls.Add(part);
            }
            benchmarkOveralls = benchmarkOveralls
                .OrderByDescending(part => int.TryParse(part.Benchmark, out int result) ? result : 0)
                .ToList();
            return benchmarkOveralls;
        }

        public List<ComputerPart> BenchmarkPerEuro(List<ComputerPart> parts, Dictionary<string, int> benchmarkScores)
        {
            List<ComputerPart> benchmarkPerEuro = new List<ComputerPart>();
            foreach (var part in parts)
            {
                part.Benchmark = benchmarkScores[part.Name] + "";
                benchmarkPerEuro.Add(part);
            }
            benchmarkPerEuro = CalculateBenchmarkPerEuro(benchmarkPerEuro);
            benchmarkPerEuro = benchmarkPerEuro
                .OrderByDescending(part => int.TryParse(part.Benchmark, out int result) ? result : 0)
                .ToList();
            return benchmarkPerEuro;
        }

        public List<ComputerPart> CalculateBenchmarkPerEuro(List<ComputerPart> benchmarkPerEuro)
        {
            List<ComputerPart> benchmarkPerEuroResult = new List<ComputerPart>();
            foreach (var part in benchmarkPerEuro)
            {
                var clonedPart = new ComputerPart
                {
                    Id = part.Id,
                    Name = part.Name,
                    Manufacturer = part.Manufacturer,
                    Model = part.Model,
                    Type = part.Type,
                    Price = part.Price,
                    Quantity = part.Quantity,
                    Description = part.Description,
                    Benchmark = part.Benchmark
                };
                clonedPart.Benchmark = Math.Round(int.Parse(clonedPart.Benchmark) / clonedPart.Price).ToString();

                benchmarkPerEuroResult.Add(clonedPart);
            }
            return benchmarkPerEuroResult;
        }

        public List<ComputerPart> IntermediateOption(List<ComputerPart> parts, Dictionary<string, int> benchmarkScores, List<String> criteria)
        {
            List<ComputerPart> intermediateOption = new List<ComputerPart>();
            List<double> averages = new List<double>();
            averages = Averages(parts, benchmarkScores);
            int priceLimit = 0;
            int benchmarkLimit = 0;
            foreach (var interim in criteria)
            {
                var criterion = interim.Split(':');
                if (criterion.Length == 2)
                {
                    if (criterion[0] == "priceLimit")
                    {
                        int parsedPriceLimit;
                        if (int.TryParse(criterion[1], out parsedPriceLimit))
                        {
                            priceLimit = parsedPriceLimit;
                        }
                    }
                    else if (criterion[0] == "benchmarkLimit")
                    {
                        int parsedBenchmarkLimit;
                        if (int.TryParse(criterion[1], out parsedBenchmarkLimit))
                        {
                            benchmarkLimit = parsedBenchmarkLimit;
                        }
                    }
                }
            }

            List<ComputerPart> GPUs = FindGPUsPrice(averages, priceLimit, parts);
            var deviceNames = GPUs.Select(p => p.Name).ToList();

            var newBenchmarkScores = _benchmarkAPI.CompareGPUStatus(deviceNames);

            List<ComputerPart> missing = CheckGPUStatus(GPUs, newBenchmarkScores);
            if (missing.Count > 0)
            {
                ViewBag.Error = missing;
                //ViewBag.CompareGPU = parts;
                return missing;
            }

            intermediateOption = FindGPUsBenchmark(GPUs, newBenchmarkScores, averages, benchmarkLimit);

            return intermediateOption;
        }

        public List<double> Averages(List<ComputerPart> parts, Dictionary<string, int> benchmarkScores)
        {
            List<double> averages = new List<double>();
            double sumPrice = 0;
            double sumBenchmark = 0;
            foreach (var part in parts)
            {
                sumPrice += part.Price;
                sumBenchmark += benchmarkScores[part.Name];
            }
            averages.Add(sumPrice / parts.Count);
            averages.Add(sumBenchmark / parts.Count);
            return averages;
        }

        public List<ComputerPart> FindGPUsPrice(List<double> averages, int priceLimit, List<ComputerPart> parts)
        {
            var existingPartIds = parts.Select(p => p.Id).ToList();

            List<ComputerPart> GPUs = _context.ComputerParts
                                              .Where(part => part.Type == "GPU"
                                                             && part.Price >= averages[0] - priceLimit
                                                             && part.Price <= averages[0] + priceLimit
                                                             && !existingPartIds.Contains(part.Id))
                                              .ToList();
            return GPUs;
        }

        public List<ComputerPart> FindGPUsBenchmark(List<ComputerPart> GPUs, Dictionary<string, int> newBenchmarkScores, List<double> averages, int benchmarkLimit)
        {
            List<ComputerPart> GPUsBenchmark = new List<ComputerPart>();
            foreach (var part in GPUs)
            {
                if (newBenchmarkScores[part.Name] >= averages[1] - benchmarkLimit && newBenchmarkScores[part.Name] <= averages[1] + benchmarkLimit)
                {
                    part.Benchmark = newBenchmarkScores[part.Name] + "";
                    GPUsBenchmark.Add(part);
                }
            }
            GPUsBenchmark = FindGPU(GPUsBenchmark);
            return GPUsBenchmark;
        }

        public List<ComputerPart> FindGPU(List<ComputerPart> GPUs)
        {
            List<ComputerPart> GPU = new List<ComputerPart>();
            int max = 0;
            ComputerPart interim = null;
            foreach (var part in GPUs)
            {
                if (int.Parse(part.Benchmark) > max)
                {
                    max = int.Parse(part.Benchmark);
                    interim = part;
                }
            }
            GPU.Add(interim);
            return GPU;
        }

        public bool CheckCriteriaResults(int flag, List<string> criteria)
        {
            if (flag == 0)
            {
                return false;
            }
            if (flag == 1)
            {
                criteria.Remove("benchmarkOverall");
            }
            if (flag == 2)
            {
                criteria.Remove("benchmarkPerEuro");
            }
            if (flag == 3)
            {
                criteria.Clear();
            }
            return true;
        }

        public Dictionary<int, List<ComputerPart>> SaveDataForOutput(List<ComputerPart> addition, int flag)
        {
            Dictionary<int, List<ComputerPart>> outputData = ViewBag.OutputData as Dictionary<int, List<ComputerPart>>;

            if (outputData == null)
            {
                outputData = new Dictionary<int, List<ComputerPart>>();
            }

            outputData[flag] = addition;

            ViewBag.OutputData = outputData;

            return outputData;
        }

        /// <summary>
        /// Add a part to the cart
        /// </summary>
        public IActionResult AddToCart(int id)
        {
            int userId = 1; // hardcoded user id

            var part = _context.ComputerParts
                   .Include(p => p.Cpu)
                   .Include(p => p.Gpu)
                   .Include(p => p.Ram)
                   .Include(p => p.Motherboard)
                   .Include(p => p.Psu)
                   .Include(p => p.Case)
                   .Include(p => p.HardDisk)
                   .FirstOrDefault(p => p.Id == id); // Get the part with all part types from the database


            var cart = _context.Carts
                .Include(c => c.Order)
                .Where(c => c.FkClient == userId && c.Order == null)
                .OrderByDescending(c => c.Date)
                .FirstOrDefault(); // Get the cart of the user

            if (cart == null) // if cart is empty create a new one
            {
                cart = new Cart
                {
                    Date = DateTime.Now,
                    Sum = 0,
                    FkClient = userId,
                };
                _context.Add(cart);
                _context.SaveChanges();
            }


            var cartElement = _context.CartElements
                .Where(ce => ce.FkCart == cart.Id && ce.FkComputerPart == id)
                .FirstOrDefault(); // Get the cart element with the part

            if (cartElement != null) // if the part is already in the cart increase the quantity
            {
                cartElement.Quantity++;
                _context.Update(cartElement);
                _context.SaveChanges();
                cart.Sum += part.Price;
            }
            else if (part != null) // if the part is not in the cart add it
            {
                 cartElement = new CartElement
                 {

                     Quantity = 1,
                     FkCart = cart.Id,
                     FkComputerPart = part.Id
                 };
                 _context.Add(cartElement);
                 cart.CartElements.Add(cartElement);
                 cart.Sum += part.Price;
            }
            _context.Update(cart);
            _context.SaveChanges(); // Save the changes to the database

            return RedirectToAction("OpenPartList"); // Redirect to the part list
        }
    }
}
