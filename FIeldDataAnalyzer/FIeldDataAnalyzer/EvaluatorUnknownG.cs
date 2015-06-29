using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using System.Windows.Markup;
using CG_Lab04;
using System.Data.Objects;
using System.Windows.Controls;
using FIeldDataAnalyzer;

namespace FieldDataAnalyzer
{
	class EvaluatorUnknownG
	{
		public double K_t = 1, K_p = 1;
		private Graph _graph;
		private Interpolator interpolator = new Interpolator("PVT.txt");
		private FieldDescription _fieldData;
		private DateTime _dateEvaluation;
		public ProgressBar toolStripProgressBar;
		public double beta_kr = 0.6;
		private FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();

		public EvaluatorUnknownG(Graph graph, FieldDescription fieldData, ProgressBar _toolStripProgressBar)
		{
			_graph = graph;
			_fieldData = fieldData;
			toolStripProgressBar = _toolStripProgressBar;
		}

		private void CalcPipe(Pipe pipe)
		{
			if (pipe.StartNode.P_.Max(x => x) == 0)
			{
				CalcNode(pipe.StartNode);
			}

			if (pipe.EndNode.Name == "N5101")
			{
				string g = "";
			}

			//pipe.EndNode.P_ = new double[5];
			//pipe.EndNode.T_ = new double[5];
			//pipe.EndNode.G_ = new double[5];

			for (int m = 0; m < 5; m++)
			{
				if (pipe.StartNode.P_[m] == 0) return;

				ZInterpolationData interpolatedParams = interpolator.FindValue(pipe.StartNode.T_[m] - 273, pipe.StartNode.P_[m]*1e-5);
				double Gi = pipe.StartNode.G_[m];

				// 1. Расчёт скорости смеси
				double V = Gi/(interpolatedParams.Ro*Math.PI*Math.Pow(pipe.Data.inner_d, 2)/4);
				// 2. Расчёт числа Рейнольса
				double Re = V * pipe.Data.inner_d / interpolatedParams.V;
				// 3. Коэффициент теплоотдачи от газожидкостной смеси к трубе
				double alpha = 0.021 * interpolatedParams.C / pipe.Data.inner_d * Math.Pow(Re, 0.8) * Math.Pow(_fieldData.Pr, 0.43);
				// 4.	Условный коэффициент теплоотдачи от теплоизоляции в грунт
				double alpha_gr = 2*_fieldData.LyambdaGr/(pipe.Data.outer_d + 2*_fieldData.ThicknessIsol)/Math.Log(2*_fieldData.Depth
																												  / pipe.Data.outer_d +
																												  Math.Sqrt(
																													  Math.Pow(
																														  2*
																														  _fieldData.Depth/
																														  pipe.Data.outer_d,
																														  2) - 1));
				// 5.	Коэффициент теплопередачи (расчётный)
				double lyambdaOkr = 5; //коэффициент теплоотдачи откружающей среды
				double K_tp = 1/(1/alpha + _fieldData.Thickness/_fieldData.LyambdaTr + _fieldData.ThicknessIsol
								 /_fieldData.LyambdaIs + _fieldData.ThicknessSnow/_fieldData.LyambdaSn + 1/lyambdaOkr);

				// 6.	Температура на выходе
				double alpha_tr = 1.0 * K_t * K_tp * Math.PI * pipe.Data.inner_d / Gi / _fieldData.Csm; //коэффициент Шухова

				double dt = (1 - Math.Exp(-alpha_tr*pipe.Data.length))*((double) pipe.StartNode.T_[m] - pipe.Data.temper);
				pipe.EndNode.T_[m] += (pipe.StartNode.T_[m] - dt) * pipe.StartNode.G_[m];

				// 6а.	Давление на выходе
				double lyambda_0_tr = 0.067 * Math.Pow(158 / Re + 2 * pipe.Data.roughness / pipe.Data.inner_d, 0.2);
				double dp = lyambda_0_tr*pipe.Data.length/pipe.Data.inner_d*interpolatedParams.Ro*V*V/2;
				if (pipe.StartNode.P_[m] > dp) pipe.EndNode.P_[m] += (pipe.StartNode.P_[m] - K_p * dp) * pipe.StartNode.G_[m];
				pipe.EndNode.G_[m] += pipe.StartNode.G_[m];
		}
	}

