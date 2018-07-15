using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace spelunker
{
  public class Miner
  {
    public string algorithm;
    public string filename;
    public string arguments;
  }
  public class Config
  {
    public string user;
    public string rig;
    public List<string> exclusions = new List<string>();
    public List<Miner> miners = new List<Miner>();

    public Config()
    {
      try
      {
        string jsonFile = File.ReadAllText("./config.json");
        dynamic config = JsonConvert.DeserializeObject(jsonFile);
        user = config.user;
        rig = config.rig;
        foreach (string algo in config.exclude)
        {
          exclusions.Add(algo);
        }
        foreach (dynamic m in config.miners)
        {
          Miner miner = new Miner
          {
            algorithm = m.algorithm,
            filename = m.filename,
            arguments = m.arguments
          };
          miners.Add(miner);
        }
      }
      catch
      {
        System.Console.Error.WriteLine("ERROR: Unable to read config.json");
      }
    }
  }

  class Program
  {
    private static Config config = new Config();
    private static CoinStats topCoin = new CoinStats();
    private static List<CoinStats> coins = new List<CoinStats>();
    private static void FetchData()
    {
      coins = QueryMph.GetCoinStats().Result;
      foreach (string ex in config.exclusions)
      {
        coins.RemoveAll(o => o.algorithm == ex);
      }
      coins = coins.OrderByDescending(o => o.normalizedProfitNvidia).ToList();
    }

    static void Main(string[] args)
    {
      Process process = null;
      int loops = 0;

      while (true)
      {
        loops++;
        bool fetched = false;
        while (fetched==false)
        {
          try
          {
            FetchData();
            fetched = true;
          }
          catch 
          { 
            System.Console.Error.WriteLine("Polling multipoolhub failed, trying again...");
            System.Threading.Thread.Sleep(5000);
          }
        }

        if (topCoin.coinName == coins[0].coinName)
        {
          if (loops % 5 == 0)
            System.Console.WriteLine("{0} is still the most profitable coin.", topCoin.coinName);
        }
        else
        {
          try
          {
            if (process != null)
            {
              System.Console.WriteLine("{0} is not the most profitable coin, with a normalised profit rating of {1}", topCoin.coinName, topCoin.normalizedProfitNvidia);
              process.Kill();
            }
          }
          catch
          {
            System.Console.Error.WriteLine("Failed to terminate process");
          }
          try
          {
            CoinStats coin = coins[0];
            Miner miner = config.miners.Find(x => x.algorithm == coin.algorithm);
            miner.arguments = string.Format(miner.arguments, coin.host, coin.port, config.user, config.rig);

            process = new Process
            {
              StartInfo = new ProcessStartInfo
              {
                FileName = miner.filename,
                Arguments = miner.arguments,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
              }
            };
            process.Start();
          }
          catch
          {
            System.Console.Error.WriteLine("Failed to start process");
          }
        }
        try
        {
          topCoin = coins[0];
          System.Console.WriteLine("Mining {0}, with a normalised profit rating of {1}", topCoin.coinName, topCoin.normalizedProfitNvidia);
        }
        catch { }
        System.Threading.Thread.Sleep(60000);
      }
    }
  }
}
