using LottologiaPatternAnalyzer.Models;
using Microsoft.Playwright;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LottologiaPatternAnalyzer.Services
{
    public class PlaywrightScraperService : IDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IBrowser _browser;
        private readonly IPage _page;
        
        public PlaywrightScraperService() {
            _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            _browser =  _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false }).GetAwaiter().GetResult();
            _page = _browser.NewPageAsync().GetAwaiter().GetResult(); 
        }

        //public async Task GetPattern()
        //{
        //    await _page.GotoAsync("https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?group=MDMudGhyZWUvdGVyemluZS1hcm1vbmljby1zcGVjaWZpY2hlXzE=&comb=3");

        //    var select = _page.Locator("select[name='group']");

        //    var options = select.Locator("option");
        //    Dictionary<string, string> patterns = new Dictionary<string, string>();
        //    foreach (var item in (await options.AllAsync()).AsEnumerable())
        //    {
        //        patterns.Add(await item.InnerTextAsync(), await item.GetAttributeAsync("value"));
        //    }

        //}
        public async Task<List<PatternToAnalize>> GetPatternList() {

            //await _page.GotoAsync("https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?tab=delay");
            await _page.GotoAsync("https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/");

            //var table = page.Locator("css=body>main>div:nth-child(5)>div.table-responsive-sm.mt-3>table>tbody");
            //var rows = await table.Locator("tr").AllAsync();

            List<PatternToAnalize> patterns = new List<PatternToAnalize>();

            var rows = await _page.Locator("css=table>tbody>tr").AllAsync();
            foreach (var row in rows)
            {
                var cells = await row.Locator("td").AllAsync();
                var description = await cells[0].InnerTextAsync();
                var punti_1 = cells.Count > 2 ? await cells[1].Locator("a").GetAttributeAsync("href") : null;
                var punti_2 = cells.Count > 3 ? await cells[2].Locator("a").GetAttributeAsync("href") : null;
                var punti_3 = cells.Count > 4 ? await cells[3].Locator("a").GetAttributeAsync("href") : null;
                var punti_4 = cells.Count > 5 ? await cells[4].Locator("a").GetAttributeAsync("href") : null;
                var punti_5 = cells.Count > 6 ? await cells[5].Locator("a").GetAttributeAsync("href") : null;
                var punti_6 = cells.Count > 7 ? await cells[6].Locator("a").GetAttributeAsync("href") : null;
                var punti_7 = cells.Count > 8 ? await cells[7].Locator("a").GetAttributeAsync("href") : null;
                var punti_8 = cells.Count > 9 ? await cells[8].Locator("a").GetAttributeAsync("href") : null;
                var punti_9 = cells.Count > 10 ? await cells[9].Locator("a").GetAttributeAsync("href") : null;
                var punti_10 = cells.Count > 11 ? await cells[10].Locator("a").GetAttributeAsync("href") : null;

                patterns.Add(new PatternToAnalize()
                {
                    Description = description,
                    Punti_1 = punti_1,
                    Punti_2 = punti_2,
                    Punti_3 = punti_3,
                    Punti_4 = punti_4,
                    Punti_5 = punti_5,
                    Punti_6 = punti_6,
                    Punti_7 = punti_7,
                    Punti_8 = punti_8,
                    Punti_9 = punti_9,
                    Punti_10 = punti_10
                }); 
            }
            return patterns;
        }

        public async Task<string> GetCountDown()
        {
            var countDown = await GetCountDown(_page);
            return countDown;
        }
       
        public async Task<List<TableEntry>> GetData()
        {
            var tableData = new List<TableEntry>();
            try
            {
                await _page.GotoAsync("https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?tab=delay&comb=2&group=MDUuZml2ZS9jaW5xdWluZS1yYWRpY2FsaS1jb3JyZWxhdGl2ZQ%3D%3D&ts=1&mode=draws-and-extra");

                var description = await GetDescription(_page);
                
                //var xxx = await GetDateTime(_page);
                var estrazioneRaw = await GetEstrazioneRaw(_page);
                var estrazioneParts = estrazioneRaw.Split([',','-'],StringSplitOptions.TrimEntries);

                var table = _page.Locator("css=#home > div.table-responsive-sm > table tbody");

                var rows = await table.Locator("tr").AllAsync();
                foreach (var row in rows)
                {
                    var cells = await row.Locator("td").AllAsync();
                    var extractionNumber = (await cells[0].InnerTextAsync()).Replace("\n", ".");
                    var rr = await cells[1].InnerTextAsync();
                    var item = new TableEntry()
                    {
                        DrawDate = DateTime.Parse(estrazioneParts[0]).ToString("yyyy-MM-dd"),
                        DrawTime = estrazioneParts[2],
                        DrawNumber = estrazioneParts[1].Replace("n.", ""),
                        DrawFullTimeStamp = "",
                        Description = description,
                        Serie = extractionNumber,
                        RrStandard = rr
                    };
                    tableData.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                //await page.CloseAsync();
            }

            return tableData;
        }

        private async Task<string> GetDescription(IPage page)
        {
            var description = await page.Locator("css=body>main>div:nth-child(5)>h1").InnerTextAsync();
            return description;
        }

        private async Task<string> GetCountDown(IPage page) {
            var countDown = await page.Locator("css=#countdown").InnerTextAsync();
            return countDown;
        }

        //private async Task<string> GetDateTime(IPage page)
        //{
        //    var dateTime = await page.Locator("css=time").GetAttributeAsync("datetime"); 
        //    return dateTime;
        //}

        private async Task<string> GetEstrazioneRaw(IPage page)
        {
            var dateTime = await page.Locator("css=#collapseActualCD > h3").InnerTextAsync();
            return dateTime;
        }



        public void Dispose()
        {
            _page.CloseAsync().GetAwaiter().GetResult();

            _browser.CloseAsync().GetAwaiter().GetResult();
            _playwright.Dispose();
        }
    }
}
