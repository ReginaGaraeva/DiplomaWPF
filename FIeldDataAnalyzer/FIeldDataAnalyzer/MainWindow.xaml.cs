using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FieldDataAnalyzer;

namespace FIeldDataAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Graph graph = new Graph();
		private LearningResult results;
		public FieldDescription fieldDescription { get; set; }
		private DBService db = new DBService();
		private Optimizer optimizer;
		private FieldDataAnalyzerDBEntities entities = new FieldDataAnalyzerDBEntities();
		private Evaluator evaluator; 
		

		public MainWindow()
		{
			InitializeComponent();
			FileParser parser = new FileParser();
			fieldDescription = parser.ParseGeneralData("UKPG2_gen.txt");

			CsmTextBox.DataContext = this;
			VTextBox.DataContext = this;
			RoTextBox.DataContext = this;
			GgTextBox.DataContext = this; 
			PrTextBox.DataContext = this; 
			TsbTextBox.DataContext = this; 
			ThicknessTextBox.DataContext = this; 
			ThicknessIsolTextBox.DataContext = this;   
			ThicknessSnowTextBox.DataContext = this;
			DepthTextBox.DataContext = this;
			LyambdaTrTextBox.DataContext = this;
			LyambdaIsTextBox.DataContext = this;
			LyambdaSnTextBox.DataContext = this;
			LyambdaGrTextBox.DataContext = this;

			DateTime lastWellMeasDate = entities.wells_measurements.Max(x => x.measure_date);
			lastWellMeasurementsLabel.Content = String.Format("Последние измерения на скважинах {0}/{1}/{2}", lastWellMeasDate.Day, lastWellMeasDate.Month, lastWellMeasDate.Year);
			workingWellsLabel.Content = String.Format("Скважин работает {0} из {1}", entities.wells_measurements.Count(x => x.measure_date == lastWellMeasDate), entities.wells.Count());
			DateTime lastGPMeasDate = entities.final_gather_point_measurements.Max(x => x.measure_date);
			lastGPMeasurementsLabel.Content = String.Format("Последние измерения на УКПГ {0}/{1}/{2}", lastGPMeasDate.Day,
				lastGPMeasDate.Month, lastGPMeasDate.Year);
			KtLabel.Content = String.Format("Поправочный коэффициент для давления K_p {0}", entities.final_gather_point_measurements.First(x => x.measure_date == lastGPMeasDate).coef_P);
			KpLabel.Content = String.Format("Поправочный коэффициент для температуры K_t {0}", entities.final_gather_point_measurements.First(x => x.measure_date == lastGPMeasDate).coef_P);

			currShtutValuesDataGrid.ItemsSource = entities.shtutzers.ToList().Select(x => new
			{
				WellName = entities.wells.First(y => y.well_id == x.well_id).name,
				d_sht = x.d_sht_current,
				KGF = 0.5
			});

			evaluator = new Evaluator(graph, fieldDescription, coefsProgressBar);
		}

		private void startEvaluatorButton_Click(object sender, RoutedEventArgs e)
		{
			FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();
			DateTime dateFrom = new DateTime(), dateTo = new DateTime();

			if (allDataCheckBox.IsChecked == true)
			{
				dateFrom = db.wells_measurements.Min(x => x.measure_date);
				dateTo = db.wells_measurements.Max(x => x.measure_date);
			}
			else
			{
				if ((fromDatePicker.SelectedDate == null) || (toDatePicker.SelectedDate == null))
				{
					MessageBox.Show("Не выбран период расчета.");
					return;
				}
				dateFrom = (DateTime)fromDatePicker.SelectedDate;
				dateTo = (DateTime)toDatePicker.SelectedDate;
			}

			Evaluator evaluator = new Evaluator(graph, fieldDescription, coefsProgressBar);
			
			results = evaluator.Calc(dateFrom, dateTo);
			finalGatherPointGrid.ItemsSource = results.GPResults;
			finalGatherPointGrid.SelectedIndex = 0;

			Coef_P_Label.Content = String.Format("Давление (Kp): {0}", results.Coef_P);
			Coef_T_Label.Content = String.Format("Температура (Kt): {0}", results.Coef_T);
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			FieldDataAnalyzerDBEntities entities = new FieldDataAnalyzerDBEntities();
			graph.Clear();

			List<WellData> wellMeas = db.GetWellsMeasurementsByDate(entities.wells_measurements.Min(x => x.measure_date));
			foreach (var well in graph.wells)
			{
				var m = wellMeas.Where(x => x.Name == well.Name);
				if (m.Count() == 0) continue;
				well.Data = m.First().Data;
				well.Shtutzer = wellMeas.First(x => x.Name == well.Name).Shtutzer;
				evaluator.CalcShtutzer(well);
			}

			optimizationGrid.ItemsSource = graph.wells.Where(m => m.Data.gas_output != 0).Select(y =>
				new
				{
					y.Name,
					y.Shtutzer.d_sht_current,
					KGF_curr = y.Gl / y.Gg,
					y.Shtutzer.M,
					y.Shtutzer.ro,
					y.Shtutzer.ro_g,
 					y.Shtutzer.ro_l,
					y.Shtutzer.d1,
					y.Shtutzer.d2,
					y.Shtutzer.k,
					y.Shtutzer.z,
					y.Shtutzer.C_l,
					y.Shtutzer.D
				});

			KGFbefore.Content = String.Format("КГФ месторождения до оптимизации {0}",
				graph.wells.Where(m => m.Data.gas_output != 0).Sum(x => x.Gl)/
				graph.wells.Where(m => m.Data.gas_output != 0).Sum(x => x.Gg));
		}

		//double F(double[] x)
		//{
		//	maxKGFWell.Shtutzer.d_sht_current = (float)x[0];
		//	min1KGFWell.Shtutzer.d_sht_current = (float)x[1];
		//	min2KGFWell.Shtutzer.d_sht_current = (float)x[2];
		//	evaluator.CalcGraph(optimizationDate);

		//	return -(graph.wells.Sum(m => m.Gl) / graph.wells.Sum(m => m.Gg) -
		//		   (alpha1 * Math.Abs(graph.endNode.P - Psb) + alpha2 * Math.Abs(graph.endNode.G - Gsb)));
		//	//return (x[0] + 10)*(x[0] + 10) + x[1]*x[1] + x[2]*x[2];
		//}

		private void startOptimizationButton_Click(object sender, RoutedEventArgs e)
		{
			FieldDataAnalyzerDBEntities entities = new FieldDataAnalyzerDBEntities();

			//OptimizerContext optimizer = new OptimizerContext(new Gradient(delegate(double[] x)
			//{
			//	return (x[0] + 10) * (x[0] + 10) + x[1] * x[1] + x[2] * x[2] + x[3] * x[3];
			//}, 4));
			//double[] res = optimizer.Run(0.001, new[] {5.0, -7.0, 9.6, -1});

			ShtutzerOptimizer optimizer = new ShtutzerOptimizer(graph);
			double[] res = optimizer.Run(Convert.ToDateTime("1.10.2010"));

			//Optimizer optimizer = new Optimizer(graph);
			//if (!entities.wells_measurements.Any(x => x.measure_date == optimizeDatePicker.DisplayDate))
			//	optimizeDatePicker.DisplayDate = entities.wells_measurements.Max(x => x.measure_date);
			//var date = entities.wells_measurements.Join(entities.final_gather_point_measurements, x => x.measure_date, y => y.measure_date,
			//	(x, y) => new { Date1 = x.measure_date, Date2 = y.measure_date }).First(m => (m.Date1 != null) && (m.Date2 != null));


			//double[] res = optimizer.Calc(date.Date1);

			//optimizer.maxKGFWell.Shtutzer.d_sht_current = (float)res[0];
			//optimizer.min1KGFWell.Shtutzer.d_sht_current = (float)res[1];
			//optimizer.min2KGFWell.Shtutzer.d_sht_current = (float)res[2];

			//var lol =
			//	graph.wells.Where(
			//		x =>
			//			x.Data.well_id == optimizer.maxKGFWell.Data.well_id || x.Data.well_id == optimizer.min1KGFWell.Data.well_id ||
			//			x.Data.well_id == optimizer.min2KGFWell.Data.well_id).ToList();
			//optValuesDataGrid.ItemsSource = lol.Select(x => new
			//{
			//	x.Name,
			//	d_sht_old = x.d_sht,
			//	d_sht_new = x.Shtutzer.d_sht_current
			//});

			//double fieldKGF = graph.wells.Sum(m => m.Gl) / graph.wells.Sum(m => m.Gg);

			//Random rnd = new Random();
			//double[] d_shts = { 0.06d, 0.08d, 0.10d, 0.12d, 0.14d };
			//var db = new FieldDataAnalyzerDBEntities();

			//foreach (var well in db.wells)
			//	db.shtutzers.Add(new shtutzer()
			//	{
			//		well_id = well.well_id,
			//		d_sht_current = (float)d_shts[rnd.Next(4)],
			//		d1 = 0.2265f,
			//		d2 = 0.2265f,
			//		C_l = 1900.0f,
			//		D = 4.2e-6f,
			//		k = 1.285f,
			//		M = 21.64e-3f,
			//		ro = 45.34f,
			//		ro_g = 0.698f,
			//		ro_l = 660f,
			//		z = 0.9f
			//	});
			//db.SaveChanges();
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			Statistics statistics = new Statistics();
			statistics.ShowDialog();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{

		}

		private void finalGatherPointGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();
			int parse = finalGatherPointGrid.SelectedIndex;
			GPLearningResult res = finalGatherPointGrid.SelectedValue as GPLearningResult;
			if (res == null) return;
			DateTime selectedDate = Convert.ToDateTime(res.Date);
			//if (selectedDate == DateTime.Parse("01.01.0001")) return;
			wellMeasDateLabel.Content = String.Format("Дaта {0}/{1}/{2}", selectedDate.Date.Day, selectedDate.Date.Month, selectedDate.Date.Year);

			wellsGrid.ItemsSource = res.WellMeasurements.Where(x => x.inlet_P != 0).Select(x => new
			{
				//Date = String.Format("{0}.{1}.{2}", x.measure_date.Day, x.measure_date.Month, x.measure_date.Year),
				Name = x.well.name,
				x.gas_output,
				x.cond_output,
				x.inlet_T,
				x.wellhead_T,
				x.inlet_P,
				x.wellhead_P
			}).ToList();

			gatherPointsGrid.ItemsSource = res.GPMeasurements.Select(x => new
			{
				x.Name,
				x.NextNodeName,
				x.Pf,
				x.Tf,
				x.Pcoef,
				x.Tcoef,
				x.G
			}).Where(y => y.G != 0).ToList();
			//finalGatherPointGrid.ItemsSource = results.GPResults;

		}

		private void SaveCoefsButton_Click(object sender, RoutedEventArgs e)
		{
			FieldDataAnalyzerDBEntities db = new FieldDataAnalyzerDBEntities();
			coef_evaluations ce = db.coef_evaluations.Add(new coef_evaluations()
			{
				date_from = results.GPResults.Min(x => x.Date),
				date_to = results.GPResults.Max(x => x.Date),
				Kt = (float)results.Coef_T,
				Kp = (float)results.Coef_P
			});
			foreach (var res in results.GPResults)
			{
				final_gather_point_measurements fgpm = db.final_gather_point_measurements.Add(new final_gather_point_measurements()
				{
					measure_date = res.Date,
					Pexper = (float)res.Pexpr,
					Texper = (float)res.Texpr,
					coef_P = (float)res.Coef_P,
					coef_T = (float)res.Coef_T,
					Pcoef = (float)res.Pcoef,
					Tcoef = (float)res.Tcoef,
					Pf = (float)res.Pf,
					Tf = (float)res.Tf,
					coef_evaluations_id = ce.coef_evaluations_id
				});
				foreach (var gp in res.GPMeasurements)
				{
					db.gather_points_measurements.Add(new gather_points_measurements()
					{
						gather_point_id = gp.Id,
						measure_date = res.Date,
						Pcoef = (float) gp.Pcoef,
						Tcoef = (float) gp.Tcoef,
						Pf = (float)gp.Pf,
						Tf = (float)gp.Tf,
						final_gather_points_measurements_id = fgpm.final_gather_point_measurement_id
					});
				}
			}
			//db.SaveChanges();
		}
	}
}
