using LottologiaPatternAnalyzer.Models;
using LottologiaPatternAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LottologiaPatternAnalyzer.Engine
{
    public class PatternAnalyzer
    {
        PatternAnalyzerConf _conf;
        public PatternAnalyzer() { }
        public PatternAnalyzer(PatternAnalyzerConf conf)
        {
            _conf = conf;
        }

        public async Task StartAnalisys()
        {
            Console.WriteLine("Analyzing Lottologia...");

            string dbpath = @"C:\PROGETTI\Personali\LottologiaPatternAnalyzer\Database\LottologiaPatternAnalizer.db";

            using (PlaywrightScraperService scraper = new PlaywrightScraperService())
            {
                List<PatternToAnalize> patternList = await scraper.GetPatternList();
                string path = patternList.Where(x => x.Description == "Cinquine Radicali-Correlative").Select(x => x.Punti_3).FirstOrDefault();
                path = "https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?tab=delay&&ts=2&mode=draws&" + path[3..];  //uguale a substring(1)
                
                //await scraper.GetPattern();
                var countDown = await scraper.GetCountDown();
                var data = await scraper.GetData();
                //var scraper = new TextFileScraperService();
                //var data = await scraper.GetData();

                SqliteService dbService = new SqliteService(dbpath);
                dbService.InsertData(data, true);
            }
        }
    }
}
