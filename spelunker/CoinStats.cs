using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace spelunker
{
  public class CoinStats
  {
    public string coinName;
    public string host;
    public string hostList;
    public long port;
    public string directMiningHost;
    public string directMiningHostList;
    public long directMiningPort;
    public string algorithm;
    public double normalizedProfit;
    public double normalizedProfitAmd;
    public double normalizedProfitNvidia;
    public double profit;
    public string poolHash;
    public string netHash;
    public double difficulty;
    public double reward;
    public long lastBlock;
    public long timeSinceLastBlock;
    public string timeSinceLastBlockInWords;
    public double bittrexBuyPrice;
    public double cryptsyBuyPrice;
    public double yobitBuyPrice;
    public double poloniexBuyPrice;
    public double highestBuyPrice;

    public CoinStats()
    {
    }
  }
}
