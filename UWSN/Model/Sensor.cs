using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class Sensor
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }

        public Sensor(int id) 
        {
            Id = id;
            Position = new Vector3();
        }
    }
}
