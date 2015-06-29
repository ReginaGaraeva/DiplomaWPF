using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using FIeldDataAnalyzer;

namespace FieldDataAnalyzer
{
	public class Graph
	{
		public List<Node> nodes = new List<Node>();
		public List<Pipe> pipes = new List<Pipe>();
		public List<WellData> wells = new List<WellData>();
		public Node endNode;
		private FieldDataAnalyzerDBEntities dbEntities = new FieldDataAnalyzerDBEntities();



		public Graph()
		{
			GetNodes();
			GetPipes();
		}

		public void Clear()
		{
			foreach (var node in nodes)
			{
				node.P = 0;
				node.T = 0;
				node.G = 0;
			}
		}

		private void GetNodes()
		{
			var sborNode = dbEntities.gather_points.Where(x => x.next_gather_point_id == null).ToList();
			if (sborNode.Count() != 1)
				throw new Exception("Конечный пункт сбора либо не задан, либо не уникален.");
			nodes.Add(new Node {Id = sborNode.First().gather_point_id, Name = sborNode.First().name, NextNode = null, P = 0, G = 0, T = 0});
			endNode = nodes[0];
			FindBindNodes(endNode);
			AddWells();
		}

		private void FindBindNodes(Node endNode)
		{
			var bindNodes =
				dbEntities.gather_points.Where(x => x.next_gather_point_id == endNode.Id).ToList().Select(x => new Node {Id = x.gather_point_id, Name = x.name, NextNode = endNode}).ToList();
			if (bindNodes.Count > 0)
			{
				nodes.AddRange(bindNodes);
				foreach (var node in bindNodes)
				{
					FindBindNodes(node);
				}
			}
		}

		private void AddWells()
		{
			foreach (var _well in dbEntities.wells.ToList())
			{
				WellData well = new WellData
				{
					Data = new wells_measurements() { well_id = _well.well_id },
					Name = _well.name,
					a = new double[] { (double)_well.a1, (double)_well.a2, (double)_well.a3 },
					b = new double[] { (double)_well.b1, (double)_well.b2, (double)_well.b3 },
					Shtutzer = dbEntities.shtutzers.First(x => x.well_id == _well.well_id)
				};
				wells.Add(well);
				nodes.First(x => x.Id == _well.gather_point_id).wells.Add(@well);
				
			}
		}

		private void GetPipes()
		{
			foreach (var pipe in dbEntities.pipes)
			{
				pipes.Add(new Pipe
				{
					Data = pipe,
					StartNode = nodes.First(x => x.Id == pipe.start_id),
					EndNode = nodes.First(x => x.Id == pipe.end_id)
				});
			}
		}

		public void OptimiseVertexPositions(int width, int height)
		{
			//int M = 200;
			////Случайное расположение вершин графа на плоскости
			//Random rnd = new Random();
			//foreach (var node in nodes)
			//{
			//	node.point.X = rnd.Next(10, width - 10);
			//	node.point.Y = rnd.Next(10, height - 10);
			//}
			////итеративный сдвиг вершин
			//double c_spring = 2, c_rep = 1, l = 100;
			//float delta = 0.85f;
			//PointF f_rep = new PointF(0, 0), f_spring = new PointF(0, 0);
			//for (int t = 0; t < M; t++)
			//{
			//	foreach (var node1 in nodes)
			//	{
			//		foreach (var node2 in nodes)
			//		{
			//			if (node1 == node2) continue;
			//			PointF vect = new PointF(0, 0);
			//			double r = EdgeLength(node1, node2);
			//			if (pipes.Count(x => ((x.StartNode == node1) && (x.EndNode == node2))
			//								 || ((x.StartNode == node2) && (x.EndNode == node1))) == 0)
			//			{
			//				vect = NormalizeVector(node1, node2);
			//				double A = c_rep/r/r;
			//				f_rep.X += (float) (A*vect.X);
			//				f_rep.Y += (float) (A*vect.Y);
			//			}
			//			else
			//			{
			//				vect = NormalizeVector(node2, node1);
			//				double B = c_spring*Math.Log(r/l);
			//				f_spring.X += (float) (B*vect.X);
			//				f_spring.Y += (float) (B*vect.Y);
			//			}

			//		}
			//		node1.point.X += delta*(f_rep.X + f_spring.X);
			//		node1.point.Y += delta*(f_rep.Y + f_spring.Y);
			//		f_rep = new PointF(0, 0);
			//		f_spring = new PointF(0, 0);

			//	}
			//}
		}

		////длина вектора
		//public double EdgeLength(Node node1, Node node2)
		//{

		//	return Math.Sqrt(Math.Pow(node1.point.X - node2.point.X, 2) + Math.Pow(node1.point.Y - node2.point.Y, 2));
		//}

		////единичный вектор от node1 к node2
		//public PointF NormalizeVector(Node node1, Node node2)
		//{
		//	double r = EdgeLength(node1, node2);
		//	return new PointF((float) ((node1.point.X - node2.point.X)/r), (float) ((node1.point.Y - node2.point.Y)/r));
		//}
	}

	public class Node
	{
		public int Id;
		public string Name;
		public Node NextNode;
		public List<WellData> wells;
		public double P = 0, T = 0, G = 0;
		public System.Windows.Point point;
		public double[] P_ = new double[5], G_ = new double[5], T_ = new double[5], a, b;

		public Node()
		{
			wells = new List<WellData>();
		}
	}

	public class Pipe
	{
		//public string Name;
		public pipe Data;
		public Node StartNode;
		public Node EndNode;

	}
}