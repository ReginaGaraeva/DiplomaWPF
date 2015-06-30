using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using FIeldDataAnalyzer;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FieldDataAnalyzer
{
	internal class Evaluator
	{
		public double K_t = 1, K_p = 1;
		public Graph _graph;
		private Interpolator interpolator = new Interpolator("PVT.txt");
		private FieldDescription _fieldData;
		private DateTime _dateEvaluation;
		public ProgressBar progressBar;
		private FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();

		delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

		public Evaluator(Graph graph, FieldDescription fieldData, ProgressBar _toolStripProgressBar)
		{
			_graph = graph;
			_fieldData = fieldData;
			progressBar = _toolStripProgressBar;
		}

		private void CalcPipe(Pipe pipe)
		{
			if (pipe.StartNode.P == 0)
			{
				CalcNode(pipe.StartNode);
			}

			if (pipe.StartNode.P == 0) return;
			ZInterpolationData interpolatedParams = interpolator.FindValue((double) pipe.StartNode.T - 273,
				(double) pipe.StartNode.P*1e-5);
			double Gi = (double) pipe.StartNode.G;

			// 1. Расчёт скорости смеси
			double V = Gi/(interpolatedParams.Ro*Math.PI*Math.Pow(pipe.Data.inner_d, 2)/4);
			// 2. Расчёт числа Рейнольса
			double Re = V * pipe.Data.inner_d / interpolatedParams.V;
			// 3. Коэффициент теплоотдачи от газожидкостной смеси к трубе
			double alpha = 0.021 * interpolatedParams.C / pipe.Data.inner_d * Math.Pow(Re, 0.8) * Math.Pow(_fieldData.Pr, 0.43);
			// 4.	Условный коэффициент теплоотдачи от теплоизоляции в грунт
			double alpha_gr = 2 * _fieldData.LyambdaGr / (pipe.Data.outer_d + 2 * _fieldData.ThicknessIsol) / Math.Log(2 * _fieldData.Depth
			                                                                                                  /pipe.Data.outer_d + Math.Sqrt(Math.Pow(2* _fieldData.Depth/pipe.Data.outer_d, 2) - 1));
			// 5.	Коэффициент теплопередачи (расчётный)
			double lyambdaOkr = 5; //коэффициент теплоотдачи откружающей среды
			double ThicknessSnow = 0;
			double K_tp = 1/(1/alpha + _fieldData.Thickness/_fieldData.LyambdaTr + _fieldData.ThicknessIsol
							 / _fieldData.LyambdaIs + ThicknessSnow / _fieldData.LyambdaSn + 1 / lyambdaOkr);

			// 6.	Температура на выходе
			double alpha_tr = 1.0*K_t*K_tp*Math.PI*pipe.Data.inner_d /Gi/_fieldData.Csm; //коэффициент Шухова

			double dt = (1 - Math.Exp(-alpha_tr * pipe.Data.length)) * ((double)pipe.StartNode.T - pipe.Data.temper);
			pipe.EndNode.T += (pipe.StartNode.T - dt)*pipe.StartNode.G;

			// 6а.	Давление на выходе
			double lyambda_0_tr = 0.067*Math.Pow(158/Re + 2*pipe.Data.roughness/pipe.Data.inner_d, 0.2);
			//double big_unkown_coef = lyambda_0_tr * pipe.Data.Length / pipe.Data.InnerD * interpolatedParams.Ro / Math.Pow(interpolatedParams.Ro * Math.PI * Math.Pow(pipe.Data.InnerD, 2) / 4, 2) / 2;
			double dp = lyambda_0_tr*pipe.Data.length/pipe.Data.inner_d*interpolatedParams.Ro*V*V/2;
			if (pipe.StartNode.P > dp) pipe.EndNode.P += (pipe.StartNode.P - K_p * dp) * pipe.StartNode.G;
			pipe.EndNode.G += pipe.StartNode.G;
		}

		private void CalcNode(Node node)
		{
			if (node.wells.Count != 0)
			{
				double TG = 0, PG = 0, G = 0;
				foreach (var well in node.wells)
				{
					if (well.Data.gas_output + well.Data.cond_output != 0) CalcShtutzer(well);
					TG += well.Data.inlet_T * (well.Gg + well.Gl);
					PG += well.Data.inlet_P * (well.Gg + well.Gl);
					G += (well.Gg + well.Gl);
				}
				if (G == 0)
				{
					node.T = 0;
					node.P = 0;
				}
				else
				{
					node.T = TG/G;
					node.P = PG/G;
				}
				node.G = G;
			}
			else
			{
				var _pipes = _graph.pipes.Where(x => x.EndNode == node).ToList();
				if (_pipes.Count == 0)
				{
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
					if (node.G != 0)
					{
						node.T /= node.G;
						node.P /= node.G;
					}
					else
					{
						node.T = 0;
						node.P = 0;
					}
				}
			}
		}

		public void CalcGraph(DateTime date)
		{
			_graph.Clear();
			foreach (var wellMeasurement in db.wells_measurements.Where(x => x.measure_date == date.Date).ToList()) //заполнение данных о скважинах за данный день
			{

				var well = _graph.wells.First(x => x.Data.well_id == wellMeasurement.well_id);
				well.Data = wellMeasurement;
				well.a = new double[] { -4f / 5, -1f / 5, 1f };
				well.b = new double[] { -12f / 5, 11f / 5, 1f / 5 };
			}
			CalcNode(_graph.endNode);
		}

		public LearningResult Calc(DateTime fromDate, DateTime toDate)
		{
			UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(progressBar.SetValue);

			double[] Ks = { 0.5, 1, 2 }; //массив коэффициентов для интерполирования
			LearningResult result = new LearningResult();
			result.GPResults = new List<GPLearningResult>();
			var datesRange = db.wells_measurements.Where(x => (x.measure_date >= fromDate) && (x.measure_date <= toDate)).Select(y => new { Date = y.measure_date }).Distinct().ToList();

			var dates = (from date in datesRange
						 join meas in db.final_gather_point_measurements on date.Date equals meas.measure_date
						 select new { Date = date.Date }).ToList();
			if (!dates.Any())
			{
				return result;
			}
			progressBar.Maximum = dates.Count;
			progressBar.Minimum = 0;
			progressBar.Value = 0;
			double value = 0;
			foreach (var date in dates)
			{
				value++;
				Dispatcher.CurrentDispatcher.Invoke(updProgress, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

				double[,] _PTk = new double[Ks.Length, 2];
				//foreach (var wellMeasurement in db.wells_measurements.Where(x => x.measure_date == date.Date).ToList()) //заполнение данных о скважинах за данный день
				//{

				//	var well = _graph.wells.First(x => x.Data.well_id == wellMeasurement.well_id);
				//	well.Data = wellMeasurement;
				//	well.a = new double[] { -4f / 5, -1f / 5, 1f };
				//	well.b = new double[] { -12f / 5, 11f / 5, 1f / 5 };
				//}

				List<GPResult> gpResults = new List<GPResult>();

				for (int i = 0; i < Ks.Length; i++) //расчет P и T для каждого из значений коэффициентов
				{
					K_p = Ks[i];
					K_t = Ks[i];
					CalcGraph(date.Date);
					//CalcNode(_graph.endNode);
					_PTk[i, 0] = _graph.endNode.P;
					_PTk[i, 1] = _graph.endNode.T;

					if (K_p == 1)
					{
						foreach (var n in _graph.nodes)
						{
							gpResults.Add(new GPResult()
							{
								G = n.G,
								Id = n.Id,
								Name = n.Name,
								NextNodeName = n.NextNode != null ? n.NextNode.Name : "",
								Pf = n.P,
								Tf = n.T
							});
						}
					}

					_graph.Clear();

					//foreach (var wellMeasurement in db.wells_measurements.Where(x => x.measure_date == date.Date).ToList()) //заполнение данных о скважинах за данный день
					//{
					//	var well = _graph.wells.First(x => x.Data.well_id == wellMeasurement.well_id);
					//	well.Data = wellMeasurement;
					//	well.a = new double[] { -4f / 5, -1f / 5, 1f };
					//	well.b = new double[] { -12f / 5, 11f / 5, 1f / 5 };
					//}
				}

				var realResults = db.final_gather_point_measurements.FirstOrDefault(x => x.measure_date == date.Date);
				double _Kt = GetInterpolatedValue(realResults.Texper, new [] {_PTk[0, 1], _PTk[1, 1], _PTk[2, 1]}, Ks);
				double _Kp = GetInterpolatedValue(realResults.Pexper, new [] {_PTk[0, 0], _PTk[1, 0], _PTk[2, 0]}, Ks);
				K_p = _Kp;
				K_t = _Kt;
				CalcNode(_graph.endNode);

				foreach (var n in _graph.nodes)
				{
					var r = gpResults.First(x => x.Id == n.Id);
					r.Pcoef = n.P;
					r.Tcoef = n.T;
				}

				result.GPResults.Add(new GPLearningResult()
				{
					Date = date.Date,
					Pexpr = realResults.Pexper,
					Texpr = realResults.Texper,
					Pf = _PTk[1, 0],
					Tf = _PTk[1, 1],
					Coef_T = _Kt,
					Coef_P = _Kp,
					Pcoef = _graph.endNode.P,
					Tcoef = _graph.endNode.T,
					G = _graph.endNode.G,
					WellMeasurements = db.wells_measurements.Where(x => EntityFunctions.TruncateTime(x.measure_date) == date.Date).ToList(),
					GPMeasurements = gpResults
				});
				
				_graph.Clear();
				//break;
			}
			result.Coef_P = result.GPResults.Average(x => x.Coef_P);
			result.Coef_T = result.GPResults.Average(x => x.Coef_T);
			//progressBar.IsIndeterminate = false;
			return result;
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

		public void CalcShtutzer(WellData well)
		{
			if (well.Shtutzer.d_sht_current <= 0)
			{
				well.Gl = 0;
				well.Gg = 0;
				return;
			}
			float G = (well.Data.cond_output + well.Data.gas_output);
			well.Gl = well.Data.cond_output * (1 - well.Shtutzer.d_sht_current*1.5);
			well.Gg = (float)(G - well.Gl);
			//G = 24;
			//well.Shtutzer.d_sht_current = 0.116f;
			//well.Data.wellhead_T = 328.87f;
			//well.Data.wellhead_P = 4132733.47f;
			//double R = 8.314/well.Shtutzer.M;
			//double S1 = Math.PI*well.Shtutzer.d1*well.Shtutzer.d1/4;
			//double V1 = G/(well.Shtutzer.ro*S1);
			//double Cpg = well.Shtutzer.k * R/(well.Shtutzer.k - 1);
			//double T1z = well.Data.wellhead_T + V1*V1/(2*Cpg);
			//double P1z = well.Data.wellhead_P * Math.Pow(T1z/well.Data.wellhead_T, well.Shtutzer.k/(well.Shtutzer.k - 1));
			//double phi = (well.Shtutzer.ro_l - well.Shtutzer.ro)/(well.Shtutzer.ro_l - well.Shtutzer.ro_g);
			//double betag = phi;
			//double Q1 = G/well.Shtutzer.ro;
			//double Qg1 = betag*Q1;
			//double Gg1 = Qg1*well.Shtutzer.ro_g;
			//double Gl1 = G - Gg1;
			//double akr = Math.Sqrt(2*well.Shtutzer.k*R*T1z/(well.Shtutzer.k + 1));
			//double Pshz = P1z -
			//			  (1 - well.Shtutzer.d_sht_current*well.Shtutzer.d_sht_current/well.Shtutzer.d1/well.Shtutzer.d1)*
			//			  well.Shtutzer.ro*V1*V1/4;
			//double rk = 2/(well.Shtutzer.k + 1);
			//double cd = well.Shtutzer.k/(well.Shtutzer.k - 1);
			//double betakrg = Math.Pow(rk, cd);
			//double f1 = Math.Sqrt(2*well.Shtutzer.k/(well.Shtutzer.k + 1))*
			//			Math.Pow((well.Shtutzer.k + 1)/2, 1/(well.Shtutzer.k - 1));
			//double Gkrg = f1*Pshz/Math.Sqrt(R*T1z)*Math.PI*well.Shtutzer.d_sht_current*well.Shtutzer.d_sht_current/4;
			//if (G > Gkrg) G = Gkrg;
			//double Gch = G/Gkrg;
			//double c = 1/((1 - betakrg)*betakrg);
			//double Psh = Pshz*(0.5 + Math.Sqrt(0.25 - Gch*Gch/c));
			//double q = Gch;
			//double f2 = Math.Sqrt((well.Shtutzer.k + 1)/(well.Shtutzer.k - 1))*
			//			Math.Pow((well.Shtutzer.k + 1)/2, 1/(well.Shtutzer.k + 1));
			//double P2sh = Pshz*
			//			  Math.Pow(q/(f2*Math.Pow((1 - Psh/Pshz), (well.Shtutzer.k - 1)/(2*well.Shtutzer.k))), well.Shtutzer.k);
			//double eP = 0.1; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//while (Math.Abs(P2sh - Psh)/well.Data.wellhead_P >= eP)
			//{
			//	Psh = P2sh;
			//	P2sh = Pshz*Math.Pow(q/(f2*Math.Pow((1 - Psh/Pshz), (well.Shtutzer.k - 1)/(2*well.Shtutzer.k))), well.Shtutzer.k);
			//}
			//double betakrgks = 0.4 + 10*(betakrg - 0.4)*(betag - 0.9);
			//double xch = (betag - 0.9)*10;
			//double y = 0;
			//if (xch <= 0.5)
			//	y = 0.278*xch;
			//else
			//{
			//	double x1 = 2*(xch - 0.5);
			//	y = 0.139 - 0.283*x1 + 1.44*x1*x1;
			//}
			//double Gkrgks = Gkrg*(0.706*y + 0.294);
			//if (G > Gkrgks)
			//{

			//}
			//else
			//{
			//	double pi = P2sh/Pshz;
			//	double lyash =
			//		Math.Sqrt((well.Shtutzer.k + 1)/(well.Shtutzer.k - 1)*(1 - Math.Pow(pi, (well.Shtutzer.k - 1)/well.Shtutzer.k)));
			//	double Vsh = lyash*akr;
			//	double Tsh = T1z*(1 - (well.Shtutzer.k - 1)/(well.Shtutzer.k + 1)*lyash*lyash);
			//	double Rosh = Pshz*Math.Pow(pi, 1/well.Shtutzer.k)/(R*T1z);
			//	double P2z = Pshz -
			//				 Math.Pow(
			//					 well.Shtutzer.d2*well.Shtutzer.d2/well.Shtutzer.d_sht_current/well.Shtutzer.d_sht_current - 1, 2)*Rosh*
			//				 Vsh*Vsh/2;
			//	double ro2 = Rosh;
			//	double V2 = G*4/(ro2*Math.PI*well.Shtutzer.d2*well.Shtutzer.d2);
			//	double lya2 = V2/akr;
			//	double P2 = P2z*
			//				Math.Pow(1 - (well.Shtutzer.k - 1)/(well.Shtutzer.k + 1)*lya2*lya2,
			//					well.Shtutzer.k/(well.Shtutzer.k - 1));
			//	double T2 = T1z*(1 - (well.Shtutzer.k - 1)/(well.Shtutzer.k + 1)*lya2*lya2);
			//	ro2 = P2/(well.Shtutzer.z*R*T2);
			//	double eRO = 0.001;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//	while (Math.Abs(ro2 - Rosh)/Rosh >= eRO)
			//	{
			//		Rosh = ro2;
			//		V2 = G * 4 / (ro2 * Math.PI * well.Shtutzer.d2 * well.Shtutzer.d2);
			//		lya2 = V2/akr;
			//		P2 = P2z * Math.Pow(1 - (well.Shtutzer.k - 1) / (well.Shtutzer.k + 1) * lya2 * lya2,
			//					well.Shtutzer.k / (well.Shtutzer.k - 1));
			//		T2 = T1z * (1 - (well.Shtutzer.k - 1) / (well.Shtutzer.k + 1) * lya2 * lya2);
			//		ro2 = P2/(R*T2*well.Shtutzer.z);
			//	}
			//	double betag2 = (well.Shtutzer.ro_l - ro2) / (well.Shtutzer.ro_l - well.Shtutzer.ro_g);//!!!!!!!!!!!!!!!!!!!!!!
			//	double Q2 = G/ro2;
			//	double Qg2 = betag2*Q2;
			//	double Ql2 = (1 - betag2)*Q2;
			//	double Gg2 = Qg2 * well.Shtutzer.ro_g;////////////////ROg2
			//	double Gl2 = G - Gg2;
			//	double T2rv = (Cpg*Gg1 + well.Shtutzer.C_l*Gl1)/(Cpg*Gg2 + well.Shtutzer.C_l*Gl2)*well.Data.wellhead_T +
			//				  well.Shtutzer.D/(Cpg*Gg2 + well.Shtutzer.C_l*Gl2)*(well.Data.wellhead_P*Gg1 - P2*Gg2) +
			//				  1.1*(V1*V1 - V2*V2)*G/(2*(Cpg*Gg2 + well.Shtutzer.C_l*Gl2));
			//	double T2zam = well.Data.wellhead_T +
			//				   well.Shtutzer.D*Gg1*(well.Data.wellhead_P - P2)/(Cpg*Gg1 + well.Shtutzer.C_l*Gl1) +
			//				   1.1*(V1*V1 - V2*V2)*G/(2*(Cpg*Gg1 + well.Shtutzer.C_l*Gl1));
			//	well.Data.inlet_T = (float)(0.5*T2zam + 0.5*T2rv);
			//	well.Data.inlet_P = (float) P2;
			//	well.Gg = (float) Gg2;
			//	well.Gl = (float) Gl2;

		//}
			
		}
	}

	internal class LearningResult
	{
		public List<GPLearningResult> GPResults;
		public double Coef_T, Coef_P;
	}

	//class WellCharacterResult

	class GPResult
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double Pf { get; set; }
		public double Tf { get; set; }
		public double Pcoef { get; set; }
		public double Tcoef { get; set; }
		public double G { get; set; }
		public string NextNodeName { get; set; }
	}

	class GPLearningResult
	{
		public DateTime Date { get; set; }
		public double Pexpr { get; set; }
		public double Texpr { get; set; }
		public double Coef_P { get; set; }
		public double Coef_T { get; set; }
		public double Pcoef { get; set; }
		public double Tcoef { get; set; }
		public double Pf { get; set; }
		public double Tf { get; set; }
		public double G { get; set; }
		public List<GPResult> GPMeasurements;
		public List<wells_measurements> WellMeasurements;
	}
	
}
