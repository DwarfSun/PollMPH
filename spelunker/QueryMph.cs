using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace spelunker
{
  public class QueryMph
  {
    public static async Task<List<CoinStats>> GetCoinStats()
    {
      string queryString = "http://miningpoolhub.com/index.php?page=api&action=getminingandprofitsstatistics";
      List<CoinStats> coins = new List<CoinStats>();
      dynamic results = await DataService.getDataFromService(queryString).ConfigureAwait(false);

      if (results["return"] != null)
      {
        var returnData = results["return"];
        foreach (var c in returnData)
        {
          CoinStats coin = new CoinStats();
          coin.coinName = c["coin_name"];
          coin.host = c["host"];
          coin.hostList = c["host_list"];
          coin.port = (long)c["port"];
          coin.directMiningHost = c["direct_mining_host"];
          coin.directMiningHostList = c["direct_mining_host_list"];
          coin.directMiningPort = (long)c["direct_mining_algo_port"];
          coin.algorithm = c["algo"];
          coin.normalizedProfit = (double)c["normalized_profit"];
          coin.normalizedProfitAmd = (double)c["normalized_profit_amd"];
          coin.normalizedProfitNvidia = (double)c["normalized_profit_nvidia"];
          coin.profit = (double)c["profit"];
          coin.poolHash = c["pool_hash"];
          coin.netHash = c["net_hash"];
          coin.difficulty = (double)c["difficulty"];
          coin.reward = (double)c["reward"];
          coin.lastBlock = (long)c["last_block"];
          coin.timeSinceLastBlock = (long)c["time_since_last_block"];
          coin.timeSinceLastBlockInWords = c["time_since_last_block_in_words"];
          coin.bittrexBuyPrice = c["bittrex_buy_price"];
          coin.cryptsyBuyPrice = c["cryptsy_buy_price"];
          coin.yobitBuyPrice = c["yobit_buy_price"];
          coin.poloniexBuyPrice = c["poloniex_buy_price"];
          coin.highestBuyPrice = c["highest_buy_price"];
          coins.Add(coin);
        }
        return coins;
      }
      else
      {
        return null;
      }
    }
    
  }
}
