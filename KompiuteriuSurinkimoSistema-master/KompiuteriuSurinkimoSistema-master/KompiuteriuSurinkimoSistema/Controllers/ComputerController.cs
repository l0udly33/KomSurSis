using ComputerBuildSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Web;

namespace ComputerBuildSystem.Controllers
{
    public class ComputerController : Controller
    {
        private readonly PsaContext _context;
        private readonly BenchmarkAPI _benchmarkAPI;

        public ComputerController(PsaContext context, BenchmarkAPI benchmarkAPI)
        {
            _context = context;
            _benchmarkAPI = benchmarkAPI;
        }

        public IActionResult Open()
        {
            var parts = _context.ComputerParts
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.Ram)
                .Include(p => p.Motherboard)
                .Include(p => p.Psu)
                .Include(p => p.Case)
                .Include(p => p.HardDisk);

            var sessionIds = HttpContext.Session.GetString("BuildParts");
            List<int> buildIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>();

            List<ComputerPart> buildParts = parts.Where(p => buildIds.Contains(p.Id)).ToList();

            ViewBag.BuildParts = buildParts;

            return View("BuildComputer", parts);
        }

        public IActionResult AddPart(string id)
        {
            int partId = int.Parse(id);
            var part = _context.ComputerParts
                   .Include(p => p.Cpu)
                   .Include(p => p.Gpu)
                   .Include(p => p.Ram)
                   .Include(p => p.Motherboard)
                   .Include(p => p.Psu)
                   .Include(p => p.Case)
                   .Include(p => p.HardDisk)
                   .FirstOrDefault(p => p.Id == partId);

            if (part != null)
            {
                var sessionIds = HttpContext.Session.GetString("BuildParts");
                List<int> buildIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>();

                if (!buildIds.Contains(part.Id))
                {
                    bool canBeAdded = _context.ComputerParts.Where(p => buildIds.Contains(p.Id)).Where(p => p.Type == part.Type).ToList().Count == 0;

                    if (canBeAdded)
                    {
                        buildIds.Add(part.Id);
                        HttpContext.Session.SetString("BuildParts", String.Join(",", buildIds));
                    }
                }
            }

            return RedirectToAction("Open");
        }

        public IActionResult RemovePart(string id)
        {
            int partId = int.Parse(id);

            var sessionIds = HttpContext.Session.GetString("BuildParts");

            List<int> buildIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>();

            if (buildIds.Contains(partId))
            {
                buildIds.Remove(partId);

                if (buildIds.Count > 0)
                {
                    HttpContext.Session.SetString("BuildParts", String.Join(",", buildIds));
                }
                else
                {
                    HttpContext.Session.Remove("BuildParts");
                }

            }

            return RedirectToAction("Open");
        }

