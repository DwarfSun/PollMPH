using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MPH_Poll
{
  class Program
  {
    private static List<CoinStats> coins = new List<CoinStats>();
    private static void FetchData()
    {
      coins = QueryMph.GetCoinStats().Result;
    }

    public static void Main(string[] args)
    {
      FetchData();
      coins = coins.OrderByDescending(o => o.normalizedProfitNvidia).ToList();

      foreach (CoinStats coin in coins)
      {
        Console.WriteLine("{0} {1} {2}", coin.coinName, coin.host, coin.port);
      }
      Console.WriteLine();
    }
  }
}
