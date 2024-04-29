using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Sim;

public class SimulationResult
{
    public int TotalSends { get; set; }
    public int TotalReceives { get; set; }
    public int TotalCollisions { get; set; }
}