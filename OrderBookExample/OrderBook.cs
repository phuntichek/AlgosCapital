using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderBookExample
{
    public enum Side
    {
        Bid,
        Ask
    }

    public interface IOrderBook
    {
        // Обновить один уровень ордербука (объем заявок по заданной цене)
        public void Update(Side side, decimal price, decimal size, bool ignoreError = false);

        // заполнить одну сторону ордербука новыми данными
        public void Fill(Side side, IEnumerable<Tuple<decimal, decimal>> data);

        // очистить ордербук. возвращает количество удаленных уровней для Bid и Ask
        public Tuple<int, int> Clear();

        // получить верхний уровень ордербука -- лучшую цену и объем для бидов и асков
        public BidAsk GetBidAsk();

        // получить count верхних уровней одной стороны ордербука.
        // cumulative -- считать кумулятивные объемы
        public Level[] GetTop(Side side, int count, bool cumulative = false);

        // получить несколько верхних уровней ордербука, вплоть до цены price включительно
        // cumulative -- считать кумулятивные объемы
        public Level[] GetTop(Side side, decimal price, bool cumulative = false);

        // получить цену с уровня, где кумулятивный объем превышает cumul
        public decimal? GetPriceWhenCumulGreater(Side side, decimal cumul);

        // возвращает true, если ордербук пуст
        public bool IsEmpty();

    }

    public class OrderBookBase
    {
        protected decimal priceMultiplier;
        protected decimal sizeMultiplier;

        protected OrderBookBase() {}
        
        // pricePrecision -- сколько цифр после запятой в ценах
        // sizePrecision -- сколько цифр после запятой в объемах
        public OrderBookBase(uint pricePrecision, uint sizePrecision)
        {
            priceMultiplier = (decimal)Math.Pow(10, -pricePrecision);
            sizeMultiplier = (decimal)Math.Pow(10, -sizePrecision);
        }
    }

    public class OrderBook : OrderBookBase, IOrderBook
    {
        private int pricePrecision;
        private int sizePrecision;
        private List<Level> bid;
        private List<Level> ask;

        public OrderBook(List<Level> _bid, List<Level> _ask, int _pricePrecision, int _sizePrecision)
        {
            this.bid = _bid;
            this.ask = _ask;
            //ask = new List<Level>();
            this.pricePrecision = _pricePrecision;
            this.sizePrecision = _sizePrecision;
        }

        public void Update(Side side, decimal price, decimal size, bool ignoreError = false)
        {
            if (side == Side.Ask)
            {
                Level item = ask.FirstOrDefault(x => Math.Round(x.Price, pricePrecision) == Math.Round(price, pricePrecision));
                if (item.Equals(null))
                    if (!ignoreError)
                        ask.Add(new Level { Price = price, Size = size });
                    else
                        item.Size = size;
            }
            else
            {
                Level item = bid.FirstOrDefault(x => Math.Round(x.Price, pricePrecision) == Math.Round(price, pricePrecision));
                if (item.Equals(null))
                    if (!ignoreError)
                        bid.Add(new Level { Price = price, Size = size });
                    else
                        item.Size = size;
            }
        }

        public void Fill(Side side, IEnumerable<Tuple<decimal, decimal>> data)
        {
            if (side == Side.Ask)
                foreach (var item in data)
                    ask.Add(new Level { Price = item.Item1, Size = item.Item2 });
            else
                foreach (var item in data)
                    bid.Add(new Level { Price = item.Item1, Size = item.Item2 });
        }

        public Tuple<int, int> Clear()
        {
            var bidCount = bid.Count;
            var askCount = ask.Count;
            bid.Clear();
            ask.Clear();
            return Tuple.Create(bidCount, askCount);
        }

        public BidAsk GetBidAsk()
        {
            var maxAsk = ask.MinBy(x => x.Price);
            var maxBid = bid.MaxBy(x => x.Price);
            return new BidAsk
            {
                AskPrice = Math.Round(maxAsk.Price, pricePrecision),
                BidPrice = Math.Round(maxBid.Price, pricePrecision),

                AskVolume = Math.Round(maxAsk.Size, sizePrecision),
                BidVolume = Math.Round(maxBid.Size, sizePrecision)
            };
        }

        public Level[] GetTop(Side side, int count, bool cumulative = false)
        {
            Level[] topLevelsByCount = new Level[count];
            if (side == Side.Ask)
            {
                var asks = ask.OrderByDescending(x => x.Price);
                for (int i = 0; i < count; i++)
                    topLevelsByCount[i] = new Level(Math.Round(asks.ElementAt(1).Price, pricePrecision),
                        Math.Round(asks.ElementAt(i).Size, sizePrecision));
                return topLevelsByCount;
            }
            else
            {
                var bids = bid.OrderByDescending(x => x.Price);
                for (int i = 0; i < count; i++)
                        topLevelsByCount[i] = new Level(Math.Round(bids.ElementAt(1).Price, pricePrecision),
                            Math.Round(bids.ElementAt(i).Size, sizePrecision));
                return topLevelsByCount;
            }
        }

        public Level[] GetTop(Side side, decimal price, bool cumulative = false)
        {
            if (side == Side.Ask)
            {
                var asks = ask.Where(x => x.Price <= price);
                Level[] topLevelsByPrice = new Level[asks.Count()];
                for (int i = 0; i < asks.Count(); i++)
                    topLevelsByPrice[i] = new Level(Math.Round(asks.ElementAt(1).Price, pricePrecision),
                        Math.Round(asks.ElementAt(i).Size, sizePrecision));
                return topLevelsByPrice;
            }
            else
            {
                var bids = bid.Where(x => x.Price <= price);
                Level[] topLevelsByPrice = new Level[bids.Count()];
                for (int i = 0; i < bids.Count(); i++)
                    topLevelsByPrice[i] = new Level(Math.Round(bids.ElementAt(1).Price, pricePrecision),
                        Math.Round(bids.ElementAt(i).Size, sizePrecision));
                return topLevelsByPrice;
            }
        }

        public decimal? GetPriceWhenCumulGreater(Side side, decimal cumul)
        {
            var bids = bid.FirstOrDefault(x => x.CumulSize > cumul);
            if (bids.Size > 0)
                return Math.Round(bids.Price, pricePrecision);
            return null;
        }

        public bool IsEmpty()
        {
            if (ask.Count == 0 && bid.Count == 0)
                return true;
            else
                return false;
        }
    }
}
