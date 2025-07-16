using LottologiaPatternAnalyzer.Models;
using LottologiaPatternAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LottologiaPatternAnalyzer.Engine
{
    public class PatternAnalyzerFromScratch
    {
        PatternAnalyzerConf _conf;
        public PatternAnalyzerFromScratch() { }
        public PatternAnalyzerFromScratch(PatternAnalyzerConf conf)
        {
            _conf = conf;
        }

        public async Task StartAnalisys(string seriesToAnalize,int minimumTarget, DateTime analisysDay)
        {
            Console.WriteLine("Analyzing Lottologia...");

            string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
            if (basePath == null)
                throw new Exception("Impossibile determinare la path relativa al progetto.");
            string dbpathStorico = $"Data Source={Path.Combine(basePath, "Database", "10el5.db")}";
            string dbpathAnalisi = $"Data Source={Path.Combine(basePath, "Database", "LottologiaPatternAnalizer.db")}";
            
            SqliteService dbServiceStorico = new SqliteService(dbpathStorico);
            SqliteService dbServiceAnalisi = new SqliteService(dbpathAnalisi);
           
            DataTable dt = dbServiceStorico.GetDrawDay(analisysDay);

            //var series = dbServiceAnalisi.GetSerieByCode("5_1_%");  //okkio, se metto 5_1% prendera anche i 5_10_xx _ è un carattere jolly in sqlite, viene fatto l'escaping interno
            var series = dbServiceAnalisi.GetSerieByCode(seriesToAnalize);  //okkio, se metto 5_1% prendera anche i 5_10_xx _ è un carattere jolly in sqlite, viene fatto l'escaping interno
            if (series.Count == 0)
                throw new Exception("Serie non trovata");

            // Calcolo del ritardo per tutta la serie di estrazioni
            List<BuildStatisticsService.AnalisysItem> risultati = BuildStatisticsService.CalcolaRitardi(dt, series, minimumTarget);

            var entries = risultati.Select(item =>
            {
                var data = item.Data.ToString("yyyy-MM-dd");
                var orario = item.Orario;

                return new TableEntry
                {
                    DrawDate = data,
                    DrawNumber = item.Progressivo.ToString(),
                    DrawTime = orario, 
                    DrawFullTimeStamp = $"{data}T{orario}:00Z",
                    Code = item.Code,
                    Objective = item.PunteggioMinimo.ToString(),
                    RrStandard = item.RitardoStandard.ToString(),
                    RrExtra = item.RitardoExtra.ToString(),
                    RrCombined = item.RitardoCombinato.ToString()
                };
            }).ToList();

            dbServiceAnalisi.InsertData(entries, true);
        }
    }
}
