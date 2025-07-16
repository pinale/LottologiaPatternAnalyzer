using LottologiaPatternAnalyzer.Models;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LottologiaPatternAnalyzer.Services
{
    public class BuildStatisticsService : IDisposable
    {
        string url = "https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?tab=delay&comb=2&group=MDUuZml2ZS9jaW5xdWluZS1yYWRpY2FsaS1jb3JyZWxhdGl2ZQ%%3D%%3D&ts=1&mode=draws&as=TXT";
       
        public BuildStatisticsService() {
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
            CultureInfo.CurrentUICulture = new CultureInfo("it-IT");
        }

        public static List<AnalisysItem> CalcolaRitardi(DataTable storicoEstrazioni, Dictionary<string, string> series, int punteggioMinimo)
        {
            var risultati = new List<AnalisysItem>();

            //trasformare in  SortedDictionary<TKey, TValue> potrebbe dare un vataggio perche itero successivamente sempre nello stesso ordine
            //oppure
            //trasformare in  ConcurrentDictionary<TKey, TValue> che è una versione thread-sefe se decido di parallelizzare il calcolo
            var ritardiSerie = series.ToDictionary(
                serie => serie.Key,
                serie => new RitardoSerie
                {
                    Combinazione = serie.Value.Split('.').Select(int.Parse).ToList(),
                    RitardoStandard = 0,
                    RitardoExtra = 0,
                    RitardoCombinato = 0
                }
            );

            foreach (DataRow estrazione in storicoEstrazioni.Rows)
            {
                var dataEstrazione = Convert.ToDateTime(estrazione.ItemArray[0]);
                var numEstrazione = Convert.ToInt32(estrazione.ItemArray[1]);
                var oraEstrazione = estrazione.ItemArray[2].ToString().Replace('.',':');
                var drawStandard = estrazione.ItemArray.ToList()[3..23].Select(x => Convert.ToInt32(x));
                var drawExtra = estrazione.ItemArray[25].ToString();

                //questa parte potrebbe diventare parallela
                foreach (var serie in series)
                {
                    string code = serie.Key;
                    List<int> combinazione = ritardiSerie[code].Combinazione;

                    int puntiStandard = combinazione.Intersect(drawStandard).Count();
                    int puntiExtra = combinazione.Intersect(drawExtra.Split('.').Select(int.Parse)).Count();
                    
                    ritardiSerie[code].RitardoStandard = (puntiStandard >= punteggioMinimo) ? 0 : ritardiSerie[code].RitardoStandard + 1;
                    ritardiSerie[code].RitardoExtra = (puntiExtra >= punteggioMinimo) ? 0 : ritardiSerie[code].RitardoExtra + 1;
                    ritardiSerie[code].RitardoCombinato = Math.Min(ritardiSerie[code].RitardoStandard, ritardiSerie[code].RitardoExtra);

                    risultati.Add(new AnalisysItem
                    {
                        Data = dataEstrazione,
                        Orario = oraEstrazione,
                        Progressivo = numEstrazione,
                        Code = code,
                        PunteggioMinimo = punteggioMinimo,
                        RitardoStandard = ritardiSerie[code].RitardoStandard,
                        RitardoExtra = ritardiSerie[code].RitardoExtra,
                        RitardoCombinato = ritardiSerie[code].RitardoCombinato
                    });
                }
            }

            return risultati;
        }

        public class AnalisysItem
        {
            public DateTime Data { get; set; }
            public String Orario { get; set; }
            public int Progressivo { get; set; }
            public string Code { get; set; }
            public int PunteggioMinimo { get; set; }
            public int RitardoStandard { get; set; }
            public int RitardoExtra { get; set; }
            public int RitardoCombinato { get; set; }
        }

        public class Estrazione
        {
            public int Progressivo { get; set; }
            public List<int> NumeriEstratti { get; set; }
        }

        public class RitardoSerie
        {
            public List<int> Combinazione { get; set; }
            public int RitardoStandard { get; set; }
            public int RitardoExtra { get; set; }
            public int RitardoCombinato { get; set; }
        }


        public void Dispose()
        {
        
        }
    }
}
