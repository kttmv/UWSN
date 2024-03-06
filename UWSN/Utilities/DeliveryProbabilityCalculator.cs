using Dew.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Utilities
{
    public static class DeliveryProbabilityCalculator
    {
        private const double ro = 1000d;
        private const double pi = Math.PI;
        private const double c = 1500d;

        /// <summary>
        /// Вычислить вероятность доставки сообщения по входным параметрам модели среды
        /// </summary>
        /// <param name="f">Несущая частота модема</param>
        /// <param name="fbit">Битовая скорость</param>
        /// <param name="tx">Координата передающего модема</param>
        /// <param name="rx">Координата принимающего модема</param>
        /// <param name="ps">Мощность модема</param>
        public static double Calculate(double f, double fbit, Vector3 tx, Vector3 rx, double ps)
        {
            // искомая вероятность доставки одного бита сообщения
            double pbit;

            double x;

            double r = Vector3.Distance(tx, rx);

            double snr;

            //double p0 = Math.Sqrt(ro * c * ps / 2 * pi);

            // в модели среды не дана формула вычисления
            //double n0 = 1d;

            //snr = 20 * Math.Log10(p0 / n0 * r);

            // в тестовых задачах положим p0/n0 = 6.71∙10^3
            snr = 20 * Math.Log10(6.71 * 1000 / r);

            double beta;

            double beta0 = 0.1 * Math.Pow(f, 2) / (1 + Math.Pow(f, 1)) + 40 * Math.Pow(f, 2) / (4100 + Math.Pow(f, 2))
                            + 2.75 * 0.0001 * Math.Pow(f, 2) + 0.0003;

            // ??? ГЛУБИНА ИЗЛУЧАЕМОГО СИГНАЛА ???
            double h = (tx.Y + rx.Y) / 2;

            double alpha = 1 - 6.54 * Math.Pow(10, -5) * h;

            beta = beta0 * alpha;

            x = Math.Sqrt(f / fbit) * Math.Pow(10, 0.05 * (snr - beta) * r);

            if (x > 3)
            {
                pbit = 1 - 1 / Math.Sqrt(2 * pi) * Math.Exp(-Math.Pow(x, 2) / 2);
            }
            else
            {
                Func<double, double> function = u => Math.Exp(-Math.Pow(u, 2) / 2);

                double integral = Integrate(function, x, 1000d, 10000);

                pbit = 1 - 1 / Math.Sqrt(2 * pi) * integral;
            }

            return pbit;
        }

        /// <summary>
        /// ЧМ прямоугольников по ХВБ :)
        /// </summary>
        private static double Integrate(Func<double, double> f, double a, double b, int n)
        {
            double h = (b - a) / n; // шаг разбиения
            double area = 0.0;

            for (int i = 0; i < n; i++)
            {
                area += f(a + i * h) * h;
            }

            return area;
        }
    }
}