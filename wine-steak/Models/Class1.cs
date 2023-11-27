using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wine_steak.Models
{
	public class obj
	{
		public int id { get; set; }
		public int amount { get; set; }
	}
	public class monCanPhucVu
	{
		public monCanPhucVu(int id, int amount, string v1, string v2)
		{
			this.id = id;
			this.amount = amount;
			anh = v1;
			ten = v2;
		}

		public int id { get; set; }
		public int amount { get; set; }
		public string anh { get; set; }
		public string ten { get; set; }
	}
	public class UserViewModel
	{
		public IEnumerable<MonAn> mon { get; set; }
		public List<HoaDon> hoaDon { get; set; }
	}

	public class UserViewModel2
	{
		public UserViewModel2(HoaDon hoadon, List<monCanPhucVu> mon)
		{
			this.hoadon = hoadon;
			this.mon = mon;
		}
		public UserViewModel2()
		{

		}
		public HoaDon hoadon { get; set; }
		public List<monCanPhucVu> mon { get; set; }
	}
}