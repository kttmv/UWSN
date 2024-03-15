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

           double snr1 = CalculatePassiveSonarEq(f, ps, r); 

            double beta;

            double beta0 = 0.1 * Math.Pow(f, 2)/(1 + Math.Pow(f, 2)) + 40 * Math.Pow(f, 2)/(4100 + Math.Pow(f, 2))
                            + 2.75 * 0.0001 * Math.Pow(f, 2) + 0.0003;

            // ??? ГЛУБИНА ИЗЛУЧАЕМОГО СИГНАЛА ???
            double h = (tx.Z + rx.Z) / 2;

            double alpha = 1 - 6.54 * Math.Pow(10, -5) * h;

            beta = beta0 * alpha;

            x = Math.Sqrt(f / fbit) * Math.Pow(10, 0.05 * (snr - beta * r / 1000));

            if (x > 3)
            {
                pbit = 1 - 1 / Math.Sqrt(2 * pi) * Math.Exp(-Math.Pow(x, 2) / 2);
            }
            else
            {
                Func<double, double> function = u => Math.Exp(-Math.Pow(u, 2) / 2);

                double integral = Integrate(function, x, 100000d, 10000);

                pbit = 1 - 1 / Math.Sqrt(2 * pi) * integral;
            }

            return pbit;
        }

        /// <summary>
        /// Вычисление отношения сигнал-шум SNR через уравнение пассивного сонара
        /// </summary>
        private static double CalculatePassiveSonarEq(double f, double ps, double r, double s = 0.5, double w = 0.0, double k = 2.0)
        {
            // искомое отношение сигнал-шум
            double snr = double.MinValue;

            double sl = 10 * Math.Log10(ps) + 170.8;

            double logAlpha = double.MinValue;

            if (f >= 0.4)
            {
                logAlpha = 0.11 * Math.Pow(f, 2) / (1 + Math.Pow(f, 2)) + 44 * Math.Pow(f, 2) / (4100 + Math.Pow(f, 2))
                    + 2.75 * 0.0001 * Math.Pow(f, 2) + 0.0003;
            }
            else
            {
                logAlpha = 0.002 + 0.11 * f / (1 + f) + 0.011 * f;            
            }

            double tl = k * 10 * Math.Log10(r) + r / 1000 * logAlpha;

            double logNt = 17 - 30 * Math.Log10(f);
            double nt = Math.Pow(10.0, logNt) / 10;

            double logNs = 40 + 20 * (s - 0.5) + 26 * Math.Log10(f) - 60 * Math.Log10(f + 0.03);
            double ns = Math.Pow(10.0, logNs) / 10;

            double logNw = 50 + 7.5 * Math.Sqrt(w) + 20 * Math.Log10(f) -40 * Math.Log10(f + 0.4);
            double nw = Math.Pow(10.0, logNw) / 10;

            double logNth = -15 + 20 * Math.Log10(f);
            double nth = Math.Pow(10.0, logNth) / 10;

            double nLinTotal = logNt + logNs + logNw +  logNth;
            double nTotal = 10 * Math.Log10(nLinTotal);

            if (double.IsNaN(nTotal))
            {
                nTotal = 0.0;
            }

            // кто сказал?
            double di = 0.0;

            snr = sl - tl - (nTotal - di);

            return snr;
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