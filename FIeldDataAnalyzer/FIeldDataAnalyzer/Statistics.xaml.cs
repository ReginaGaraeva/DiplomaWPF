using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FIeldDataAnalyzer
{
	/// <summary>
	/// Interaction logic for Statistics.xaml
	/// </summary>
	public partial class Statistics : Window
	{
		private FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();
		private List<wells_measurements> data = new List<wells_measurements>();

		public Statistics()
		{
			InitializeComponent();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			var wells = db.wells.Select(x => new
			{
				x.well_id,
				x.name
			}).ToList();
			wellsComboBox.ItemsSource = wells;
			wellsComboBox.SelectedValuePath = "well_id";
			wellsComboBox.DisplayMemberPath = "name";

			wellChart.ChartAreas.Add(new ChartArea("Default"));
			wellChart.Series.Add("T1");
			wellChart.Series.Add("T2");
			wellChart.Series.Add("P1");
			wellChart.Series.Add("P2");
			wellChart.Series.Add("G");

			wellChart.Series["T1"].ChartArea = "Default";
			wellChart.Series["T2"].ChartArea = "Default";
			wellChart.Series["P1"].ChartArea = "Default";
			wellChart.Series["P2"].ChartArea = "Default";
			wellChart.Series["G"].ChartArea = "Default";

			wellChart.Series["T1"].ChartType = SeriesChartType.Spline;
			wellChart.Series["T2"].ChartType = SeriesChartType.Spline;
			wellChart.Series["P1"].ChartType = SeriesChartType.Spline;
			wellChart.Series["P2"].ChartType = SeriesChartType.Spline;
			wellChart.Series["G"].ChartType = SeriesChartType.Spline;
			
		}

		private void wellsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int well_id = (int)wellsComboBox.SelectedValue;
			if (well_id <= 0) return;
			
			data = db.wells_measurements.Where(x => x.well_id == well_id).ToList();
			wellChart.Series["T1"].Points.Clear();
			wellChart.Series["T2"].Points.Clear();
			wellChart.Series["P1"].Points.Clear();
			wellChart.Series["P2"].Points.Clear();
			wellChart.Series["G"].Points.Clear();
			if (T1CheckBox.IsChecked == true)
			{				
				foreach (var d in data)
				{
					wellChart.Series["T1"].Points.AddXY(d.measure_date, d.wellhead_T);
				}
			}
			if (T2CheckBox.IsChecked == true)
			{
				wellChart.Series.Add("T2");
				foreach (var d in data)
				{
					wellChart.Series["T2"].Points.AddXY(d.measure_date, d.inlet_T);
				}
			}
			if (P1CheckBox.IsChecked == true)
			{
				foreach (var d in data)
				{
					wellChart.Series["P1"].Points.AddXY(d.measure_date, d.wellhead_P);
				}
			}
			if (P2CheckBox.IsChecked == true)
			{
				foreach (var d in data)
				{
					wellChart.Series["P2"].Points.AddXY(d.measure_date, d.inlet_P);
				}
			}
			if (GCheckBox.IsChecked == true)
			{
				foreach (var d in data)
				{
					wellChart.Series["G"].Points.AddXY(d.measure_date, d.cond_output + d.gas_output);
				}
			}
		}

		private void T1CheckBox_Checked(object sender, RoutedEventArgs e)
		{

			//wellChart.Series["T1"].Points.Clear();
			foreach (var d in data)
			{
				wellChart.Series["T1"].Points.AddXY(d.measure_date, d.wellhead_T);
			}
		}

		private void T1CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			wellChart.Series["T1"].Points.Clear();
		}

		private void T2CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			wellChart.Series["T2"].Points.Clear();
		}

		private void P1CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			wellChart.Series["P1"].Points.Clear();
		}

		private void P2CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			wellChart.Series["P2"].Points.Clear();
		}

		private void GCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			wellChart.Series["G"].Points.Clear();
		}

		private void T2CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			foreach (var d in data)
			{
				wellChart.Series["T2"].Points.AddXY(d.measure_date, d.inlet_T);
			}
		}

		private void P1CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			foreach (var d in data)
			{
				wellChart.Series["P1"].Points.AddXY(d.measure_date, d.wellhead_P);
			}
		}

		private void P2CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			foreach (var d in data)
			{
				wellChart.Series["P2"].Points.AddXY(d.measure_date, d.inlet_P);
			}
		}

		private void GCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			foreach (var d in data)
			{
				wellChart.Series["G"].Points.AddXY(d.measure_date, d.cond_output + d.gas_output);
			}
		}
	}
}
