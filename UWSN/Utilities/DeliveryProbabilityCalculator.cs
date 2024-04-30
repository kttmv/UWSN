using Dew.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UWSN.Model.Modems;

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
        public static double Calculate(double f, double fbit, Vector3 tx, Vector3 rx, double ps, bool isPassiveEq = true)
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
            snr = isPassiveEq ? CalculatePassiveSonarEq(f, ps, r) : 20 * Math.Log10(6.71 * 1000 / r);

            double beta;

            double beta0 = 0.1 * Math.Pow(f, 2)/(1 + Math.Pow(f, 2)) + 40 * Math.Pow(f, 2)/(4100 + Math.Pow(f, 2))
                            + 2.75 * 0.0001 * Math.Pow(f, 2) + 0.0003;

            // ??? ГЛУБИНА ИЗЛУЧАЕМОГО СИГНАЛА ???
            double h = (tx.Y + rx.Y) / 2;

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

                double integral = Integrate(function, x, 5d, 1000);

                pbit = 1 - 1 / Math.Sqrt(2 * pi) * integral;
            }

            return Math.Pow(pbit, 256.0);
        }

        /// <summary>
        /// Вычисление отношения сигнал-шум SNR через уравнение пассивного сонара
        /// </summary>
        public static double CalculatePassiveSonarEq(double f, double ps, double r, double s = 0.5, double w = 0.0, double k = 2.0)
        {
            // искомое отношение сигнал-шум
            double snr = double.MinValue;

            double sl = 10 * Math.Log10(ps) + 170.8;

            //double sl = 190.8;

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

            double logNs = 40 + 20 * (s - 0.5) + 26 * Math.Log10(f) - 60 * Math.Log10(f + 0.03);

            double logNw = 50 + 7.5 * Math.Sqrt(w) + 20 * Math.Log10(f) -40 * Math.Log10(f + 0.4);

            double logNth = -15 + 20 * Math.Log10(f);


            double nTotal = Math.Pow(10.0, logNt / 10.0) + Math.Pow(10.0, logNs / 10.0) + Math.Pow(10.0, logNw / 10.0)
                            + Math.Pow(10.0, logNth / 10.0);

            double nl = 10 * Math.Log10(nTotal);

            double di = 0.0;

            snr = sl - tl - (nl - di);

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

        public static double CaulculateSensorDistance(ModemBase modem, Vector3Range arealimits, double s = 0.5, double w = 0.0, double k = 2.0)
        {
            //Попробуйте посчитать расстояние для мощности - 35 Вт, частоты - 26 кГц,
            //полосы пропускания – 16 кГц, SNR – 18,1 ДБ, битовой скорости 13,9 кБит.
            //Расстояние должно получиться в районе 3.5 км.


            // искомое максимальное допустимое расстояние между сенсорами
            double rmin = double.NaN;
            // договорились, что не ищем по битовой ошибке и модуляции снр, а просто берем 10
            double snrMin = 18.1;



            // мощность модема
            double ps = modem.PowerTX;
            // несущая частота модема
            double f = modem.CenterFrequency;
            double fbit = modem.Bitrate;
            double b = modem.Bandwidth;

            // чо делать с пустыми значениями характеристик модемов - неясно

            // тестовый модем
            //f = 26.0;
            //ps = 35.0;
            //b = 16.0;
            //fbit = 13900;

            double bf = 10 * Math.Log10(b / fbit);

            double sl = 10 * Math.Log10(ps) + 170.8;
            
            double di = 0.0;

            #region calculate nl
            double logNt = 17 - 30 * Math.Log10(f);

            double logNs = 40 + 20 * (s - 0.5) + 26 * Math.Log10(f) - 60 * Math.Log10(f + 0.03);

            double logNw = 50 + 7.5 * Math.Sqrt(w) + 20 * Math.Log10(f) - 40 * Math.Log10(f + 0.4);

            double logNth = -15 + 20 * Math.Log10(f);


            double nTotal = Math.Pow(10.0, logNt / 10.0) + Math.Pow(10.0, logNs / 10.0) + Math.Pow(10.0, logNw / 10.0)
                            + Math.Pow(10.0, logNth / 10.0);
            #endregion
            double nl = 10 * Math.Log10(nTotal);
            //nl = 0.0;


            double tl = sl - snrMin - (nl - di) + bf;

            double logAlpha = double.MinValue;
            #region calculate logAlpha
            if (f >= 0.4)
            {
                logAlpha = 0.11 * Math.Pow(f, 2) / (1 + Math.Pow(f, 2)) + 44 * Math.Pow(f, 2) / (4100 + Math.Pow(f, 2))
                    + 2.75 * 0.0001 * Math.Pow(f, 2) + 0.0003;
            }
            else
            {
                logAlpha = 0.002 + 0.11 * f / (1 + f) + 0.011 * f;
            }
            #endregion

            // имеем уравнение
            // k * 10log(rmin) + rmin/1000 * logalpha = tl
            
            Func<double, double> function = x => k * 10 * Math.Log10(x) + x/1000 * logAlpha - tl;
            Func<double, double> dfunction = x => logAlpha/1000 + 10*k/x*Math.Log10(x);

            rmin = NewtonMethod(function, dfunction, 500.0, 1.0);

            return rmin;
        }

        private static double NewtonMethod(Func<double, double> f, Func<double, double> df,double x0, double eps)
        {
            double xn = x0;
            double xn1 = xn - f(xn)/df(xn);
            int i = 1;

            while (Math.Abs((xn1 - xn)/(1-((xn1-xn)/(xn-xn1)))) > eps)
            {
                xn = xn1;
                xn1 = xn1 - f(xn1)/df(xn1);
                i++;
            }

            return xn1;
        }
    }
}