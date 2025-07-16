using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottologiaPatternAnalyzer.Models
{
    public  class AnalisysItem
    {
        public DateTime DataOra { get; set; }
        public int Estrazione { get; set; }
        public string Name { get; set; }
        public string Sequenza { get; set; }
        public int RR { get; set; }
        public int Indovinati { get; set; }
        public bool WinStandard { get; set; }
        public bool WinExtra { get; set; }
        public int Prize { get; set; }
    }
}