        public IActionResult AddPartsToCart()
        {
            int userId = 1;
            var sessionIds = HttpContext.Session.GetString("BuildParts");

            List<int> buildIds = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>();

            var cart = _context.Carts
                .Include(c => c.Order)
                .Where(c => c.FkClient == userId && c.Order == null)
                .OrderByDescending(c => c.Date)
                .FirstOrDefault();

            if (cart == null)
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

            foreach (var id in buildIds)
            {
                var part = _context.ComputerParts.Where(p => p.Id == id).FirstOrDefault();

                var cartElement = _context.CartElements
               .Where(ce => ce.FkCart == cart.Id && ce.FkComputerPart == id)
               .FirstOrDefault();

                if (cartElement != null)
                {
                    cartElement.Quantity++;
                    _context.Update(cartElement);
                    _context.SaveChanges();
                    cart.Sum += part.Price;
                }

                else if (part != null)
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
            }
            _context.Update(cart);
            _context.SaveChanges();

            HttpContext.Session.Remove("BuildParts");

            return RedirectToAction("Open");
        }
        /// <summary>
        /// OpenPartCriteria method is used to get the criteria for the selected part type
        /// </summary>
        /// <param name="type">computer part type</param>
        /// <returns>criteria as json</returns>
        [HttpGet]
        public JsonResult OpenPartCriteria(string type)
        {
            List<ComputerPart> computerParts;

            switch (type.ToLower())
            {
                case "case":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Case)
                                            .ToList();
                    break;
                case "cpu":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Cpu)
                                            .ToList();
                    break;
                case "gpu":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Gpu)
                                            .ToList();
                    break;
                case "hard disk":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.HardDisk)
                                            .ToList();
                    break;
                case "psu":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Psu)
                                            .ToList();
                    break;
                case "ram":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Ram)
                                            .ToList();
                    break;
                case "motherboard":
                    computerParts = _context.ComputerParts
                                            .Where(p => p.Type.ToLower() == type)
                                            .Include(p => p.Motherboard)
                                            .ToList();
                    break;
                default:
                    computerParts = new List<ComputerPart>();
                    break;
            }

            var criteria = FilterAvailableCriteria(computerParts, type); // get available criteria for the selected part type
            return Json(criteria); // return the criteria as a json object

        }

        /// <summary>
        /// for computer part type get its criteria from sent computer parts
        /// </summary>
        /// <param name="parts">Computer Parts</param>
        /// <param name="type">Computer part type</param>
        /// <returns>criteria for computer part type</returns>
        public Dictionary<string, List<string>> FilterAvailableCriteria(List<ComputerPart> parts, string type)
        {
            if (parts == null || parts.Count == 0)
            {
                return null;
            }

            Dictionary<string, List<string>> criteria = new Dictionary<string, List<string>>();

            switch (type.ToLower())
            {
                case "case":
                    var cases = parts.Select(p => p.Case).Where(c => c != null).ToList();
                    if (cases.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("color", cases.Select(c => c.Color).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("dimensions", cases.Select(c => c.Dimensions).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("standards", cases.Select(c => c.Standarts).Distinct().OrderBy(c => c).ToList());
                    break;
                case "cpu":
                    var cpus = parts.Select(p => p.Cpu).Where(c => c != null).ToList();
                    if (cpus.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("memory", cpus.Select(c => c.Memory).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("connection", cpus.Select(c => c.Connection).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("cores", cpus.Select(c => c.Cores.ToString()).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("frequency", cpus.Select(c => c.Frequency.ToString()).Distinct().OrderBy(c => c).ToList());
                    criteria.Add("power", cpus.Select(c => c.Power.ToString()).Distinct().OrderBy(c => c).ToList());
                    break;
                case "gpu":
                    var gpus = parts.Select(p => p.Gpu).Where(g => g != null).ToList();
                    if (gpus.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("memory", gpus.Select(g => g.Memory.ToString()).Distinct().OrderBy(g => g).ToList());
                    criteria.Add("memory type", gpus.Select(g => g.MemoryType).Distinct().OrderBy(g => g).ToList());
                    criteria.Add("connection", gpus.Select(g => g.Connection).Distinct().OrderBy(g => g).ToList());
                    criteria.Add("RAM type", gpus.Select(g => g.RamType).Distinct().OrderBy(g => g).ToList());
                    criteria.Add("power", gpus.Select(g => g.Power.ToString()).Distinct().OrderBy(g => g).ToList());
                    break;
                case "hard disk":
                    var hardDisks = parts.Select(p => p.HardDisk).Where(h => h != null).ToList();
                    if (hardDisks.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("type", hardDisks.Select(h => h.Type).Distinct().OrderBy(h => h).ToList());
                    criteria.Add("connection", hardDisks.Select(h => h.Connection).Distinct().OrderBy(h => h).ToList());
                    criteria.Add("reading speed", hardDisks.Select(h => h.ReadingSpeed.ToString()).Distinct().OrderBy(h => h).ToList());
                    criteria.Add("writing speed", hardDisks.Select(h => h.WritingSpeed.ToString()).Distinct().OrderBy(h => h).ToList());
                    criteria.Add("capacity", hardDisks.Select(h => h.Capacity).Distinct().OrderBy(h => h).ToList());
                    break;
                case "psu":
                    var psus = parts.Select(p => p.Psu).Where(p => p != null).ToList();
                    if (psus.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("power", psus.Select(p => p.Power.ToString()).Distinct().OrderBy(p => p).ToList());
                    break;
                case "ram":
                    var rams = parts.Select(p => p.Ram).Where(r => r != null).ToList();
                    if (rams.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("type", rams.Select(r => r.Type).Distinct().OrderBy(r => r).ToList());
                    criteria.Add("frequency", rams.Select(r => r.Frequency.ToString()).Distinct().OrderBy(r => r).ToList());
                    criteria.Add("voltage", rams.Select(r => r.Voltage.ToString()).Distinct().OrderBy(r => r).ToList());
                    break;
                case "motherboard":
                    var motherboards = parts.Select(p => p.Motherboard).Where(m => m != null).ToList();
                    if (motherboards.Count == 0)
                    {
                        return null;
                    }
                    criteria.Add("maximum memory frequency", motherboards.Select(m => m.MaximumMemoryFrequency.ToString()).Distinct().OrderBy(m => m).ToList());
                    criteria.Add("memory standard", motherboards.Select(m => m.MemoryStandart).Distinct().OrderBy(m => m).ToList());
                    criteria.Add("maximum amount of memory", motherboards.Select(m => m.MaximumAmountOfMemory.ToString()).Distinct().OrderBy(m => m).ToList());
                    criteria.Add("CPU socket", motherboards.Select(m => m.CpuSocket).Distinct().OrderBy(m => m).ToList());
                    criteria.Add("GPU socket", motherboards.Select(m => m.GpuSocket).Distinct().OrderBy(m => m).ToList());
                    criteria.Add("memory connection", motherboards.Select(m => m.MemoryConnection).Distinct().OrderBy(m => m).ToList());
                    break;
            }

            return criteria;
        }

        /// <summary>
        /// StartSelection method is used to execute the selection of computer parts
        /// </summary>
        /// <param name="selectedParts">Selected computer parts from BuildComputer view</param>
        /// <param name="selectedCriteria">Selected criteria from Build Computer view</param>
        /// <returns>message</returns>
        [HttpPost]
        public string StartSelection(string selectedParts, string selectedCriteria) 
        {
            List<ComputerPart> selectedPartsList = new List<ComputerPart>(); // new empty list of selected parts

            var partIds = JsonConvert.DeserializeObject<List<int?>>(selectedParts); // get selected parts ids from json
            if (partIds == null || partIds.Count == 0)
            {
                partIds = new List<int?>(); // if no parts are selected create new empty list
            }

            var criteria = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(selectedCriteria); // get selected criteria from json

            CompatibilityController compatibilityController = new CompatibilityController(_context); // create new compatibility controller

            if (partIds.Count > 1) // if there are selected parts we check compatibility
            {
                var compatibleParts = compatibilityController.CheckCompatibility(partIds); // check compatibility of selected parts

                var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                var jsonObj = JObject.Parse(jsonString);
                string message = (string)jsonObj["message"]; // deserialize the message from json


                if (!message.Equals("All parts are compatible")) // if not all parts are compatible return message
                {
                    return "Your selected parts are not compatible";
                }
            }

            selectedPartsList = _context.ComputerParts.Where(cp => partIds.Contains(cp.Id)) // Create Selected parts list
                   .Include(p => p.Cpu)
                   .Include(p => p.Gpu)
                   .Include(p => p.Ram)
                   .Include(p => p.Motherboard)
                   .Include(p => p.Psu)
                   .Include(p => p.Case)
                   .Include(p => p.HardDisk).ToList();
        

            Dictionary<string, bool> partTypes = GetPartTypesForComputer(selectedPartsList.Select(p => p.Type).Distinct().ToList()); // Get part types that are nescecarry for computer

            var partTypeToFilter = ChosePartTypeToFilter(partTypes); // Chose first part type to filter

            int i = 0; // counter

            while (!CheckIfPartListIsFull(selectedPartsList, partTypes.Count) && i<10) // while selected parts list is not full and counter is less than 10
            {
                List<ComputerPart> filteredParts;

                if (criteria != null && criteria.Keys.Contains(partTypeToFilter.ToUpper())) // if criteria for type is chosen
                {
                    filteredParts = FilterParts(criteria[partTypeToFilter.ToUpper()], partTypeToFilter); // filter parts by criteria
                }
                else // if criteria for type is not chosen
                {
                    filteredParts = _context.ComputerParts.Where(p => p.Type == partTypeToFilter).ToList(); // get all parts of that type
                    if (filteredParts.Count == 0)
                    {
                        return "There Are no parts to build a computer left"; // if there are no parts needed for computer return message
                    }
                }

                while(filteredParts.Count != 0) // while there are filtered parts
                {
                    ComputerPart part = ChoseRandomPart(filteredParts); //chose random part
                    filteredParts.Remove(part); // remove part from filtered parts

                    AddPartToSelectedPartsList(selectedPartsList, part); // add part to selected parts list

                    var compatibleParts = compatibilityController.CheckCompatibility(selectedPartsList.Select(p => (int?)p.Id).ToList()); // check if added part is compatible with other parts

                    var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                    var jsonObj = JObject.Parse(jsonString);
                    var message = (string)jsonObj["message"]; // deserialize message from json


                    if (message.Equals("All parts are compatible")) // if added part is compatible with other parts
                    {
                        MarkPartTypeAsChecked(partTypes, partTypeToFilter); // mark part type as checked
                        partTypeToFilter = ChosePartTypeToFilter(partTypes); // chose next part type to filter
                        break; // break the loop
                    }
                    else // if added part is not compatible with other parts
                    {
                        RemovePartFromSelectedPartsList(selectedPartsList, part); // remove part from selected parts list 
                        if (filteredParts.Count == 0) // if no parts are left to check it means the computer was not built
                        {
                            selectedPartsList = _context.ComputerParts.Where(cp => partIds.Contains(cp.Id)) // reset selected parts list
                             .Include(p => p.Cpu)
                             .Include(p => p.Gpu)
                             .Include(p => p.Ram)
                             .Include(p => p.Motherboard)
                             .Include(p => p.Psu)
                             .Include(p => p.Case)
                             .Include(p => p.HardDisk).ToList();

                            partTypes = GetPartTypesForComputer(selectedPartsList.Select(p => p.Type).Distinct().ToList()); // reset part types

                            partTypeToFilter = ChosePartTypeToFilter(partTypes); // reset part type to filter
                            i++; // increase counter
                            break; // break the loop
                        }
                    }

                }           
            }

            if(i == 10) // if counter is 10 it means that computer was not built and the there are no parts available to build computer
            {
                return "Computer was not built and the there are no parts available to build computer";
            }

            var buildPartsIds = string.Join(",", selectedPartsList.Select(p => p.Id));
            HttpContext.Session.SetString("BuildParts", buildPartsIds);

            return "Success:" + JsonConvert.SerializeObject(selectedPartsList.Select(p => p.Name).ToList()); // computer was built, show the selected parts
        }

        /// <summary>
        /// Check if Part List is full
        /// </summary>
        /// <param name="selectedParts">list with selected parts</param>
        /// <param name="n">maximum selected parts count</param>
        /// <returns>true if equal, false if not</returns>
        public bool CheckIfPartListIsFull(List<ComputerPart> selectedParts, int n)
        {
            return selectedParts.Count == n;
        }

        /// <summary>
        /// Remove part from selected parts list
        /// </summary>
        /// <param name="selectedPartsList">selected parts list</param>
        /// <param name="selectedPart">selected part which is set to be removed</param>
        public void RemovePartFromSelectedPartsList(List<ComputerPart> selectedPartsList, ComputerPart selectedPart)
        {
            selectedPartsList.Remove(selectedPart);
        }

        /// <summary>
        /// Check if part types, needed for computer, are all checked
        /// </summary>
        /// <param name="partTypes">part type and boolean if it was checked or not</param>
        /// <returns>if part types, needed for computer, are all checked, return true</returns>
        public bool CheckIfAllPartsAreChecked(Dictionary<string, bool> partTypes)
        {
            return partTypes.Where(pt => pt.Value == false).Count() == 0;
        }

        /// <summary>
        /// Mark part type as checked
        /// </summary>
        /// <param name="PartTypes">part type and boolean if it was checked or not</param>
        /// <param name="partTypeToFilter">part type which was checked</param>
        public void MarkPartTypeAsChecked(Dictionary<string, bool> PartTypes, string partTypeToFilter)
        {
            PartTypes[partTypeToFilter] = true;
        }

        /// <summary>
        /// Add part to selected parts list
        /// </summary>
        /// <param name="selectedPartsList"> selected Parts list</param>
        /// <param name="part">part which needs to be added</param>
        public void AddPartToSelectedPartsList(List<ComputerPart> selectedPartsList, ComputerPart part)
        {
            selectedPartsList.Add(part);
        }

        /// <summary>
        /// Chose random part from list
        /// </summary>
        /// <param name="parts">parts list</param>
        /// <returns>part</returns>
        public ComputerPart ChoseRandomPart(List<ComputerPart> parts)
        {
            Random random = new Random();
            int index = random.Next(0, parts.Count);
            ComputerPart part = parts[index];
            return part;
        }

        /// <summary>
        /// Filter parts by criteria
        /// </summary>
        /// <param name="criteria">criteria</param>
        /// <param name="partType">part type</param>
        /// <returns>parts which fit criteria</returns>
        public List<ComputerPart> FilterParts(Dictionary<string, List<string>> criteria, string partType)
        {

            List<ComputerPart> partsByType = _context.ComputerParts.Where(p => p.Type == partType)
                   .Include(p => p.Cpu)
                   .Include(p => p.Gpu)
                   .Include(p => p.Ram)
                   .Include(p => p.Motherboard)
                   .Include(p => p.Psu)
                   .Include(p => p.Case)
                   .Include(p => p.HardDisk).ToList();

            List<ComputerPart> filteredParts = new List<ComputerPart>();

            switch (partType.ToLower())
            {
                case "case":
                    if (criteria.ContainsKey("color"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["color"].Contains(p.Case.Color)).ToList());
                    }
                    if (criteria.ContainsKey("dimensions"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["dimensions"].Contains(p.Case.Dimensions)).ToList());
                    }
                    if (criteria.ContainsKey("standards"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["standards"].Contains(p.Case.Standarts)).ToList());
                    }
                    break;

                case "cpu":
                    if (criteria.ContainsKey("memory"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["memory"].Contains(p.Cpu.Memory)).ToList());
                    }
                    if (criteria.ContainsKey("connection"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["connection"].Contains(p.Cpu.Connection)).ToList());
                    }
                    if (criteria.ContainsKey("cores"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["cores"].Contains(p.Cpu.Cores.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("frequency"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["frequency"].Contains(p.Cpu.Frequency.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("power"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["power"].Contains(p.Cpu.Power.ToString())).ToList());
                    }
                    break;
                case "gpu":
                    if (criteria.ContainsKey("memory"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["memory"].Contains(p.Gpu.Memory.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("memory type"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["memory type"].Contains(p.Gpu.MemoryType)).ToList());
                    }
                    if (criteria.ContainsKey("connection"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["connection"].Contains(p.Gpu.Connection)).ToList());
                    }
                    if (criteria.ContainsKey("RAM type"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["RAM type"].Contains(p.Gpu.RamType)).ToList());
                    }
                    if (criteria.ContainsKey("power"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["power"].Contains(p.Gpu.Power.ToString())).ToList());
                    }
                    break;
                case "hard disk":
                    if (criteria.ContainsKey("type"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["type"].Contains(p.HardDisk.Type)).ToList());
                    }
                    if (criteria.ContainsKey("connection"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["connection"].Contains(p.HardDisk.Connection)).ToList());
                    }
                    if (criteria.ContainsKey("reading speed"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["reading speed"].Contains(p.HardDisk.ReadingSpeed.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("writing speed"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["writing speed"].Contains(p.HardDisk.WritingSpeed.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("capacity"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["capacity"].Contains(p.HardDisk.Capacity)).ToList());
                    }
                    break;
                case "psu":
                    if (criteria.ContainsKey("power"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["power"].Contains(p.Psu.Power.ToString())).ToList());
                    }
                    break;
                case "ram":
                    if (criteria.ContainsKey("type"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["type"].Contains(p.Ram.Type)).ToList());
                    }
                    if (criteria.ContainsKey("frequency"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["frequency"].Contains(p.Ram.Frequency.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("voltage"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["voltage"].Contains(p.Ram.Voltage.ToString())).ToList());
                    }
                    break;
                case "motherboard":
                    if (criteria.ContainsKey("maximum memory frequency"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["maximum memory frequency"].Contains(p.Motherboard.MaximumMemoryFrequency.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("memory standard"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["memory standard"].Contains(p.Motherboard.MemoryStandart)).ToList());
                    }
                    if (criteria.ContainsKey("maximum amount of memory"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["maximum amount of memory"].Contains(p.Motherboard.MaximumAmountOfMemory.ToString())).ToList());
                    }
                    if (criteria.ContainsKey("CPU socket"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["CPU socket"].Contains(p.Motherboard.CpuSocket)).ToList());
                    }
                    if (criteria.ContainsKey("GPU socket"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["GPU socket"].Contains(p.Motherboard.GpuSocket)).ToList());
                    }
                    if (criteria.ContainsKey("memory connection"))
                    {
                        filteredParts.AddRange(partsByType.Where(p => criteria["memory connection"].Contains(p.Motherboard.MemoryConnection)).ToList());
                    }
                    break;
            }

            return filteredParts.DistinctBy(p => p.Id).ToList();


        }


        /// <summary>
        /// Get Computer parts which are needed to build a computer and check already selected part types
        /// </summary>
        /// <param name="selectedPartTypes">Selected Part types</param>
        /// <returns>Computer parts which are needed to build a computer and check already selected part types</returns>
        public Dictionary<string, bool> GetPartTypesForComputer(List<string> selectedPartTypes)
        {
            Dictionary<string, bool> partTypes = new Dictionary<string, bool>();

            string[] partTypesList = { "motherboard", "cpu", "gpu", "ram", "hard disk", "case", "psu" };

            foreach(var partType in partTypesList)
            {
                partTypes.Add(partType, false);              
            }
            selectedPartTypes.ForEach(pt => partTypes[pt.ToLower()] = true);

            return partTypes;

        }

        /// <summary>
        /// Get first Computer part type to filter
        /// </summary>
        /// <param name="partTypes">part type and boolean if it was checked or not</param>
        /// <returns></returns>
        public string ChosePartTypeToFilter(Dictionary<string, bool> partTypes) 
        {
            return partTypes.Where(pt => pt.Value == false).FirstOrDefault().Key;
        }

        /// <summary>
        /// Get Selected Parts List
        /// </summary>
        /// <param name="partIds">ids of selected parts</param>
        /// <returns>Selected Parts List</returns>
        public List<ComputerPart> CreateSelectedPartList(List<int> partIds)
        {
            return _context.ComputerParts.Where(cp => partIds.Contains(cp.Id))
                   .Include(p => p.Cpu)
                   .Include(p => p.Gpu)
                   .Include(p => p.Ram)
                   .Include(p => p.Motherboard)
                   .Include(p => p.Psu)
                   .Include(p => p.Case)
                   .Include(p => p.HardDisk).ToList();
        }

        /// <summary>
        /// Save method is used to not delete the selected parts from session
        /// </summary>
        /// <returns></returns>
        public IActionResult Save()
        {
            return RedirectToAction("Open");
        }

        /// <summary>
        /// Delete method is used to delete the selected parts from session
        /// </summary>
        /// <returns></returns>
        public IActionResult Delete()
        {
            HttpContext.Session.Remove("BuildParts");
            return RedirectToAction("Open");
        }

        /// <summary>
        /// StartComputerBuilding method is used to start computer building by game
        /// </summary>
        /// <param name="type">game id</param>
        /// <returns>message</returns>
        [HttpPost]
        public string StartComputerBuilding(string type)
        {
            List<int?> partIdsTest = new List<int?>(); // create list of part ids
            CompatibilityController compatibilityController = new CompatibilityController(_context); // create new compatibility controller

            string data = SteamAPI.GetGameData(type); // Get selected game data
            data = RemoveTags(data); // remove unnecessary things from data
            string[] lines = data.Split(':'); // split data into lines

            string minCpu = GetMinCpu(3, lines, type); // get min cpu
            var cpuScore = _benchmarkAPI.SendCPU(minCpu); // get cpu score

            string minGpu = GetMinGpu(5, lines, type); // get min gpu
            var gpuScore = _benchmarkAPI.SendGPU(minGpu); // get gpu score

            string minDiskSize = GetMinDiskSize(7, lines); // get min disk size

            string minRamSize = GetMinRamSize(4, lines); // get min ram size

            List<ComputerPart> firstPartList = new List<ComputerPart>(); // Empty list of parts

            List<int?> partIds = new List<int?>(); // create list of part ids

            List<ComputerPart> cpuList = new List<ComputerPart>(); // create new cpu list
            cpuList = SelectAllTypeParts("CPU"); // get all cpu's
            for (int i = 0; i < cpuList.Count; i++)
            {
                var testCpuScore = _benchmarkAPI.SendCPU(cpuList[i].Name); // check cpu score
                if (testCpuScore > cpuScore)
                {
                    AddtoList(firstPartList, cpuList[i]); // add cpu to list
                    break;
                }
            }

            List<ComputerPart> gpuList = new List<ComputerPart>(); // create new gpu list
            gpuList = SelectAllTypeParts("GPU"); // get all gpu's
            for (int i = 0; i < gpuList.Count; i++)
            {
                var testCpuScore = _benchmarkAPI.SendGPU(gpuList[i].Name); // check gpu score
                if (testCpuScore > gpuScore)
                {
                    AddtoList(firstPartList, gpuList[i]); // add gpu to list
                    break;
                }
            }

            List<ComputerPart> motherboardList = new List<ComputerPart>(); // create new motherboard list
            motherboardList = SelectAllTypeParts("Motherboard"); // get all motherboards
            if (CheckIfThereAreParts(motherboardList)) // check if its not empty
            {
                return "There are no motherboards";
            }
            for (int i = 0; i < motherboardList.Count; i++)
            {
                partIdsTest.Clear();
                foreach (var first in firstPartList)
                {
                    partIdsTest.Add(first.Id);
                }
                partIdsTest.Add(motherboardList[i].Id);
                var compatibleParts = compatibilityController.CheckCompatibility(partIdsTest); // check motherboard compatibility
                var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                var jsonObj = JObject.Parse(jsonString);
                string message = (string)jsonObj["message"];
                if (message.Equals("All parts are compatible"))
                {
                    AddtoList(firstPartList, motherboardList[i]); // add motherboard to list
                    break;
                }
            }

            List<ComputerPart> ramList = new List<ComputerPart>(); // create new ram list
            ramList = SelectAllTypeParts("RAM"); // get all rams
            if (CheckIfThereAreParts(ramList)) // check if not empty
            {
                return "There are no rams";
            }
            for (int i = 0; i < ramList.Count; i++)
            {
                if (CheckIfSizeIsEnough(ramList[i], minRamSize))
                {
                    partIdsTest.Clear();
                    foreach (var first in firstPartList)
                    {
                        Console.WriteLine(first.Id);
                        partIdsTest.Add(first.Id);
                    }
                    partIdsTest.Add(ramList[i].Id);
                    var compatibleParts = compatibilityController.CheckCompatibility(partIdsTest); // check ram compatibility
                    var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                    var jsonObj = JObject.Parse(jsonString);
                    string message = (string)jsonObj["message"];
                    if (message.Equals("All parts are compatible"))
                    {
                        AddtoList(firstPartList, ramList[i]); // add ram to list
                        break;
                    }
                }
            }

            List<ComputerPart> diskList = new List<ComputerPart>();
            diskList = SelectAllTypeParts("Hard disk");
            if (CheckIfThereAreParts(diskList))
            {
                return "There are no disks";
            }
            for (int i = 0; i < diskList.Count; i++)
            {
                if (CheckIfSizeIsEnough(diskList[i], minDiskSize))
                {
                    partIdsTest.Clear();
                    foreach (var first in firstPartList)
                    {
                        Console.WriteLine(first.Id);
                        partIdsTest.Add(first.Id);
                    }
                    partIdsTest.Add(diskList[i].Id);
                    var compatibleParts = compatibilityController.CheckCompatibility(partIdsTest);
                    var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                    var jsonObj = JObject.Parse(jsonString);
                    string message = (string)jsonObj["message"];
                    if (message.Equals("All parts are compatible"))
                    {
                        AddtoList(firstPartList, diskList[i]);
                        break;
                    }
                }
            }

            List<ComputerPart> psuList = new List<ComputerPart>();
            psuList = SelectAllTypeParts("PSU");
            if (CheckIfThereAreParts(psuList))
            {
                return "There are no psus";
            }
            for (int i = 0; i < psuList.Count; i++)
            {
                partIdsTest.Clear();
                foreach (var first in firstPartList)
                {
                    Console.WriteLine(first.Id);
                    partIdsTest.Add(first.Id);
                }
                partIdsTest.Add(psuList[i].Id);
                var compatibleParts = compatibilityController.CheckCompatibility(partIdsTest);
                var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                var jsonObj = JObject.Parse(jsonString);
                string message = (string)jsonObj["message"];
                if (message.Equals("All parts are compatible"))
                {
                    AddtoList(firstPartList, psuList[i]);
                    break;
                }
            }

            List<ComputerPart> caseList = new List<ComputerPart>();
            caseList = SelectAllTypeParts("Case");
            if (CheckIfThereAreParts(psuList))
            {
                return "There are no cases";
            }
            for (int i = 0; i < caseList.Count; i++)
            {
                partIdsTest.Clear();
                foreach (var first in firstPartList)
                {
                    Console.WriteLine(first.Id);
                    partIdsTest.Add(first.Id);
                }
                partIdsTest.Add(caseList[i].Id);
                var compatibleParts = compatibilityController.CheckCompatibility(partIdsTest);
                var jsonString = JsonConvert.SerializeObject(compatibleParts.Value);
                var jsonObj = JObject.Parse(jsonString);
                string message = (string)jsonObj["message"];
                if (message.Equals("All parts are compatible"))
                {
                    AddtoList(firstPartList, caseList[i]);
                    break;
                }
            }


            foreach (var first in firstPartList)
            {
                partIds.Add(first.Id); // get all id's of items in list
            }

            var compatibleParts2 = compatibilityController.CheckCompatibility(partIds); // check compatiblity

            string compatiblePartsJson = JsonConvert.SerializeObject(compatibleParts2);

            var buildPartsIds = string.Join(",", firstPartList.Select(p => p.Id));
            HttpContext.Session.SetString("BuildParts", buildPartsIds);
            return "Success:" + JsonConvert.SerializeObject(firstPartList.Select(p => p.Name).ToList()); // show selected parts after done
        }

        /// <summary>
        /// RemoveTags method is used to RemoveTags from game data
        /// </summary>
        /// <param name="input">Data from game/param>
        /// <returns>data string</returns>
        static string RemoveTags(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        /// <summary>
        /// GetMinCpu method is used to get min cpu from data
        /// </summary>
        /// <param name="number">line where is cpu</param>
        /// <param name="lines">all data lines</param>
        /// <param name="type">type of game</param>
        /// <returns>name of cpu</returns>
        static string GetMinCpu(int number, string[] lines, string type)
        {
            string cpuname = "";
            if (number == 3)
            {
                string[] words = lines[number].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (type == "1091500")
                {
                    if (words.Length >= 2)
                    {
                        cpuname = words[0] + " " + words[1];
                    }
                }
                if (type == "1245620")
                {
                    if (words.Length >= 3)
                    {
                        cpuname = words[1] + " " + words[2];
                    }
                }
            }
            return cpuname;
        }

        /// <summary>
        /// GetMinGpu method is used to get min gpu from data
        /// </summary>
        /// <param name="number">line where is cpu</param>
        /// <param name="lines">all data lines</param>
        /// <param name="type">type of game</param>
        /// <returns>name of gpu</returns>
        static string GetMinGpu(int number, string[] lines, string type)
        {
            string gpuname = "";
            if (number == 5)
            {
                string[] words = lines[number].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (type == "1091500")
                {
                    if (words.Length >= 4)
                    {
                        gpuname = words[0] + " " + words[1] + " " + words[2] + " " + words[3];
                    }
                }
                if (type == "1245620")
                {
                    if (words.Length >= 5)
                    {
                        gpuname = words[1] + " " + words[2] + " " + words[3] + " " + words[4];
                    }
                }
            }
            return gpuname;
        }

        /// <summary>
        /// GetMinDiskSize method is used to get min disk size from data
        /// </summary>
        /// <param name="number">line where is cpu</param>
        /// <param name="lines">all data lines</param>
        /// <returns>name of disk</returns>
        static string GetMinDiskSize(int number, string[] lines)
        {
            string storagesize = "";
            int storageSizeGB = 0;
            if (number == 7)
            {
                string[] words = lines[number].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                storageSizeGB = int.Parse(words[0]);
                if (storageSizeGB < 500)
                {
                    if (words.Length >= 2)
                    {
                        storagesize = "500" + words[1];
                    }
                }
                else
                {
                    storagesize = "1TB";
                }
            }
            return storagesize;
        }

        /// <summary>
        /// GetMinRamSize method is used to get min ram size from data
        /// </summary>
        /// <param name="number">line where is ram</param>
        /// <param name="lines">all data lines</param>
        /// <returns>ram size</returns>
        static string GetMinRamSize(int number, string[] lines)
        {
            string ramsize = "";
            int ramsizeGB = 0;
            if (number == 4)
            {
                string[] words = lines[number].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                ramsizeGB = int.Parse(words[0]);
                if (ramsizeGB < 16)
                {
                    if (words.Length >= 2)
                    {
                        ramsize = "16GB";
                    }
                }
                else
                {
                    ramsize = "32GB";
                }
            }
            return ramsize;
        }

        /// <summary>
        /// SelectAllTypeParts method is used to get all parts of certain type
        /// </summary>
        /// <param name="name">part type</param>
        /// <returns>list</returns>
        public List<ComputerPart> SelectAllTypeParts(string name)
        {
            List<ComputerPart> cpuList = _context.ComputerParts.Where(p => p.Type == name).ToList();

            return cpuList;
        }

        /// <summary>
        /// CheckIfThereAreParts method is used to check if list is not empty
        /// </summary>
        /// <param name="partsList">list</param>
        /// <returns>true/false</returns>
        public bool CheckIfThereAreParts(List<ComputerPart> partsList)
        {
            return partsList.Count == 0;
        }

        /// <summary>
        /// CheckIfSizeIsEnough method is used to check if size is enough
        /// </summary>
        /// <param name="part">part to check</param>
        /// <param name="searchString">min size</param>
        /// <returns>true/false</returns>
        public bool CheckIfSizeIsEnough(ComputerPart part, string searchString)
        {
            if (part == null || searchString == null)
            {
                throw new ArgumentNullException("part and searchString cannot be null");
            }

            return part.Description != null && part.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// AddToList method is used to add part to list
        /// </summary>
        /// <param name="selectedPartsList">list to add to</param>
        /// <param name="part">part to add</param>
        public void AddtoList(List<ComputerPart> selectedPartsList, ComputerPart part)
        {
            selectedPartsList.Add(part);
        }
    }
}
