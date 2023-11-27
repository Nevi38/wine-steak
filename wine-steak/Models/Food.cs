using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wine_steak.Models
{
    public class Food
    {
        private dbDataContext db = new dbDataContext();
        public int id { get; set; }

        public string Anh { get; set; }
        public double GiaTien { get; set; }
        public string TenMon { get; set; }
        public int amount { get; set; }
        public double ThanhTien { get; set; }
        public Food(int id, int amount)
        {
            MonAn f = db.MonAns.Single(n => n.id == id);
            this.id = id;
            this.TenMon = f.TenMon;
            this.GiaTien = double.Parse(f.GiaTien.ToString());
            this.amount = amount;
            ThanhTien = amount * GiaTien;
        }
    }
}