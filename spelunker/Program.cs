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
    private string configFileName = "spelunker.json";

    private void CreateDefaultConfig(string _usr, string _rig)
    {
      user = _usr;
      rig = _rig;

      exclusions.Add("X11");
      exclusions.Add("Sia");
      exclusions.Add("Scrypt");
      exclusions.Add("Qubit");
      exclusions.Add("Yescrypt");
      exclusions.Add("Keccak");

      miners.Add(new Miner{
        algorithm = "Equihash",
        filename = "/spelunker/zm/zm",
        arguments = @"--noreconnect --server {0} --port {1} --user {2}.{3} --pass x --telemetry=0.0.0.0:2222 --time --color"
      });
      miners.Add(new Miner{
        algorithm = "Equihash-BTG",
        filename = "/spelunker/ewbf/miner",
        arguments = @"--server {0} --port {1} --user {2}.{3} --pass x  --algo 144_5 --pers BgoldPoW --fee 1 --eexit 3 --api 0.0.0.0:2222"
      });
      miners.Add(new Miner{
        algorithm = "Ethash",
        filename = "/spelunker/ethdcrminer/ethdcrminer64",
        arguments = @"-epool {0}:{1} -ewal {2}.{3} -eworker {2}.{3} -esm 2 -epsw x -allcoins 1 -dbg -1 -retrydelay -1 -mport -2222"
      });
      miners.Add(new Miner{
        algorithm = "Cryptonight-Monero",
        filename = "/spelunker/xmr-nvidia/xmr-nvidia",
        arguments = @"-o {0}:{1} -u {2}.{3} -p x -r 1 --donate-level=1 --api-port=2222"
      });
      miners.Add(new Miner{
        algorithm = "Groestl",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a groestl -o stratum+tcp://{0}:{1} -u {2}.{3} -p x"
      });
      miners.Add(new Miner{
        algorithm = "Lyra2RE2",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a lyra2v2 -o stratum+tcp://{0}:{1} -u {2}.{3} -p x"
      });
      miners.Add(new Miner{
        algorithm = "Lyra2z",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a lyra2z -o stratum+tcp://{0}:{1} -u {2}.{3} -p x"
      });
      miners.Add(new Miner{
        algorithm = "Myriad-Groestl",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a myr-gr -o stratum+tcp://{0}:{1} -u {2}.{3} -p x"
      });
      miners.Add(new Miner{
        algorithm = "NeoScrypt",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a neoscrypt -o stratum+tcp://{0}:{1} -u {2}.{3} -p x -i 19"
      });
      miners.Add(new Miner{
        algorithm = "Skein",
        filename = "/spelunker/ccminer/ccminer",
        arguments = @"-r 0 -a skein -o stratum+tcp://{0}:{1} -u {2}.{3} -p x"
      });

      using(StreamWriter file = File.CreateText(configFileName))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, this);
      }
    }

    private void LoadConfig()
    {
      try
      {
        string jsonFile = File.ReadAllText(configFileName);
        dynamic config = JsonConvert.DeserializeObject(jsonFile);
        user = config.user;
        rig = config.rig;
        foreach (string algo in config.exclusions)
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
        System.Console.WriteLine("Loaded configuration from {0}", configFileName);
      }
      catch
      {
        System.Console.Error.WriteLine("ERROR: Unable to read {0}", configFileName);
      }
    }

    public Config()
    {
      LoadConfig();
    }
    public Config(string _usr, string _rig)
    {
      CreateDefaultConfig(_usr, _rig);
    }
  }

  class Program
  {
    private static Config config;// = new Config();
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

    private static void DoWork()
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

    static void Main(string[] args)
    {
      if (args.Length > 1)
      {
        config = new Config(args[0], args[1]);
      }
      else
      {
        config = new Config();
      }
      DoWork();
    }
  }
}
