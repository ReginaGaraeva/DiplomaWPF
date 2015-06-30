using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FieldDataAnalyzer;

namespace FIeldDataAnalyzer
{
	public interface IOptimizerStrategy
	{
		double[] Run(double eps, double[] x0);
	}

	public interface IOptimizer
	{
		double[] NextSolution(double[] x0);
		bool IsConvergense(double[] x1, double[] x0);
	}

	public class OptimizerContext
	{
		private IOptimizerStrategy _optimizer;

		public OptimizerContext(IOptimizerStrategy optimizer)
		{
			_optimizer = optimizer;
		}

		public void SetStrategy(IOptimizerStrategy optimizer)
		{
			_optimizer = optimizer;
		}

		public double[] Run(double eps, double[] x0)
		{
			return _optimizer.Run(eps, x0);
		}
	}	

	public class Gradient : IOptimizerStrategy, IOptimizer
	{
		private int _N; //кол-во параметров оптимизируемой функции
		public delegate double F(double[] x);

		private double _eps;

		private F _func;

		public Gradient(F func, int N)
		{
			_func = func;
			_N = N;
		}

		public double[] Run(double eps, double[] x0)
		{
			_eps = eps;
			//double lyambda = Dihotomiya(x0, eps);
			//double[] x1 = x0.Select((m, i) => m - dF(i, x0)*lyambda).ToArray();
			double[] x1 = NextSolution(x0);
			while (!IsConvergense(x1,x0))
			{
				//lyambda = Dihotomiya(x0, eps);
				x0 = x1;
				x1 = NextSolution(x0);
			}
			return x1;
		}

		public double[] NextSolution(double[] x0)
		{
			double lyambda = Dihotomiya(x0, _eps);
			return x0.Select((m, i) => m - dF(i, x0[i]) * lyambda).ToArray();
		}

		public bool IsConvergense(double[] x1, double[] x0)
		{
			return Math.Abs(_func(x1) - _func(x0)) <= _eps;
		}

		double dF(int i, double _x)
		{
			double delta = 0.0001;
			double[] x0 = new double[_N];
			x0[i] = _x;
			double [] x = new double[_N];
			x[i] += _x + delta;
			return (_func(x) - _func(x0)) / delta;	
		}

		double dFi(double lyambda, double[] x0)
		{
			return _func(x0.Select((m, i) => m - dF(i, x0[i])*lyambda).ToArray());
			//return _func(x0 - dZdX(x0) * lyambda, y0 - dZdY(y0) * lyambda);
		}

		double Dihotomiya(double[] x_0,double eps)
		{
			double left = 0;
			double right = 1;
			double x1 = left + (right - left) / 2.0 - eps / 2;
			double x2 = left + (right - left) / 2.0 + eps / 2;
			while (right - left > 2 * eps + 0.00001)
			{
				if (dFi(x1, x_0) < dFi(x2, x_0))
					right = x2;
				else
					left = x1;
				x1 = left + (right - left) / 2.0 - eps / 2;
				x2 = left + (right - left) / 2.0 + eps / 2;
			}
			return left + (right - left) / 2;
		}
	}

	public class ShtutzerOptimizer
	{
		private DBService dbService;
		private Graph graph;
		private Evaluator evaluator;
		public WellData maxKGFWell;
		public WellData min1KGFWell;
		public WellData min2KGFWell;
		public DateTime optimizationDate;
		int n = 3; //мерность пространства

		private double alpha1 = 1, alpha2 = 1; //коэффициенты влияния
		private double[] d_shts = { 0.06d, 0.08d, 0.10d, 0.12d, 0.14d };
		private double Psb, Gsb;

		private OptimizerContext context;

		public ShtutzerOptimizer(Graph _graph)
		{
			graph = _graph;
			graph.Clear();
			evaluator = new Evaluator(graph, new FieldDescription(), new ProgressBar());
			dbService = new DBService();

			context = new OptimizerContext(new Gradient(F,n));
			//db.GetWellsMeasurementsByDate(optimizeDatePicker.DisplayDate.Date);
		}

