using ChEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TestCh
{
    [TestClass()]
    public class PerformanceTest
    {
        [TestMethod()]
        public void TestStartPositionPerformance()
        {
            Engine e = new Engine(isWhite: true);

            // let the engine crunch on this
            Stopwatch stopwatch = Stopwatch.StartNew();
            e.ReactToMove(Enumerable.Empty<Move>());
            stopwatch.Stop();

            // Dump
            string resultJson = JsonConvert.SerializeObject(new PerformanceTestModel()
            {
                Date = DateTime.Now,
                Depth = e.Statistics.MaxDepth,
                Nodes = e.Statistics.NodesVisited,
                Evaluation = e.Statistics.Evaluation,
                Duration = stopwatch.Elapsed,
            });

            // copy to file
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            path = Path.Combine(path, "PerformanceTest.json");
            File.WriteAllText(path, resultJson);
        }
    }

    public class PerformanceTestModel
    {
        public DateTime Date { get; set; }
        public int Depth { get; set; }
        public int Nodes { get; set; }
        public double Evaluation { get; set; }
        public TimeSpan Duration { get; set; }

        public int NodesPerSecond => (int)(Nodes / Duration.TotalSeconds);

    }
}
