﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Modems;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers
{
    public class PlaceSensorsBySpecsHandler
    {
        public static void Handle(PlaceSensorsBySpecsOptions o)
        {
            SerializationHelper.LoadSimulation(o.FilePath);
            var sensors = new List<Sensor>();

            Simulation.Instance.AvailableModems = new List<ModemBase>() { new AquaCommMako(), new AquaCommMarlin(), new AquaCommOrca(),
                                                  new AquaModem1000(), new AquaModem500(), new MicronModem(), new SMTUTestModem()};

            var selectedModem = Simulation.Instance.AvailableModems.FirstOrDefault(m => m.Name == o.ModemModel);
            if (selectedModem == null)
            {
                throw new ArgumentException("Неверное название модема");
            }

            Simulation.Instance.Modem = selectedModem;

            var al = Simulation.Instance.AreaLimits;

            double rMax = DeliveryProbabilityCalculator.CaulculateSensorDistance(selectedModem, al) * o.DistanceCoeff;

            double areaLength = al.Max.X - al.Min.X;
            double areaDepth = al.Max.Y - al.Min.Y;
            double areaWidth = al.Max.Z - al.Min.Z;

            // количество сенсоров по каждой из координат
            int countX = (int)Math.Ceiling(areaLength / rMax);
            int countY = (int)Math.Ceiling(areaDepth / rMax);
            int countZ = (int)Math.Ceiling(areaWidth / rMax);

            int sensorsCount = countX * countY * countZ;

            for (int i = 0; i < sensorsCount; i++)
            {
                sensors.Add(new Sensor(i));
            }

            switch (o.DistributionType)
            {
                case "rndStepNormal":
                    Simulation.Instance.Environment.Sensors =
                        PlaceNormal(sensors, rMax, countX, countY, countZ);

                    break;

                case "orth":
                    Simulation.Instance.Environment.Sensors =
                        PlaceOrth(sensors, rMax, countX, countY, countZ);

                    break;

                case "rndStepUni":

                    break;

                default:
                    throw new Exception("Указан не поддерживаемый тип распределения сенсоров");
            }
            

            SerializationHelper.SaveSimulation(o.FilePath);
        }

        private static List<Sensor> PlaceNormal(List<Sensor> sensors, double stepRange, int countX, int countY, int countZ)
        {
            var al = Simulation.Instance.AreaLimits;

            var rnd = new Random();

            int placedCount = 0;

            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    for (int k = 0; k < countZ; k++)
                    {
                        var x = al.Min.X + (float)((i * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));
                        var y = al.Min.Y + (float)((j * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));
                        var z = al.Min.Z + (float)((k * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));

                        if (x < al.Min.X)
                            x = al.Min.X;
                        if (x > al.Max.X)
                            x = al.Max.X;
                        if (y < al.Min.Y)
                            y = al.Min.Y;
                        if (y > al.Max.Y)
                            y = al.Max.Y;
                        if (z < al.Min.Z)
                            z = al.Min.Z;
                        if (z > al.Max.Z)
                            z = al.Max.Z;

                        sensors[placedCount].Position = new Vector3(x, y, z);

                        placedCount++;
                    }
                }
            }

            Console.WriteLine($"Расстановка сенсоров ({countX * countY * countZ}) в узлах " +
                $"ортогональной решетки с нормальным отклонением прошла успешно.");

            return sensors;
        }

        private static List<Sensor> PlaceOrth(List<Sensor> sensors, double stepRange, int countX, int countY, int countZ)
        {
            var al = Simulation.Instance.AreaLimits;

            var rnd = new Random();

            int placedCount = 0;

            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    for (int k = 0; k < countZ; k++)
                    {
                        var x = al.Min.X + (float)(i * stepRange);
                        var y = al.Min.Y + (float)(j * stepRange);
                        var z = al.Min.Z + (float)(k * stepRange);

                        sensors[placedCount].Position = new Vector3(x, y, z);

                        placedCount++;
                    }
                }
            }

            Console.WriteLine($"Расстановка сенсоров ({countX * countY * countZ}) в узлах ортогональной решетки прошла успешно.");

            return sensors;
        }

        private static double NextDouble(Random rnd, double min, double max)
        {
            return min + (rnd.NextDouble() * (max - min));
        }
    }
}