		double F(double[] x)
		{
			maxKGFWell.Shtutzer.d_sht_current = (float)x[0];
			min1KGFWell.Shtutzer.d_sht_current = (float)x[1];
			min2KGFWell.Shtutzer.d_sht_current = (float)x[2];
			evaluator.CalcGraph(optimizationDate);
			
			return -(graph.wells.Sum(m => m.Gl) / graph.wells.Sum(m => m.Gg) -
				   (alpha1 * Math.Abs(graph.endNode.P - Psb) + alpha2 * Math.Abs(graph.endNode.G - Gsb)));
			//return (x[0] + 10)*(x[0] + 10) + x[1]*x[1] + x[2]*x[2];
		}

		public double[] Run(DateTime _optimizationDate)
		{
			optimizationDate = _optimizationDate;
			double maxKGF = graph.wells.Where(s => s.Gg != 0).Max(m => m.Gl / m.Gg),
				min1KGF = graph.wells.Where(s => s.Gg != 0).Min(m => m.Gl / m.Gg),
				min2KGF = graph.wells.Where(z => (z.Gg != 0) && (z.Gl / z.Gg != min1KGF)).Min(m => m.Gl / m.Gg);

			maxKGFWell = graph.wells.First(z => z.Gl / z.Gg == maxKGF);
			min1KGFWell = graph.wells.First(z => z.Gl / z.Gg == min1KGF);
			min2KGFWell = graph.wells.First(z => z.Gl / z.Gg == min2KGF);

			evaluator.CalcGraph(optimizationDate);
			Psb = graph.endNode.P;
			Gsb = graph.endNode.G;

			return context.Run(0.001, new [] {0.06, 0.12, 0.08});
		}


	}


	class Optimizer
	{
		private DBService dbService;
		private Graph graph;
		private Evaluator evaluator;
		public WellData maxKGFWell;
		public WellData min1KGFWell;
		public WellData min2KGFWell;
		public DateTime optimizationDate;
		int n = 3; //мерность пространства

		private double alpha1 = 1, alpha2 = 1; //коэффициенты влияния
		private double[] d_shts = {0.06d, 0.08d, 0.10d, 0.12d, 0.14d};
		private double Psb, Gsb;
		

		public Optimizer(Graph _graph)
		{
			graph = _graph;
			graph.Clear();
			evaluator = new Evaluator(graph, new FieldDescription(), new ProgressBar());
			dbService = new DBService();

			//db.GetWellsMeasurementsByDate(optimizeDatePicker.DisplayDate.Date);
		}

		double F(double[] x)
		{
			maxKGFWell.Shtutzer.d_sht_current = (float)x[0];
			min1KGFWell.Shtutzer.d_sht_current = (float)x[1];
			min2KGFWell.Shtutzer.d_sht_current = (float)x[2];
			evaluator.CalcGraph(optimizationDate);
			
			return -(graph.wells.Sum(m => m.Gl) / graph.wells.Sum(m => m.Gg) -
				   (alpha1 * Math.Abs(graph.endNode.P - Psb) + alpha2 * Math.Abs(graph.endNode.G - Gsb)));
			//return (x[0] + 10)*(x[0] + 10) + x[1]*x[1] + x[2]*x[2];
		}

		bool Сonvergence(double eps, double[] f)
		{
			//вычисляем стандартное отклонение
			double f_ = f.Sum()/(n + 1);
			return Math.Sqrt(f.Sum(x => Math.Pow(x - f_, 2))/(n + 1)) < eps;
		}

