namespace LibiadaWeb.Math
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// The fft.
    /// </summary>
    public static class FastFourierTransform
    {
        /// <summary>
        /// The fourier transform.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        public static void FourierTransform(List<List<double>> characteristics)
        {
            // переводим в комлексный вид
            // Для всех характеристик
            for (int i = 0; i < characteristics.Last().Count; i++)
            {
                var complex = new List<Complex>();
                int j;

                // Для всех фрагментов цепочек
                for (j = 0; j < characteristics.Count; j++)
                {
                    complex.Add(new Complex(characteristics[j][i], 0));
                }

                int m = 1;

                while (m < j)
                {
                    m *= 2;
                }

                for (; j < m; j++)
                {
                    complex.Add(new Complex(0, 0));
                }

                // вернёт массив
                Complex[] data = FourierTransform(complex.ToArray()); 

                // переводим в массив double
                for (int g = 0; g < characteristics.Count; g++)
                {
                    characteristics[g][i] = data[g].Real;
                }
            }
        }

        /// <summary>
        /// Возвращает спектр сигнала
        /// </summary>
        /// <param name="x">
        /// Массив значений сигнала. Количество значений должно быть степенью 2
        /// </param>
        /// <returns>
        /// Массив со значениями спектра сигнала
        /// </returns>
        public static Complex[] FourierTransform(Complex[] x)
        {
            Complex[] result;
            int n = x.Length;
            if (n == 2)
            {
                result = new Complex[2];
                result[0] = x[0] + x[1];
                result[1] = x[0] - x[1];
            }
            else
            {
                var even = new Complex[n / 2];
                var odd = new Complex[n / 2];
                for (int i = 0; i < n / 2; i++)
                {
                    even[i] = x[2 * i];
                    odd[i] = x[(2 * i) + 1];
                }

                even = FourierTransform(even);
                odd = FourierTransform(odd);
                result = new Complex[n];
                for (int i = 0; i < n / 2; i++)
                {
                    result[i] = even[i] + (W(i, n) * odd[i]);
                    result[i + (n / 2)] = even[i] - (W(i, n) * odd[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Вычисление поворачивающего модуля e^(-i*2*PI*k/N).
        /// </summary>
        /// <param name="k">
        /// The k.
        /// </param>
        /// <param name="n">
        /// The n.
        /// </param>
        /// <returns>
        /// The <see cref="Complex"/>.
        /// </returns>
        private static Complex W(int k, int n)
        {
            if ((k % n) == 0)
            {
                return 1;
            }

            double arg = -2 * System.Math.PI * k / n;
            return new Complex(System.Math.Cos(arg), System.Math.Sin(arg));
        }
    }
}
