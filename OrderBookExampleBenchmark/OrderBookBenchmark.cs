using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OrderBookExample;

namespace OrderBookExampleBenchmark
{
    [MemoryDiagnoser]
    public class OrderBookBenchmark
    {
        private OrderBook testOrderBook;

        public OrderBookBenchmark()
        {
            int pricePrecision = 1;
            int sizePrecision = 1;
            List<Tuple<decimal, decimal>> ask = new List<Tuple<decimal, decimal>>();
            List<Tuple<decimal, decimal>> bid = new List<Tuple<decimal, decimal>>();
            for (int i = 0; i < 5_00; i++)
            {
                ask.Add(Tuple.Create((decimal)i, (decimal)i));
                bid.Add(Tuple.Create((decimal)i, (decimal)i));
            }
            testOrderBook = new OrderBook(bid, ask, pricePrecision, sizePrecision);
        }

        [Benchmark]
        public void TestUpdateAsk() => testOrderBook.Update(Side.Ask, 30, 20);

        [Benchmark]
        public void GetBidAsk() => testOrderBook.GetBidAsk();

        [Benchmark]
        public void GetTopAsksCount() => testOrderBook.GetTop(Side.Ask, 2, true);

        [Benchmark]
        public void GetTopBidsByPrice() => testOrderBook.GetTop(Side.Bid, 3m, true);
    }
}
