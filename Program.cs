namespace LottologiaPatternAnalyzer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start analisys");

            //var PatternAnalyzer = new Engine.PatternAnalyzer();
            //await PatternAnalyzer.StartAnalisys();

            string seriesPattern = "5_%";  //okkio, se metto 5_1% prendera anche i 5_10_xx _ è un carattere jolly in sqlite, viene fatto l'escaping interno
            int minimumTarget = 3;
            DateTime analisysDay = new DateTime(2025, 06, 01);
            
            var pafs = new Engine.PatternAnalyzerFromScratch();
            await pafs.StartAnalisys(seriesPattern,minimumTarget,analisysDay);

            Console.ReadLine();
        }
    }
}
