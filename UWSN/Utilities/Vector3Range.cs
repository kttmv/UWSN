using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Utilities;

public struct Vector3Range
{
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public Vector3Range(Vector3 min, Vector3 max)
    {
        Min = min; Max = max;
    }
}