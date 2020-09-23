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

#if !DEBUG
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
                Depth = InterlockedEngineStats.Statistics.MaxDepth,
                Nodes = InterlockedEngineStats.Statistics.NodesVisited,
                Evaluation = InterlockedEngineStats.Statistics.Evaluation,
                Duration = stopwatch.Elapsed,
            });

            // copy to file
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            path = Path.Combine(path, $"PerformanceTest.json");
            File.WriteAllText(path, resultJson);
        }
#endif

        [TestMethod]
        public void TestColNumberAvoidDivide()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<int> rows = new List<int>(2000000);
            List<int> cols = new List<int>(2000000);
            Random rng = new Random();

            for (int i = 0; i < 500000; i++)
            {
                int from = rng.Next(0, 8 * 8);
                int rownumber = from / 8;
                int colnumer = from % 8;

                rows.Add(rownumber);
                cols.Add(colnumer);
            }

            Trace.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            for (int i = 0; i < 500000; i++)
            {
                int from = rng.Next(0, 8 * 8);
                int rownumber = from / 8;
                int colnumer = from - rownumber * 8;

                rows.Add(rownumber);
                cols.Add(colnumer);
            }
            Trace.WriteLine(stopwatch.ElapsedMilliseconds);

            Trace.WriteLine(rows.Average() + cols.Average());
        }


        [TestMethod]
        public void TestForVsForeach()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<int> data = new List<int>(2000000);

            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 8 * 8; j++)
                {
                    data.Add(i + j);
                }
            }

            Trace.WriteLine(stopwatch.ElapsedMilliseconds);

            var list = Enumerable.Range(0, 8 * 8).ToList();

            stopwatch.Restart();

            for (int i = 0; i < 10000; i++)
            {
                foreach (var j in list)
                {
                    data.Add(i + j);
                }
            }

            Trace.WriteLine(stopwatch.ElapsedMilliseconds);

            Trace.WriteLine(data.Average());
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
