using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ChEngine
{
    public static class InterlockedEngineStats
    {
        private static int NodesVisited_;
        private static int MaxDepth_;
        private static double Evaluation_;

        public static EngineStatistics Statistics => new EngineStatistics() { NodesVisited = NodesVisited_, MaxDepth = MaxDepth_, Evaluation = Evaluation_ };

        public static void Reset() => NodesVisited_ = MaxDepth_ = 0;

        public static void Increment_NodesVisited()
        {
            Interlocked.Increment(ref NodesVisited_);
        }

        public static void Update_MaxDepth(int depth)
        {
            // check if this is bigger
            while (true)
            {
                int expectedValue = MaxDepth_;

                // Update only if greater!
                if (depth <= expectedValue)
                    return;

                // There could have been an update of another thread
                int actualValue = Interlocked.CompareExchange(ref MaxDepth_, depth, expectedValue);
                if (actualValue == expectedValue)
                    return;

                // not necessary to try again?
                if (depth <= actualValue)
                    return;
            }
        }

        public static void Set_Evaluation(double evaluation) => Interlocked.Exchange(ref Evaluation_, evaluation);
    }

    public class EngineStatistics
    {
        public int NodesVisited { get; set; }
        public int MaxDepth { get; set; }
        public double Evaluation { get; set; }
    }
}