		private void CalcNode(Node node)
		{
			if (node.wells.Count != 0)
			{
				WellData firstWell = null;
				double TG = 0, PG = 0, G = 0;
				for (int i = 0; i < node.wells.Count(); i++) //выполняем расчет G для всех скважин
				{
					
					node.wells[i].G = new double[5];
					node.wells[i].P = new double[5];
					if (node.wells[i].Gkr == 0) continue;
					if (firstWell == null) //если скважина первая, то считаем для нее давление
					{
						firstWell = node.wells[i];
						node.wells[i].P[0] = beta_kr * node.wells[i].Pkr;
						node.wells[i].G[0] = Math.Sqrt(node.wells[i].a[2])
							* node.wells[i].Gkr;
						for (int m = 1; m < 5; m++)
						{
							node.wells[i].P[m] = (m * 0.25 * (1 - beta_kr) + beta_kr) * node.wells[i].Pkr;
							node.wells[i].G[m] = Math.Sqrt(node.wells[i].a[0] * Math.Pow(m * 0.25,2) + node.wells[i].a[1] * m * 0.25 + node.wells[i].a[2])
								* node.wells[i].Gkr;
						}
					}
					else
					{
						for (int m = 0; m < 5; m++)
						{
							node.wells[i].G[m] = Math.Sqrt(node.wells[i].a[0] * Math.Pow(m * 0.25, 2) + node.wells[i].a[1] * m * 0.25 + node.wells[i].a[2])
								* node.wells[i].Gkr;
						}
					}
				}
				//считаем в узле сбора
				node.P_ = new double[5];
				node.G_ = new double[5];
				node.T_ = new double[5];

				if (firstWell == null)
				{
					node.T = 0;
					node.P = 0;
					return;
				}

				for (int i = 0; i < 5; i++)
				{
					double big_unkown_coef = 300;
					node.P_[i] = firstWell.P[i] - K_p * big_unkown_coef * firstWell.G[i];
					for (int j = 0; j < node.wells.Count(); j++)
						TG += node.wells[j].Data.inlet_T*node.wells[j].G[i];
					node.G_[i] = node.wells.Sum(x => x.G[i]);
					if (node.G_[i] == 0) node.T_[i] = 0;
						else node.T_[i] = TG/node.G_[i];
					TG = 0;
				}

				node.a = GetParabolCoefs(new[] {node.P_[0], node.P_[1], node.P_[2]}, new[] {node.G_[0], node.G_[1], node.G_[2]});
				node.b = GetParabolCoefs(new[] { node.P_[2], node.P_[3], node.P_[4] }, new[] { node.G_[2], node.G_[3], node.G_[4]});

				//считаем давление в остальных скважинах

				for (int j = 1; j < node.wells.Count(); j++) //выполняем расчет для первой скважины
				{
					for (int m = 1; m < 5; m++)
					{
						double big_unkown_coef = 300;
						node.wells[j].P[m] = firstWell.P[m] + K_p * big_unkown_coef * firstWell.G[m];
					}
				}
			}
			else
			{
				if (node.Name == "K336")
				{
					string f = "";
				}
				var _pipes = _graph.pipes.Where(x => x.EndNode == node).ToList();
				if (_pipes.Count == 0)
				{
					node.P_ = new double[5];
					node.G_ = new double[5];
					node.a = new double[5];
					node.b = new double[5];
					node.P = 0;
					node.T = 0;
					node.G = 0;
				}
				else
				{
					foreach (var pipe in _pipes)
					{
						CalcPipe(pipe);
					}
					for (int i = 0; i < 5; i++)
					{
						if (node.G_[i] == 0)
						{
							node.T_[i] = 0;
							node.P_[i] = 0;
						}
						else
						{
							node.T_[i] /= node.G_[i];
							node.P_[i] /= node.G_[i];
						}
					}
					if (node.P_.Max(x => x) == 0)
					{
						node.a = new double[5];
						node.b = new double[5];
					}
					else
					{
						node.a = GetParabolCoefs(new[] {node.P_[0], node.P_[1], node.P_[2]}, new[] {node.G_[0], node.G_[1], node.G_[2]});
						node.b = GetParabolCoefs(new[] {node.P_[2], node.P_[3], node.P_[4]}, new[] {node.G_[2], node.G_[3], node.G_[4]});
					}

				}
			}
		}

