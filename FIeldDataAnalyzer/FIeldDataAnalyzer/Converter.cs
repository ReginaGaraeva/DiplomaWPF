using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldDataAnalyzer
{
	class Converter
	{
		public double ToM(double value, Units unit)
		{
			switch (unit)
			{
				case Units.Mm:
					return value/1000;
				case Units.Sm:
					return value/100;
				default:
					throw new Exception("Uncorrect cast to SI");
			}
		}

		public double ToM2(double value, Units unit)
		{
			switch (unit)
			{
				case Units.Mm2:
					return value / 1000 / 1000;
				case Units.Sm2:
					return value / 100 / 100;
				default:
					throw new Exception("Uncorrect cast to SI");
			}
		}

		public double ToK(double value, Units unit)
		{
			switch (unit)
			{
				case Units.C:
					return value + 273;
				default:
					throw new Exception("Uncorrect cast to SI");
			}
		}

		public double ToKG(double value, Units unit)
		{
			switch (unit)
			{
				case Units.T:
					return value * 1000;
				default:
					throw new Exception("Uncorrect cast to SI");
			}
		}

		public double ToSec(double value, Units unit)
		{
			switch (unit)
			{
				case Units.Day:
					return value * 60 * 60 * 24;
				default:
					throw new Exception("Uncorrect cast to SI");
			}
		}

		public double ToPascal(double value) // кг/см^2 -> Pa
		{
			return value*1e5;
		}

	}

	public enum Units
	{
		Mm, //миллиметры
		Sm, //сантиметры
		Mm2, //миллиметры ^ 2
		Sm2, //сантиметры ^ 2
		T, //тонны
		C, //градусы Цельсия
		Day //сутки
	}
}
