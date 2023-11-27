using wine_steak.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace wine_steak.Controllers
{
    public class EmployeeController : Controller
    {
		private dbDataContext data = new dbDataContext();
	
		public ActionResult Index()
        {
			UserViewModel viewModel = new UserViewModel();
			viewModel.mon = data.MonAns.OrderBy(a => a.id).Take(10);
			viewModel.hoaDon = new List<HoaDon>();
			var query = from hoadon in data.HoaDons where hoadon.isServed == false select hoadon;
			foreach (HoaDon hoadon in query)
			{
				viewModel.hoaDon.Add(hoadon);
			}

            return View(viewModel);
        }
		
		public ActionResult List()
		{
			var MonAn = data.MonAns.OrderBy(n => n.id).ToList();
			return View(MonAn);
		}

		[HttpGet]
		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Create(MonAn monAn, HttpPostedFileBase fileUpload)
		{
			if (ModelState.IsValid)
			{
				int currentSTT = data.MonAns.Count();

				var fileName = Path.GetFileName(fileUpload.FileName);
				var path = Path.Combine(Server.MapPath("~/Content/img/ratio"), fileName);
                var pathOrigin = Path.Combine(Server.MapPath("~/Content/img"), fileName);
                if (System.IO.File.Exists(path))
				{
					ViewBag.ThongBao = "Hình ảnh đã tồn tại";
				}
				else
				{
					fileUpload.SaveAs(path);
					fileUpload.SaveAs(pathOrigin);
				}
				monAn.Anh = fileName;
				monAn.id = currentSTT + 1;

                data.MonAns.InsertOnSubmit(monAn);
				data.SubmitChanges();
			}
		return RedirectToAction("List");
		}

		[HttpGet]
		public ActionResult Delete(int id)
		{
			MonAn monAn = data.MonAns.SingleOrDefault( n => n.id == id);

			if (monAn == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			ViewBag.id = monAn.id;

			return View(monAn);
		}

		[HttpPost]
		public ActionResult ConfirmDelete(int id)
		{
			MonAn monAn = data.MonAns.SingleOrDefault(n => n.id == id);

			if (monAn == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			ViewBag.id = monAn.id;

			data.MonAns.DeleteOnSubmit(monAn);
			data.SubmitChanges();

			return RedirectToAction("List");
		}

		[HttpGet]
		public ActionResult Edit(int? id)
		{
			MonAn monAn = data.MonAns.SingleOrDefault(n => n.id == id);

			if (monAn == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			return View(monAn);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(MonAn monAn, HttpPostedFileBase fileUpload)
		{
			if (fileUpload == null)
			{
				var query =
						from ord in data.MonAns
						where ord.id == monAn.id
						select ord;

				foreach (MonAn mon in query)
				{
					mon.TenMon = monAn.TenMon;
					mon.MoTa = monAn.MoTa;
					mon.GiaTien = monAn.GiaTien;
					mon.video = monAn.video;
					mon.Calories = monAn.Calories;
					mon.SoLuongMon = monAn.SoLuongMon;
				}
			}
			else
			{
				if (ModelState.IsValid)
				{
					var fileName = Path.GetFileName(fileUpload.FileName);
					var path = Path.Combine(Server.MapPath("~/Content/img/"), fileName);

					if (System.IO.File.Exists(path))
					{
						monAn.Anh = fileName;
					}
					else
					{
						fileUpload.SaveAs(path);
						monAn.Anh = fileName;
					}

					var query =
						from ord in data.MonAns
						where ord.id == monAn.id
						select ord;

					foreach (MonAn mon in query)
					{
						mon.TenMon = monAn.TenMon;
						mon.MoTa = monAn.MoTa;
						mon.GiaTien = monAn.GiaTien;
						mon.video = monAn.video;
						mon.Anh = monAn.Anh;
						mon.Calories = monAn.Calories;
						mon.SoLuongMon = monAn.SoLuongMon;
					}
				}
			}
			data.SubmitChanges();
			return RedirectToAction("List");
		}
		[HttpGet]
		public ActionResult Confirm(HoaDon hoadon)
		{
			return PartialView();
		}
		[HttpPost]
		public ActionResult Confirm(int id)
		{
			var query = from ord in data.HoaDons
						where ord.id == id
						select ord;
			foreach (var bill in query)
			{
				bill.isServed = true;
			}
			data.SubmitChanges();
			return RedirectToAction("Index");
		}

		public List<monCanPhucVu> getListFood(HoaDon hoaDon)
		{
			string foodList = data.HoaDons.SingleOrDefault(n => n.id == hoaDon.id).DanhSachMonAn;
			List<obj> listObj = JsonConvert.DeserializeObject<List<obj>>(foodList);
			List<monCanPhucVu> listMon = new List<monCanPhucVu>();
			for (int i = 0; i < listObj.Count(); i++)
			{
				monCanPhucVu mon = new monCanPhucVu(listObj[i].id, listObj[i].amount, null, null);
				listMon.Add(mon);
			}

			for (int i = 0; i < listMon.Count(); i++)
			{
				var query =
						from ord in data.MonAns
						where ord.id == listMon[i].id
						select ord;

				foreach (MonAn mon in query)
				{
					listMon[i].ten = mon.TenMon;
					listMon[i].anh = mon.Anh;
				}
			}
			return listMon;
		}
	}
}