		public LearningResult Calc(DateTime fromDate, DateTime toDate)
		{
			double[] Ks = { 0.5, 1, 2 }; //массив коэффициентов для интерполирования
			LearningResult result = new LearningResult();
			result.GPResults = new List<GPLearningResult>();
			var datesRange = db.wells_measurements.Where(x => (x.measure_date >= fromDate) && (x.measure_date <= toDate)).Select(y => new { Date = y.measure_date }).Distinct().ToList();

			var dates = (from date in datesRange
				join meas in db.final_gather_point_measurements on date.Date equals meas.measure_date
				select new {Date = date.Date}).ToList();
			toolStripProgressBar.Maximum = dates.Count; 
			toolStripProgressBar.Minimum = 0;
			toolStripProgressBar.Value = 0;
			foreach (var date in dates)
			{
				toolStripProgressBar.Value++;
				//double[,] _PTk = new double[Ks.Length, 2];
				foreach (var wellMeasurement in db.wells_measurements.Where(x => x.measure_date == date.Date).ToList()) //заполнение данных о скважинах за данный день
				{
					var well = _graph.wells.First(x => x.Data.measurement_id == wellMeasurement.well_id);
					well.Data = wellMeasurement;
					//well.G_condensat = wellMeasurement.cond_output;
					//well.G_gas = wellMeasurement.gas_output;
					//well.P_shl = wellMeasurement.inlet_P;
					//well.P_ust = wellMeasurement.wellhead_P;
					//well.T_shl = wellMeasurement.inlet_T;
					//well.T_ust = wellMeasurement.wellhead_T;
					well.a = new double[] {-4f/5, -1f/5, 1f};
					well.b = new double[] { -12f / 5, 11f/5, 1f/5 };
					well.Gkr = (double) db.wells.First(x => x.well_id == wellMeasurement.well_id).Gkr;
					well.Pkr = (double) db.wells.First(x => x.well_id == wellMeasurement.well_id).Pkr;
				}

				CalcNode(_graph.endNode);
				break;
			}
			return new LearningResult();
		}

		double GetInterpolatedValue(double value, double[] y, double[] x)
		{
			double a = ((y[0] - y[1]) * (x[0] - x[2]) - (y[0] - y[2]) * (x[0] - x[1])) / ((x[0] - x[1]) * (x[0] - x[2]) * (x[1] - x[2]));
			double b = ((y[0] - y[1]) - a * (x[0] - x[1]) * (x[0] + x[1])) / (x[0] - x[1]);
			double c = y[0] - (a * x[0] + b) * x[0] - value;
			double d = b * b - 4 * a * c;
			double x1 = (-b - Math.Sqrt(d)) / 2 / a;
			double x2 = (-b + Math.Sqrt(d)) / 2 / a;
			if (x1 > 0) return x1;
			if (x2 > 0) return x2;
			return 0;
		}

		double[] GetParabolCoefs(double[] x, double[] y)
		{
			double[,] a = new double[3, 3];
			for (int i = 0; i < 3; i++)
			{
				a[i, 0] = Math.Pow(x[i], 2);
				a[i, 1] = x[i];
				a[i, 2] = 1;
			}
			bool correct = true;
			return Matrix.SLE_Gauss(a, y, out correct);
		}
	}
}
