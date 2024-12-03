using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NeatTrader.TickDataProcessor.Data;

namespace NeatTrader.Simulator
{
    internal static class FlowEstimator
    {
        public static double EstimateBidQuantityHit(TickSlice previousSlice, TickSlice slice)
        {
            var flow = 0d;
            
            AddToFlow(previousSlice.B1, previousSlice.B1V);
            AddToFlow(previousSlice.B2, previousSlice.B2V);
            AddToFlow(previousSlice.B3, previousSlice.B3V);
            AddToFlow(previousSlice.B4, previousSlice.B4V);
            AddToFlow(previousSlice.B5, previousSlice.B5V);
            
            void AddToFlow(double price, double quantity)
            {
                if (price > slice.B1)
                {
                    flow += quantity;
                }
                else if (Math.Abs(price - slice.B1) < 0.000001)
                {
                    //if (slice.B1V < quantity) flow += quantity - slice.B1V;
                }
            }

            return flow;
        }

        public static double EstimateAskQuantityHit(TickSlice previousSlice, TickSlice slice)
        {
            var flow = 0d;

            AddToFlow(previousSlice.A1, previousSlice.A1V);
            AddToFlow(previousSlice.A2, previousSlice.A2V);
            AddToFlow(previousSlice.A3, previousSlice.A3V);
            AddToFlow(previousSlice.A4, previousSlice.A4V);
            AddToFlow(previousSlice.A5, previousSlice.A5V);

            void AddToFlow(double price, double quantity)
            {
                if (price < slice.A1)
                {
                    flow += quantity;
                }
                else if (Math.Abs(price - slice.A1) < 0.000001)
                {
                    //if (slice.A1V < quantity) flow += quantity - slice.A1V;
                }
            }

            return flow;
        }
    }
}