		public double[] Calc(DateTime _optimizationDate)
		{
			optimizationDate = _optimizationDate;
			double maxKGF = graph.wells.Where(s => s.Gg != 0).Max(m => m.Gl/m.Gg),
				min1KGF = graph.wells.Where(s => s.Gg != 0).Min(m => m.Gl / m.Gg),
				min2KGF = graph.wells.Where(z => (z.Gg != 0)&&(z.Gl / z.Gg != min1KGF)).Min(m => m.Gl / m.Gg);

			maxKGFWell = graph.wells.First(z => z.Gl/z.Gg == maxKGF);
			min1KGFWell = graph.wells.First(z => z.Gl / z.Gg == min1KGF);
			min2KGFWell = graph.wells.First(z => z.Gl / z.Gg == min2KGF);

			maxKGFWell.d_sht = maxKGFWell.Shtutzer.d_sht_current;
			min1KGFWell.d_sht = min1KGFWell.Shtutzer.d_sht_current;
			min2KGFWell.d_sht = min2KGFWell.Shtutzer.d_sht_current;

			double eps = 0.001;
			//maxKGFWell.Name = "99999999";
			//Подставить начальные значения dшт
			//foreach (var well in graph.wells)
			//{
			//	well.d_sht = 0.06;
			//	well.Gl = 400;
			//	well.Gg = 1300;
			//}
			//Вычислить по модели штуцера и сети
			//Определить скважину с max КГФ и две с min КГФ

			//Перейти к новому решению для этих скважин
			evaluator.CalcGraph(optimizationDate);
			Psb = graph.endNode.P;
			Gsb = graph.endNode.G;
			double alpha = 1, beta = 0.5, gamma = 2;
			List<double[]> x = new List<double[]>();
			double[] f = new double[n+1];

			//начальные значения x
			x.Add(new []{d_shts[0], d_shts[1], d_shts[2]});
			x.Add(new []{d_shts[0], d_shts[0], d_shts[2]});
			x.Add(new []{d_shts[2], d_shts[3], d_shts[4]});
			x.Add(new[] { d_shts[2], d_shts[3], d_shts[1]});

			int xh_ind; //индекс наибольшего f 
			int xg_ind; //индекс второго максимума
			int xl_ind; //индекс наименьшего f

			double[] xr; //отраженная точка
			double[] xe; //точка после растяжения
			double[] xc = new double[n]; //точка после сжатия

			double[] x0 = new double[n]; //центр тяжести

			do
			{
				//Вычисляем F в точках х
				for (int i = 0; i < f.Length; i++)
				{
					f[i] = F(x[i]);
				}

				x0 = new double[n];

				xh_ind = Array.IndexOf(f, f.Max());
				xg_ind = Array.IndexOf(f, f.Where(y => y != f[xh_ind]).Max());
				xl_ind = Array.IndexOf(f, f.Min());

				for (int i = 0; i < n + 1; i++)
				{
					if (i != xh_ind)
						for (int j = 0; j < x0.Length; j++)
							x0[j] += x[i][j]/n;
				}
				double Fx0 = F(x0);

				double[] x0_ = x0.Select(y => y*(1 + alpha)).ToArray();
				double[] xh_ = x[xh_ind].Select(y => y*alpha).ToArray();
				xr = x0_.Select((y, i) => y - xh_[i]).ToArray();

				if (F(xr) < F(x[xl_ind]))
				{
					double[] xr_ = xr.Select(y => y*gamma).ToArray();
					x0_ = x0.Select(y => y*(1 - gamma)).ToArray();
					xe = xr_.Select((y, i) => y + x0_[i]).ToArray();
					if (F(xe) < F(x[xl_ind]))
					{
						x[xh_ind] = xe;
						f[xh_ind] = F(xe);
						if (Сonvergence(eps, f)) return x[xl_ind];
						continue;
					}
					else
					{
						x[xh_ind] = xr;
						f[xh_ind] = F(xr);
						if (Сonvergence(eps, f)) return x[xl_ind];
						continue;
					}
				}
				else
				{
					if (F(xr) < f[xg_ind])
					{
						x[xh_ind] = xr;
						f[xh_ind] = F(xr);
						if (Сonvergence(eps, f)) return x[xl_ind];
						continue;
					}
					else
					{
						//шаг Е
						if (F(xr) > f[xh_ind])
						{
						}
						else
						{
							x[xh_ind] = xr;
							f[xh_ind] = F(xr);
						}
						double[] xh_beta = x[xh_ind].Select(y => y * beta).ToArray();
						double[] x0_beta = x0.Select(y => y * (1 - beta)).ToArray();
						xc = xh_beta.Select((y, i) => y + x0_beta[i]).ToArray();
					}
				}
				if (F(xc) < f[xh_ind])
				{
					x[xh_ind] = xc;
					f[xh_ind] = F(xc);
					if (Сonvergence(eps, f)) return x[xl_ind];
				}
				else
				{
					//уменьшаем размер симплекса Ж3
					for (int i = 0; i < n + 1; i++)
					{
						x[i] = x[i].Select((m, j) => (m - x[xl_ind][j])/2).ToArray();
						f[i] = F(x[i]);
					}
					if (Сonvergence(eps, f)) return x[xl_ind];
				}
			}
			while (!Сonvergence(eps, f)) ;

			return x[xh_ind];
		}

	}
}
