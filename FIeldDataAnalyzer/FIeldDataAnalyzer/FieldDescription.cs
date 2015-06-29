using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldDataAnalyzer
{
	public class FieldDescription
	{
		public double Csm { get; set; } //удельная теплоемкость смеси, дж/кг/К
		public double V { get; set; }  //кинематическая вязкость смеси, м2/c
		public double Ro { get; set; }  //плотность смеси, кг/м3
		public double Gg { get; set; }  //относительная доля газа в массовом расходе смеси
		public double Pr { get; set; }  //число Прандтля газа
		public double Tsb { get; set; }  //температура на узле сбора (экспер), К
		public double Thickness { get; set; }  //толщина стенки трубы (средняя), м
		public double ThicknessIsol { get; set; }  //толщина изоляции (средняя), м
		public double ThicknessSnow { get; set; }  //толщина снежного покрова (средняя), м
		public double Depth { get; set; }  //глубина залегания трубопровода (средняя), м
		public double LyambdaTr { get; set; }  //к-т теплопроводности материала трубы, Вт/м/К
		public double LyambdaIs { get; set; }  //к-т теплопроводности изоляции, Вт/м/К
		public double LyambdaSn { get; set; }  //к-т теплопроводности снега, Вт/м/К
		public double LyambdaGr { get; set; }  //к-т теплопроводности грунта, Вт/м/К
	}
}
