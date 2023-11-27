using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace wine_steak.Models
{
    public class MonAnInfo
    {
        public int id { get; set; }
        public string TenMon { get; set; }
        public double GiaTien { get; set; }
        public int Amount { get; set; }
        public string Anh { get; set; }
        public int DiemTichLuy { get; set; }
        public int VAT { get; set; }
    }
}