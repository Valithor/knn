using System;
using System.Collections.Generic;
using System.Text;

namespace knn
{
    public class Metryki
    {
        public static double Euklidesowa(double[] x, double[] y, double p)
        {
            double suma = 0;
            for (int i = 0; i < x.Length; i++)
                suma += Math.Pow((x[i] - y[i]),2);
            return Math.Sqrt(suma);
        }
        public static double Manhatan(double[] x, double[] y, double p)
        {
            double suma = 0;
            for (int i = 0; i < x.Length; i++)
                suma += Math.Abs(x[i] - y[i]);
            return suma;
        }
        public static double Czebyszewa(double[] x, double[] y, double p)
        {
            double najwyzsza = Math.Abs(x[0] - y[0]);
            for (int i = 0; i < x.Length; i++)
                if (najwyzsza < Math.Abs(x[i] - y[i]))
                    najwyzsza = Math.Abs(x[i] - y[i]);
            return najwyzsza;
        }
        public static double zLogarytmem(double[] x, double[] y, double p)
        {
            double suma = 0;
            for (int i = 0; i < x.Length; i++)
                suma += Math.Abs((Math.Log10(x[i])) - Math.Log10(y[i]));
            return suma;
        }
        public static double Minkowskiego(double[] x, double[] y, double p)
        {
            double suma = 0;
            for (int i = 0; i < x.Length; i++)
                suma += Math.Pow(Math.Abs(x[i] - y[i]),p);
            return Math.Pow(suma, 1/p);
        }
    }
}
