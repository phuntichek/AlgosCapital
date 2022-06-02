using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrderBookExample;

namespace OrderBookExampleTests
{
    public class OrderBookTests
    {
        private OrderBook testOrderBook;
        [SetUp]
        public void Setup()
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

        [Test]
        public void TestBidAsk()
        {
            var testBidAsk = new BidAsk
            {
                AskPrice = 0,
                BidPrice = 4_000,

                AskVolume = 0,
                BidVolume = 4_000
            };
            Assert.AreEqual(testBidAsk, testOrderBook.GetBidAsk());
        }

        [Test]
        public void TestGetTop2BidsByCount()
        {

            var testItems = new Level[]
            {
                new Level
                {
                    Price = 5_000,
                    Size = 5_000
                },
                new Level
                {
                    Price = 4_999,
                    Size = 4_999
                },
            };
            Assert.AreEqual(testItems, testOrderBook.GetTop(Side.Bid, 3, false));
        }

        [Test]
        public void TestGetTop2Asks_ByPrice()
        {
            var testItems = new Level[]
            {
                new Level
                {
                    Price = 0,
                    Size = 0
                },
                new Level
                {
                    Price = 1,
                    Size = 1
                },
            };
            Assert.AreEqual(testItems, testOrderBook.GetTop(Side.Ask, 3m, false));
        }
    }
}
