using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FIeldDataAnalyzer;

namespace FieldDataAnalyzer
{
	class FileParser
	{

		public FileParser()
		{

		}

		public FieldDescription ParseGeneralData(string filename)
		{			
			var sr = new StreamReader(filename);
			return new FieldDescription()
			{
				Csm = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				V = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Ro = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Gg = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Pr = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Tsb = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Thickness = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				ThicknessIsol = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				ThicknessSnow = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				Depth = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				LyambdaTr = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				LyambdaIs = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				LyambdaSn = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0]),
				LyambdaGr = Convert.ToDouble(sr.ReadLine().Split(new char[] { ' ', '\t' })[0])
			};
		}

		public List<string[]> ParseSchema(string filename)
		{
			var result = new List<string[]>();
			var sr = new StreamReader(filename);
			string[] buf;
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine().Split(new char[] {' ', '\t'});
				if (buf.Count() != 0)
					result.Add(new string[]{buf[0], buf[1]});
			}
			return result;
		}

		public List<PipeData> ParsePipes(string filename)
		{
			Converter converter = new Converter();
			var result = new List<PipeData>();
			var sr = new StreamReader(filename);
			string[] buf;
			sr.ReadLine();
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine().Split(new char[] { ' ', '\t' });
				if (buf.Count() != 0)
					result.Add(new PipeData()
					{
						Num = Convert.ToInt32(buf[0]),
						Length = Convert.ToDouble(buf[1]),
						OuterD = converter.ToM(Convert.ToDouble(buf[2]), Units.Mm),
						Width = converter.ToM(Convert.ToDouble(buf[3]), Units.Mm),
						InnerD = converter.ToM(Convert.ToDouble(buf[4]), Units.Mm),
						Roughness = converter.ToM(Convert.ToDouble(buf[5]), Units.Mm),
						StartNode = buf[6],
						EndNode = buf[7],
						OuterT = converter.ToK(Convert.ToDouble(buf[8]), Units.C)
					});
			}
			return result;
		}

		public List<string[]> ParseWells(string filename)
		{
			var result = new List<string[]>();
			var sr = new StreamReader(filename);
			string[] buf;
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine().Split(new char[] { ' ', '\t' });
				if (buf.Count() != 0)
					result.Add(new string[] { buf[0], buf[1] });
			}
			return result;
		}

		public List<WellData> ParseSkv(string filename)
		{
			Converter converter = new Converter();

			var result = new List<WellData>();
			var sr = new StreamReader(filename);
			string[] buf;
			sr.ReadLine();
			sr.ReadLine();
			sr.ReadLine();
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine().Split(new char[] { ' ', '\t' });
				if ((buf.Count() == 8)&&(buf.Count(x => x == "") == 0))
					result.Add(new WellData
					{
						Name = buf[0],
						Data = new wells_measurements()
						{
							measure_date = Convert.ToDateTime(buf[1]),
							gas_output = (float)(Convert.ToDouble(buf[2]) * 1000 / converter.ToSec(1, Units.Day)),
							cond_output = (float)(converter.ToKG(Convert.ToDouble(buf[3]), Units.T) / converter.ToSec(1, Units.Day)),
							wellhead_P = (float)(converter.ToPascal(Convert.ToDouble(buf[4]))),
							wellhead_T = (float)(converter.ToK(Convert.ToDouble(buf[5]), Units.C)),
							inlet_P = (float)(converter.ToPascal(Convert.ToDouble(buf[6]))),
							inlet_T = (float)(converter.ToK(Convert.ToDouble(buf[7]), Units.C))
						}
					});
			}
			return result;
		}

		public List<SborData> ParseSbor(string filename)
		{
			Converter converter = new Converter();

			var result = new List<SborData>();
			var sr = new StreamReader(filename);
			string[] buf;
			sr.ReadLine();
			sr.ReadLine();
			sr.ReadLine();
			while (!sr.EndOfStream)
			{
				buf = sr.ReadLine().Split(new char[] { ' ', '\t' });
				if ((buf.Count() == 3)&&(buf.Count(x => x == "") == 0))
					result.Add(new SborData
					{
						Date = Convert.ToDateTime(buf[0]),
						P = converter.ToPascal(Convert.ToDouble(buf[1])),
						T = converter.ToK(Convert.ToDouble(buf[2]), Units.C)
					});
			}
			return result;
		}
	}

	public class PipeData
	{
		public int Num;
		public double Length; //длина участка трубопровода, м
		public double OuterD; //внешний диаметр по ГОСТ, мм
		public double Width; // Толщина стенки, мм
		public double InnerD; //Внутренний диаметр, мм
		public double Roughness; //Шероховатость, мм
		public string StartNode; //начальный узел
		public string EndNode; //конечный узел
		public double OuterT; //температура окружающей среды, градусы Цельсия
	}

	public class WellData
	{
		public wells_measurements Data { get; set; }
		//public int Id { get; set; }
		public string Name { get; set; }
		//public DateTime Date { get; set; }
		//public double G_gas { get; set; } //дебит газа, тыс. м3/сут (ст.)
		//public double G_condensat { get; set; } //дебит конденсата, т/сут (ст.)
		//public double P_ust { get; set; } //давление устьевое, кг/см2
		//public double T_ust { get; set; } //температура устьевая, градусы Цельсия
		//public double P_shl { get; set; } //давление (шл.), кг/см2 
		//public double T_shl { get; set; } //температура (шл.), градусы Цельсия
		public double[] a { get; set; }
		public double[] b { get; set; } //коэффициенты первой и второй параболы
		public double[] G { get; set; }
		public double[] P { get; set; }//значения массового расхода и давления (точки на графике)
		public double Gkr { get; set; }
		public double Pkr { get; set; }
		public double d_sht { get; set; }
		public double Gl { get; set; }
		public double Gg { get; set; }
		public shtutzer Shtutzer { get; set; }
	}

	public class SborData
	{
		public DateTime Date;
		public double P; //давление, кг/см2
		public double T; //температура, , градусы Цельсия
	}
}
