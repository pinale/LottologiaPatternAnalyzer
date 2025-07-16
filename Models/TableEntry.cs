using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottologiaPatternAnalyzer.Models
{
    public  class TableEntry
    {
        public string DrawDate { get; set; }
        public string DrawTime { get; set; }
        public string DrawNumber { get; set; }
        public string DrawFullTimeStamp { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Serie { get; set; }
        public string Objective { get; set; }
        public string RrStandard { get; set; }
        public string RrExtra { get; set; }
        public string RrCombined { get; set; }
    }
}
