using LottologiaPatternAnalyzer.Models;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
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
    public class TextFileScraperService : IDisposable
    {
        string url = "https://www.lottologia.com/10elotto5minuti/gruppi-omogenei/?tab=delay&comb=2&group=MDUuZml2ZS9jaW5xdWluZS1yYWRpY2FsaS1jb3JyZWxhdGl2ZQ%%3D%%3D&ts=1&mode=draws&as=TXT";
       
        public TextFileScraperService() {
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
            CultureInfo.CurrentUICulture = new CultureInfo("it-IT");
        }

        //public async Task<List<AnalisysItem>> GetTableData() {
        //    using (HttpClient client = new HttpClient())
        //    {
        //        // Aggiungi un header User-Agent (simulando un browser)
        //        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        //        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        //        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        //        try
        //        {
        //            // Effettua la richiesta GET
        //            var response = await client.GetAsync(url);

        //            // Controlla se la risposta è OK
        //            response.EnsureSuccessStatusCode();

        //            byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        //            //string contentEncoding = response.Content.Headers.ContentEncoding.ToString();

        //            // Prova a interpretare i dati come UTF-8 o ASCII
        //            string contentUtf8 = Encoding.UTF8.GetString(contentBytes);
        //            string contentAscii = Encoding.ASCII.GetString(contentBytes);

        //            // Verifica se i dati sono compressi in gzip
        //            using (var compressedStream = new MemoryStream(contentBytes))
        //            using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        //            using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
        //            {
        //                // Leggi il contenuto decompresso come stringa
        //                string decompressedContent = reader.ReadToEnd();

        //                // Mostra il contenuto decompresso
        //                Console.WriteLine("Contenuto decompresso:");
        //                Console.WriteLine(decompressedContent);
        //            }

        //        }
        //        catch (HttpRequestException e)
        //        {
        //            Console.WriteLine($"Errore durante il download: {e.Message}");
        //        }
        //        catch (IOException e)
        //        {
        //            Console.WriteLine($"Errore durante la scrittura del file: {e.Message}");
        //        }
        //    }

        //    //var lines = File.ReadAllLines(filePath);
        //    //// Estrarre i dati iniziali
        //    //string dateTimeLine = lines[0];
        //    //string descriptionLine = lines[1];

        //    //// Estrarre data, numero estrazione e orario
        //    //string[] dateTimeParts = dateTimeLine.Split(", ");
        //    //string date = dateTimeParts[0];
        //    //string[] extractionParts = dateTimeParts[1].Split(" - ");
        //    //string extractionNumber = extractionParts[0];
        //    //string time = extractionParts[1];

        //    //string description = descriptionLine;

        //    //// Creare la tabella per i dati
        //    //List<TableEntry> table = new List<TableEntry>();

        //    //for (int i = 4; i < lines.Length; i++) // Inizia dalla quarta riga, dove inizia la tabella
        //    //{
        //    //    if (string.IsNullOrWhiteSpace(lines[i])) continue;

        //    //    string[] parts = lines[i].Split('\t');
        //    //    if (parts.Length == 2)
        //    //    {
        //    //        string serie = parts[0];
        //    //        string rc = parts[1];

        //    //        table.Add(new TableEntry
        //    //        {
        //    //            Date = date,
        //    //            Time = time,
        //    //            ExtractionNumber = extractionNumber,
        //    //            Description = description,
        //    //            Serie = serie,
        //    //            Rc = rc
        //    //        });
        //    //    }
        //    //}

        //    //// Stampare i dati estratti
        //    //Console.WriteLine($"Data: {date}, Estratto: {extractionNumber}, Orario: {time}\n");
        //    //Console.WriteLine("Data/Ora\tEstratto\tDescrizione\tSerie\tRC");

        //    //foreach (var entry in table)
        //    //{
        //    //    Console.WriteLine($"{entry.Date} {entry.Time}\t{entry.ExtractionNumber}\t{entry.Description}\t{entry.Serie}\t{entry.Rc}");
        //    //}


        //    var tableData = new List<AnalisysItem>();

        //    return tableData;
        //}

        public async Task<List<TableEntry>> GetData()
        {
            using (HttpClient client = new HttpClient())
            {
                SetLottologiaHeaders(client); 
                
                try
                {
                    var response = await client.GetAsync(url);
                    // Controlla se la risposta è OK
                    response.EnsureSuccessStatusCode();

                    byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

                    //non mi servono, giusto per controllare la compressione e l'encoding
                    var encoding = response.Content.Headers.ContentEncoding;
                    var contentType = response.Content.Headers.ContentType;

                    // Verifica se i dati sono compressi in gzip
                    using (var compressedStream = new MemoryStream(contentBytes))
                    using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        // Leggi il contenuto decompresso come stringa
                        string decompressedContent = reader.ReadToEnd();
                        return ExtractData(decompressedContent);
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Errore durante il download: {e.Message}");
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Errore durante la decompressione: dati non validi o non in formato GZIP.");
                }
                return null;
            }
        }

        private void SetLottologiaHeaders(HttpClient client)
        {
            //exported session from fidler as curl and convertet to HttpClient headers with ChatGPT
            client.DefaultRequestHeaders.Add("Host", "www.lottologia.com");
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("it-IT", 0.9));
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            client.DefaultRequestHeaders.Add("Cookie", "cross-site-cookie=name; _ga=GA1.1.27593136.1734938676; ..."); // Aggiungi il resto del valore del cookie.
        }

        private List<TableEntry> ExtractData(string testo) {

            // Dividi il testo in righe - Supporta diversi formati di terminatori di riga (Windows, Linux, MacOS). - windows "\r\n", Deriva dalle vecchie stampanti dove: \r riportava il cursore all'inizio della riga e \n passava alla riga successiva
            string[] lines = testo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            // Estrarre i dati iniziali
            string dateTimeLine = lines[0];
            string descriptionLine = lines[1];
            // Estrarre data, numero estrazione e orario
            string[] dateTimeParts = dateTimeLine.Split(", ");
            string date = dateTimeParts[0];
            date = DateTime.Parse(date).ToString("yyyy-MM-dd");
            string[] extractionParts = dateTimeParts[1].Split(" - ");
            string extractionNumber = extractionParts[0].Replace("n.","").Trim();
            string time = extractionParts[1];
            string description = descriptionLine;

            List<TableEntry> table = new List<TableEntry>();
            for (int i = 4; i < lines.Length; i++) // Inizia dalla quarta riga, dove inizia la tabella
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split('\t');
                if (parts.Length == 2)
                {
                    string serie = parts[0].Replace("*","");
                    string rr = parts[1].Replace("*", "");

                    table.Add(new TableEntry
                    {
                        DrawDate = date,
                        DrawTime = time,
                        DrawNumber = extractionNumber,
                        DrawFullTimeStamp = "",
                        Description = description,
                        Serie = serie,
                        RrStandard = rr
                    });
                }
            }

            return table;
        }

        public void Dispose()
        {
        
        }
    }
}
