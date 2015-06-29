using System;
using System.Data.Common.CommandTrees;
using System.Data.Objects;
using System.Linq;
using FIeldDataAnalyzer;
using Npgsql;
using FieldDataAnalyzer;
using System.Collections.Generic;

public class DBService
{
	NpgsqlConnection conn;
	private FieldDataAnalyzerDBEntities dbEntities = new FieldDataAnalyzerDBEntities();

	public DBService()
	{
		//NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=FieldDataAnalyzerDB;");
		//conn.Open();
	}

	public void Disconnect()
	{
		//conn.Close();
	}

	public void LoadGPFromSchemaToDB(List<string[]> schema)
	{
		foreach (var s in schema)
		{
			dbEntities.gather_points.Add(new gather_points()
			{
				name = s[0],
			});
		}
		dbEntities.SaveChanges();
	}

	public void LoadSchemaToDB(List<string[]> schema)
	{
		foreach (var s in schema)
		{
			string tmp = s[0];
			var fromNode = dbEntities.gather_points.First(x => x.name == tmp);
			tmp = s[1];
			if (tmp == "") continue;
			var toId = dbEntities.gather_points.Where(x => x.name == tmp).Select(y => y.gather_point_id).First();
			fromNode.next_gather_point_id = toId;
		}
		dbEntities.SaveChanges();
	}

	public void LoadWellsToDB(List<string[]> wells)
	{
		foreach (var well in wells)
		{
			string tmp = well[1];
			var gpId = dbEntities.gather_points.Where(x => x.name == tmp).Select(y => y.gather_point_id).First();
			dbEntities.wells.Add(new well
			{
				gather_point_id = gpId,
				name = well[0]
			});
		}
		dbEntities.SaveChanges();
	}

	public void LoadPipesToDB(List<PipeData> pipes)
	{
		foreach (var pipe in pipes)
		{
			var startId = dbEntities.gather_points.Where(x => x.name == pipe.StartNode).Select(y => y.gather_point_id).First();
			var endId = dbEntities.gather_points.Where(x => x.name == pipe.EndNode).Select(y => y.gather_point_id).First();
			dbEntities.pipes.Add(new pipe
			{
				name = pipe.Num.ToString(),
				end_id = endId,
				start_id = startId,
				inner_d = (float)pipe.InnerD,
				length = (float)pipe.Length,
				outer_d = (float)pipe.OuterD,
				roughness = (float)pipe.Roughness,
				temper = (float)pipe.OuterT,
				thickness = (float)pipe.Width
			});
		}
		dbEntities.SaveChanges();
	}

	public void LoadWellsMeasurementsToDB(List<WellData> wells)
	{
		int b = 20000, e = 24022;
		for (int i = b; i < e; i++)
		{
			string tmp = wells[i].Name;
			var id = dbEntities.wells.Where(x => x.name == tmp).Select(y => y.well_id).First();
			dbEntities.wells_measurements.Add(wells[i].Data);
		}
		dbEntities.SaveChanges();
	}

	public void LoadGPMeasurementsToDB(List<SborData> sborData)
	{
		foreach (var s in sborData)
		{
			dbEntities.final_gather_point_measurements.Add(new final_gather_point_measurements
			{
				measure_date = s.Date.Date,
				Pexper = (float)s.P,
				Texper = (float)s.T
			});
		}
		dbEntities.SaveChanges();
	}

	public List<WellData> GetWellsMeasurementsByDate(DateTime date)
	{
		return dbEntities.wells_measurements.Where(x => x.measure_date == date).Select(
			y => new WellData()
			{
				Name = dbEntities.wells.FirstOrDefault(m => m.well_id == y.well_id).name,
				Data = y, 
				Shtutzer = dbEntities.shtutzers.FirstOrDefault(z => z.well_id == y.well_id)
			}).ToList();
	}


}
