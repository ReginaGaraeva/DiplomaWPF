using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CG_Lab04
{
    class Matrix
    {
        // Определитель матрицы
        static public double Determinant(double[,] m, out bool Correct)
        {
            if (m.GetLength(0) != m.GetLength(1))
            {
                Correct = false;
                return -1;
            }
            else
            {
                int N = m.GetLength(0);
                double[,] A = new double[N, N];
                Array.Copy(m, A, m.Length);

                // Приведение матрицы к верхнему треугольному виду
                double res = 1;
                for (int j = 0; j < N - 1; j++)
                {
                    double c = A[j, j];
                    for (int i = j + 1; i < N; i++)
                    {
                        c = A[i, j] / m[j, j];
                        for (int k = j; k < m.GetLength(1); k++)
                            A[i, k] = A[i, k] - A[j, k] * c;
                    }
                }
                // Вычисление определителя
                for (int i = 0; i < N; i++)
                    res *= A[i, i];

                Correct = true;
                return res;
            }
        }

        static public double[,] Transpose(double[,] M)
        {
            double[,] res = new double[M.GetLength(1), M.GetLength(0)];
            for (int i = 0; i < M.GetLength(0); i++)
                for (int j = 0; j < M.GetLength(1); j++)
                    res[j, i] = M[i, j];
            return res;
        }

        // Умножение матриц
        static public double[,] Multiplication(double[,] m1, double[,] m2, out bool Correct)
        {
            if (m1.GetLength(1) == m2.GetLength(0))
            {
                int na = m1.GetLength(0);
                int nb = m2.GetLength(1);
                int n = m1.GetLength(1);
                double[,] m = new double[na, nb];

                for (int i = 0; i < na; i++)
                {
                    for (int j = 0; j < nb; j++)
                    {
                        m[i, j] = 0;
                        for (int k = 0; k < n; k++)
                            m[i, j] += m1[i, k] * m2[k, j];
                    }
                }
                Correct = true;
                return m;
            }
            else
            {
                Correct = false;
                return new double[0, 0];
            }
        }

        // Умножение вектора на матрицу
        static public double[] Multiplication(double[] v, double[,] m2, out bool Correct)
        {
            if (v.GetLength(0) == m2.GetLength(1))
            {
                int nb = m2.GetLength(0);
                int n = v.Length;
                double[] vr = new double[nb];

                for (int j = 0; j < nb; j++)
                {
                    vr[j] = 0;
                    for (int k = 0; k < n; k++)
                        vr[j] += v[k] * m2[j, k];
                }

                Correct = true;
                return vr;
            }
            else
            {
                Correct = false;
                return new double[0];
            }
        }

        // Решение СЛАУ методом Гаусса
        static public double[] SLE_Gauss(double[,] a, double[] b, out bool Correct)
        {
            bool correct;
            double det = Determinant(a, out correct);
            if (!correct || det == 0 || a.GetLength(1) != b.Length)
            {
                Correct = false;
                return new double[0];
            }
            else
            {
                int N = a.GetLength(0);
                double[] res = new double[N];
                double[,] A = new double[N, N];
                double[] B = new double[N];
                Array.Copy(a, A, a.Length);
                Array.Copy(b, B, b.Length);

                // Прямой ход метода Гаусса
                for (int j = 0; j < N - 1; j++)
                {
                    double c = A[j, j];
                    for (int i = j + 1; i < N; i++)
                    {
                        c = A[i, j] / A[j, j];
                        for (int k = j; k < A.GetLength(1); k++)
                            A[i, k] = A[i, k] - A[j, k] * c;
                        B[i] = B[i] - B[j] * c;
                    }
                }
                // Обратный ход метода Гаусса
                for (int i = B.Length - 1; i >= 0; i--)
                {
                    double sum = 0;
                    for (int j = B.Length - 1; j > i; j--)
                        sum += res[j] * A[i, j];
                    res[i] = (B[i] - sum) / A[i, i];
                }

                Correct = true;
                return res;
            }
        }

        // Обратная матрица
        static public double[,] InvertibleMatrix(double[,] m, out bool Correct)
        {
            bool correct;
            double det = Determinant(m, out correct);
            if (!correct || det == 0)
            {
                Correct = false;
                return new double[0, 0];
            }
            else
            {
                int N = m.GetLength(0);
                double[,] res = new double[N, N];
                double[,] A = new double[N, N];
                Array.Copy(m, A, m.Length);

                // Метод Гаусса-Жордано
                for (int i = 0; i < N; i++)
                {
                    double[] E = new double[N];
                    double[] X = new double[N];

                    // Создаем единичные векторы
                    for (int j = 0; j < N; j++) E[j] = 0;
                    E[i] = 1;

                    X = SLE_Gauss(A, E, out correct);
                    for (int j = 0; j < N; j++)
                        res[j, i] = X[j];
                }

                Correct = true;
                return res;
            }
        }
    }
}
