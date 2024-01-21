﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dew.Math;

namespace UWSN.Model
{
    public class SensorPlacementRandomStep : ISensorPlacementModel
    {
        public required List<Sensor> Sensors { get; set; }
        public required float StepRange { get; set; }
        public required string DistrType { get; set; }
        public required double UniParameterA { get; set; }
        public required double UniParameterB { get; set; }

        public List<Sensor> PlaceSensors()
        {
            if (DistrType == "Normal")
            {
                Random rnd = new Random();

                int placedCount = 0;
                int cubicEdge = (int)(Math.Ceiling(Math.Pow(Sensors.Count, 1 / 3)));

                for (int i = 0; i < cubicEdge; i++)
                {
                    for (int j = 0; j < cubicEdge; j++)
                    {
                        for (int k = 0; k < cubicEdge; k++)
                        {
                            if (placedCount >= Sensors.Count)
                            {
                                break;
                            }

                            Sensors[placedCount].Position = new System.Numerics.Vector3((float)((i * StepRange) + NextDouble(rnd, -StepRange/2, StepRange/2)), 
                                                                                        (float)((j * StepRange) + NextDouble(rnd, -StepRange/2, StepRange/2)), 
                                                                                        (float)((k * StepRange) + NextDouble(rnd, -StepRange/2, StepRange/2)));
                            placedCount++;
                        }
                    }
                }
            }
            if (DistrType == "Uniform")
            {
                TRngStream rng = new TRngStream();
                var dst = new TMtxVec();
                dst.Length = Sensors.Count * 3;
                rng.RandomUniform(dst, UniParameterA, UniParameterB);
            }

            return Sensors;
        }

        public SensorPlacementRandomStep(List<Sensor> sensors, float stepRange, string distrType, double uniParameterA = 0, double uniParameterB = 1) 
        {
            Sensors = sensors;
            StepRange = stepRange;
            if (DistrType != "Normal" && DistrType != "Uniform")
            {
                throw new Exception("Неверное распределение");
            }
            else
            {
                DistrType = distrType;
                UniParameterA = uniParameterA;
                UniParameterB = uniParameterB;
            }
        }

        private double NextDouble(Random rnd, double min, double max)
        {
            return min + (rnd.NextDouble() * (max - min));
        }

        private double UniformDouble(double uniformValue, double min, double max)
        {
            return min + (uniformValue * (max - min));
        }
    }
}
