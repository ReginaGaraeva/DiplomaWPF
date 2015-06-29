using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace FieldDataAnalyzer
{
	internal class Interpolator
	{
		private List<InterpolationData> Data = new List<InterpolationData>();

		public Interpolator(string filename)
		{
			StreamReader sr = new StreamReader(filename);
			string buf = "";
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine();
				if (buf.IndexOf("PVT") == 0)
				{
					InterpolationData data = new InterpolationData {T = Convert.ToDouble(sr.ReadLine().Split(' ')[0])};
					buf = sr.ReadLine();
					while (buf != "/")
					{
						string[] values = buf.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
						if (values.Count() != 7)
						{
							buf = sr.ReadLine();
							continue;
						}
						data.Data.Add(new InnerInterpolationData
						{
							P = Convert.ToDouble(values[0].Replace(".",",")),
							ZData = new ZInterpolationData
							{
								Ro = Convert.ToDouble(values[1].Replace(".", ",")),
								V = Convert.ToDouble(values[2].Replace(".", ",")),
								C = Convert.ToDouble(values[3].Replace(".", ",")),
								G_gas = Convert.ToDouble(values[4].Replace(".", ",")),
								Ro_gas = Convert.ToDouble(values[5].Replace(".", ",")),
								Ro_cond = Convert.ToDouble(values[6].Replace(".", ","))
							}
						});
						buf = sr.ReadLine();
					}
					Data.Add(data);
				}
			}
		}


		public ZInterpolationData FindValue(double T, double P)
		{
			if (Data.Where(x => x.T <= T).ToList().Count == 0) T = Data.Min(x => x.T);
			else
				if (Data.Where(x => x.T > T).ToList().Count == 0) T = Data.Max(x => x.T);

			InterpolationData Tmin = Data.Last(x => x.T <= T);
			InterpolationData Tmax = Data.First(x => x.T > T);

			InnerInterpolationData PminTmin = Tmin.Data.Where(x => x.P <= P).ToList().Count == 0 ? Tmin.Data.First(x => x.P == Tmin.Data.Min(y => y.P)) : Tmin.Data.Last(x => x.P <= P);
			InnerInterpolationData PmaxTmin = Tmin.Data.Where(x => x.P > P).ToList().Count == 0 ? Tmin.Data.First(x => x.P == Tmin.Data.Max(y => y.P)) : Tmin.Data.First(x => x.P > P);
			InnerInterpolationData PminTmax = Tmax.Data.Where(x => x.P <= P).ToList().Count == 0 ? Tmax.Data.First(x => x.P == Tmax.Data.Min(y => y.P)) : Tmax.Data.Last(x => x.P <= P);
			InnerInterpolationData PmaxTmax = Tmax.Data.Where(x => x.P > P).ToList().Count == 0 ? Tmax.Data.First(x => x.P == Tmax.Data.Max(y => y.P)) : Tmax.Data.First(x => x.P > P);

			double TmaxTmin = Tmax.T - Tmin.T;
			double TmaxT = Tmax.T - T;
			double PmaxPmin = PmaxTmin.P - PminTmin.P;
			double PmaxP = PmaxTmin.P - P;
			double TTmin = T - Tmin.T;
			double PPmin = P - PminTmin.P;


			return new ZInterpolationData()
			{
				Ro = PminTmin.ZData.Ro/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				     PminTmax.ZData.Ro/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				     PmaxTmin.ZData.Ro/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				     PmaxTmax.ZData.Ro/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin,
				V = PminTmin.ZData.V/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				    PminTmax.ZData.V/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				    PmaxTmin.ZData.V/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				    PmaxTmax.ZData.V/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin,
				C = PminTmin.ZData.C/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				    PminTmax.ZData.C/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				    PmaxTmin.ZData.C/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				    PmaxTmax.ZData.C/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin,
				G_gas = PminTmin.ZData.G_gas/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				        PminTmax.ZData.G_gas/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				        PmaxTmin.ZData.G_gas/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				        PmaxTmax.ZData.G_gas/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin,
				Ro_gas = PminTmin.ZData.Ro_gas/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				         PminTmax.ZData.Ro_gas/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				         PmaxTmin.ZData.Ro_gas/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				         PmaxTmax.ZData.Ro_gas/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin,
				Ro_cond = PminTmin.ZData.Ro_cond/((TmaxTmin)*(PmaxPmin))*TmaxT*PmaxP +
				          PminTmax.ZData.Ro_cond/((TmaxTmin)*(PmaxPmin))*TTmin*PmaxP +
				          PmaxTmin.ZData.Ro_cond/((TmaxTmin)*(PmaxPmin))*TmaxT*PPmin +
				          PmaxTmax.ZData.Ro_cond/((TmaxTmin)*(PmaxPmin))*TTmin*PPmin
			};
		}
	}

	internal class InterpolationData
	{
		public double T; //температура, градусы Цельсия
		public List<InnerInterpolationData> Data = new List<InnerInterpolationData>();
	}

	internal class InnerInterpolationData
	{
		public double P; // давление, бар
		public ZInterpolationData ZData = new ZInterpolationData();
	}

	internal class ZInterpolationData
	{
		public double Ro; // Плотность, кг/м3.   
		public double V; // Кин. вязкость, м2/с.
		public double C; // Теплоемкость, Дж/кг*К.
		public double G_gas; // Объемное газосодержание, м3/м3.
		public double Ro_gas; // Плотность газа, кг/м3.
		public double Ro_cond; // Плотность жидкой фазы, кг/м3
	}
}