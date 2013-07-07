using System;
using System.Numerics;

namespace LibiadaWeb.Models
{
    public static class FFT
    {
        /// <summary>
        /// Вычисление поворачивающего модуля e^(-i*2*PI*k/N)
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static Complex W(int k, int n)
        {
            if (k % n == 0) return 1;
            double arg = -2 * Math.PI * k / n;
            return new Complex(Math.Cos(arg), Math.Sin(arg));
        }
        /// <summary>
        /// Возвращает спектр сигнала
        /// </summary>
        /// <param name="x">Массив значений сигнала. Количество значений должно быть степенью 2</param>
        /// <returns>Массив со значениями спектра сигнала</returns>
        public static Complex[] Fft(Complex[] x)
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
                Complex[] xEven = new Complex[n / 2];
                Complex[] xOdd = new Complex[n / 2];
                for (int i = 0; i < n / 2; i++)
                {
                    xEven[i] = x[2 * i];
                    xOdd[i] = x[2 * i + 1];
                }
                xEven = Fft(xEven);
                xOdd = Fft(xOdd);
                result = new Complex[n];
                for (int i = 0; i < n / 2; i++)
                {
                    result[i] = xEven[i] + W(i, n) * xOdd[i];
                    result[i + n / 2] = xEven[i] - W(i, n) * xOdd[i];
                }
            }
            return result;
        }
    }
}