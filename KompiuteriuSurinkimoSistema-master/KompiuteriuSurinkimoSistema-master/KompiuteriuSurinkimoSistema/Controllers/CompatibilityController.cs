using ComputerBuildSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.RegularExpressions;

namespace ComputerBuildSystem.Controllers
{
    public class CompatibilityController : Controller
    {
        private readonly PsaContext _context; // database context

        private Dictionary<ComputerPart, List<ComputerPart>> incompatibilities = new Dictionary<ComputerPart, List<ComputerPart>>(); // dictionary to store incompatible parts

        private List<String> freePorts = new List<String>(); // list to store free ports

        /// <summary>
        /// Constructor
        /// </summary>
        public CompatibilityController(PsaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds incompatible part to the list of incompatible parts for the motherboard
        /// </summary>
        private void AddToMbIncompatibleList(ComputerPart motherboard, ComputerPart incompatiblePart)
        {
            if (incompatibilities.ContainsKey(motherboard)) // check if the motherboard is already in the dictionary
            {
                List<ComputerPart> incompatibleParts = incompatibilities[motherboard]; // get the list of incompatible parts

                incompatibleParts.Add(incompatiblePart);

                incompatibilities[motherboard] = incompatibleParts; // update the list of incompatible parts

            }
            else
            {
                List<ComputerPart> incompatibleParts = new List<ComputerPart> { incompatiblePart }; // create a new list with the incompatible part

                incompatibilities.Add(motherboard, incompatibleParts);
            }
        }

        /// <summary>
        /// Checks if the motherboard and CPU are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckMbCpuCompatibility(ComputerPart motherboard, ComputerPart cpu)
        {
            return motherboard.Motherboard.CpuSocket == cpu.Cpu.Connection; // check if the CPU socket is compatible with the motherboard
        }

        /// <summary>
        /// Checks if the motherboard and RAM are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckMbRamCompatibility(ComputerPart motherboard, ComputerPart ram)
        {
            return motherboard.Motherboard.MemoryStandart.Contains(ram.Ram.Type) // check if the memory type is compatible
                && motherboard.Motherboard.MaximumMemoryFrequency >= ram.Ram.Frequency  // check if the frequency is compatible
                && motherboard.Motherboard.MaximumAmountOfMemory >= ram.Ram.Amount; // check if the amount of memory is compatible
        }

        /// <summary>
        /// Checks if the motherboard and GPU are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckMbGpuCompatibility(ComputerPart motherboard, ComputerPart gpu)
        {
            return motherboard.Motherboard.GpuSocket == gpu.Gpu.Connection; // check if the GPU socket is compatible with the motherboard
        }

        /// <summary>
        /// Gets the free ports of the motherboard
        /// </summary>
        /// <returns>A list of free ports</returns>>
        private List<string> GetFreeMbPorts(ComputerPart motherboard)
        {
            if(freePorts.Count == 0) // if the list is empty
            {
                freePorts = motherboard.Motherboard.MemoryStandart.Split(',').Select(s => s.Trim()).ToList(); // get the ports from the motherboard
            }

            return freePorts;
        }

        /// <summary>
        /// Marks the port as taken
        /// </summary>
        private void MarkPortAsTaken(string portName)
        {
            freePorts.Remove(portName);
        }

        /// <summary>
        /// Calculates the wattage of the computer
        /// </summary>
        /// <returns>Wattage of the computer</returns>
        private int CalculateWattage(List<ComputerPart> parts)
        {
            int wattage = 0; // default wattage

            foreach (var part in parts)
            {
                if(part.Type == "CPU")
                {
                    var cpu = part.Cpu;

                    if(cpu == null) // if the CPU is not in the part object
                    {
                        cpu = _context.Cpus.Where(c => c.Id == part.Id).FirstOrDefault(); // get the CPU from the database
                    }

                    wattage += cpu.Power; // add the power of the CPU
                }
                else if(part.Type == "GPU")
                {
                    var gpu = part.Gpu;

                    if (gpu == null) // if the GPU is not in the part object
                    {
                        gpu = _context.Gpus.Where(c => c.Id == part.Id).FirstOrDefault(); // get the GPU from the database
                    }

                    wattage += gpu.Power; // add the power of the GPU
                }
            }

            return wattage;
        }

        /// <summary>
        /// Marks the PSU as incompatible
        /// </summary>
        private void MarkPsuIncompatible(ComputerPart psu)
        {
            if (!incompatibilities.ContainsKey(psu))
            {
                incompatibilities.Add(psu, new List<ComputerPart>());
            }
        }

        /// <summary>
        /// Checks if the case and motherboard are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckCaseMbCompatibility(ComputerPart computerCase, ComputerPart motherboard)
        {
            return computerCase.Case.Standarts.Contains(motherboard.Motherboard.SizeStandart); // check if the size standart is compatible
        }

        /// <summary>
        /// Checks if the case and PSU are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckCasePsuCompatibility(ComputerPart computerCase, ComputerPart psu)
        {
            return computerCase.Case.Standarts.Contains(psu.Psu.SizeStandart); // check if the size standart is compatible
        }

        /// <summary>
        /// Checks if the case and GPU are compatible
        /// </summary>
        /// <returns>True if compatible, false otherwise</returns>
        private bool CheckCaseGpuCompatibility(ComputerPart computerCase, ComputerPart gpu)
        {
            List<int> caseDimensions = computerCase.Case.Dimensions
                .Split('x')
                .Select(s => int.Parse(new Regex(@"\d+")
                .Match(s.Trim()).Value))
                .ToList(); // get the dimensions of the case

            List<int> gpuDimensions = gpu.Gpu.Dimensions
                .Split('x')
                .Select(s => int.Parse(new Regex(@"\d+")
                .Match(s.Trim()).Value))
                .ToList(); // get the dimensions of the GPU

            return caseDimensions[1] > gpuDimensions[1]; // check if the width of the case is greater than the width of the GPU
        }

        /// <summary>
        /// Adds incompatible part to the list of incompatible parts for the case
        /// </summary>
        private void AddToCaseIncompatibleList(ComputerPart computerCase, ComputerPart incompatiblePart)
        {
            if (incompatibilities.ContainsKey(computerCase)) // check if the case is already in the dictionary
            {
                List<ComputerPart> incompatibleParts = incompatibilities[computerCase]; // get the list of incompatible parts

                incompatibleParts.Add(incompatiblePart);

                incompatibilities[computerCase] = incompatibleParts; // update the list of incompatible parts

            }
            else
            {
                List<ComputerPart> incompatibleParts = new List<ComputerPart> { incompatiblePart }; // create a new list with the incompatible part

                incompatibilities.Add(computerCase, incompatibleParts);
            }
        }

        /// <summary>
        /// Checks compatibility of the parts
        /// </summary>
        /// <param name="partIds">IDs of the parts to check the compatibility of</param>
        /// <returns>
        /// JSON of "All parts are compatible" if all parts are compatible, 
        /// otherwise returns a JSON of what parts are incompatible with others
        /// </returns>
        [HttpPost]
        public JsonResult CheckCompatibility(List<int?> partIds)
        {
            List<int> ids;
            if (partIds == null || partIds.Count == 0) // if the list of part IDs is empty
            {
                var sessionIds = HttpContext.Session.GetString("BuildParts"); // get the part IDs from the session
                ids = sessionIds != null ? sessionIds.Split(',').Select(int.Parse).ToList() : new List<int>();
            }
            else
            {
                ids = partIds.Select(i => i.Value).ToList(); // get the part IDs from the list
            }

            List<ComputerPart> parts = _context.ComputerParts.Where(p => ids.Contains(p.Id)).ToList(); // get the parts from the database

            incompatibilities = new Dictionary<ComputerPart, List<ComputerPart>>(); // reset the dictionary

            foreach (ComputerPart part in parts)
            {
                if(part.Type == "Motherboard")
                {
                    ComputerPart motherboard = _context.ComputerParts.Include(p => p.Motherboard).Where(p => p.Id == part.Id).FirstOrDefault();

                    if (motherboard != null) // if the motherboard is in the list of parts
                    {
                        foreach(var part2 in parts)
                        {
                            // check compatibility with other parts
                            if (!part.Equals(part2)) // if the parts are not the same
                            {
                                if (part2.Type == "CPU")
                                {
                                    ComputerPart cpu = _context.ComputerParts.Include(p => p.Cpu).Where(p => p.Id == part2.Id).FirstOrDefault(); // get the CPU from the database

                                    // check compatibility
                                    if (!CheckMbCpuCompatibility(motherboard, cpu))
                                    {
                                        AddToMbIncompatibleList(motherboard, cpu); // add the incompatible part to the list
                                    }
                                }else if(part2.Type == "RAM")
                                {
                                    ComputerPart ram = _context.ComputerParts.Include(p => p.Ram).Where(p => p.Id == part2.Id).FirstOrDefault();

                                    if (!CheckMbRamCompatibility(motherboard, ram))
                                    {
                                        AddToMbIncompatibleList(motherboard, ram);
                                    }
                                }
                                else if(part2.Type == "GPU")
                                {
                                    ComputerPart gpu = _context.ComputerParts.Include(p => p.Gpu).Where(p => p.Id == part2.Id).FirstOrDefault();

                                    if (!CheckMbGpuCompatibility(motherboard, gpu))
                                    {
                                        AddToMbIncompatibleList(motherboard, gpu);
                                    }
                                }
                                else if(part2.Type == "Hard disk")
                                {
                                    ComputerPart disk = _context.ComputerParts.Include(p => p.HardDisk).Where(p => p.Id == part2.Id).FirstOrDefault();

                                    if (GetFreeMbPorts(motherboard).Contains(disk.HardDisk.Connection)) // check if the port is free
                                    {
                                        MarkPortAsTaken(disk.HardDisk.Connection); // mark the port as taken
                                    }
                                    else
                                    {
                                        AddToMbIncompatibleList(motherboard, disk); // add the incompatible part to the list
                                    }
                                }
                            }
                        }
                    }
                }else if(part.Type == "PSU") // if the part is a PSU
                {
                    ComputerPart psu = _context.ComputerParts.Include(p => p.Psu).Where(p => p.Id == part.Id).FirstOrDefault(); // get the PSU from the database

                    int w = CalculateWattage(parts); // calculate the wattage of the computer

                    if (psu.Psu.Power < w)
                    {
                        MarkPsuIncompatible(psu);
                    }

                }
                else if(part.Type == "Case") // if the part is a case
                {
                    ComputerPart computerCase = _context.ComputerParts.Include(p => p.Case).Where(p => p.Id == part.Id).FirstOrDefault();

                    foreach (var part2 in parts)
                    {
                        if(part2.Type == "Motherboard")
                        {
                            ComputerPart motherboard = _context.ComputerParts.Include(p => p.Motherboard).Where(p => p.Id == part2.Id).FirstOrDefault();

                            if(!CheckCaseMbCompatibility(computerCase, motherboard))
                            {
                                AddToCaseIncompatibleList(computerCase, motherboard);
                            }
                        }else if (part2.Type == "PSU")
                        {
                            ComputerPart psu = _context.ComputerParts.Include(p => p.Psu).Where(p => p.Id == part2.Id).FirstOrDefault();

                            if(!CheckCasePsuCompatibility(computerCase, psu))
                            {
                                AddToCaseIncompatibleList(computerCase, psu);
                            }
                        }
                        else if(part2.Type == "GPU")
                        {
                            ComputerPart gpu = _context.ComputerParts.Include(p => p.Gpu).Where(p => p.Id == part2.Id).FirstOrDefault();

                            if(!CheckCaseGpuCompatibility(computerCase, gpu))
                            {
                                AddToCaseIncompatibleList(computerCase, gpu);
                            }
                        }
                    }
                }
            }

            string message = "All parts are compatible"; // default message

            if (incompatibilities.Count > 0) // if there are incompatible parts
            {
                message = "";
                foreach(var key in incompatibilities.Keys) // iterate through the dictionary
                {
                    message += $"* Parts incompatible with {key.Type} {key.Name}: \n";
                    List<ComputerPart> incompatibleParts = incompatibilities[key];
                    foreach(var part in incompatibleParts)
                    {
                        message += $"   {part.Type}: {part.Name}\n"; // add the incompatible part to the message
                    }
                    message += "\n";
                }
            }

            return Json(new { message });
        }
    }
}
