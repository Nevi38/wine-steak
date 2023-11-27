using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wine_steak.Models;

namespace wine_steak.Controllers
{
    public class CashierController : Controller
    {
        private dbDataContext db = new dbDataContext();
        
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            string username = collection["username"];
            string password = collection["password"];

            var account = from tk in db.TaiKhoans
                           where tk.username == username 
                           && tk.password == password 
                           && tk.role == "cashier"
                           select tk;

            bool isAccount = account.Any();
            TaiKhoan taikhoan = account.SingleOrDefault();

            if (isAccount) {
                // Lấy thông tin nhân viên
                NhanVien nv = (from n in db.NhanViens
                               where n.id == taikhoan.id
                               select n).SingleOrDefault();

                Session["cashier"] = nv.HoTen;

                return RedirectToAction("Index", "Cashier");
            } 
            else {
                ViewBag.message = "Tài khoản hoặc mật khẩu không hợp lệ";
                return View();
            }
        }

        public ActionResult Index()
        {
            bool isLogin = (Session["cashier"] != null) ? true : false;
            if (isLogin)
            {
                ViewBag.user = Session["cashier"];

                return View();
            }
            else
                return RedirectToAction("Login", "Cashier");
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            int masoban = Int32.Parse(collection["masoban"]);

            // Tim ma so ban
            HoaDon hoadon = (from hoaDon in db.HoaDons
                             where hoaDon.MaSoBan == masoban && hoaDon.isPayment == false
                             select hoaDon).SingleOrDefault();

            if (hoadon != null)
                return RedirectToAction("Payment", "Cashier", new { @masoban = masoban });
            else
            {
                ViewBag.message = "Bàn số " + masoban + " không hợp lệ";
                ViewBag.user = Session["cashier"];
                return View();
            }
        }

        public ActionResult Payment(int masoban)
        {
            bool isLogin = (Session["cashier"] != null) ? true : false;
            if (!isLogin)
                return RedirectToAction("Login", "Cashier");
            
            ViewBag.user = Session["cashier"];
            ViewBag.masoban = masoban;
            Session["masoban"] = masoban;

            var query = from hoaDon in db.HoaDons
                        where hoaDon.MaSoBan == masoban && hoaDon.isPayment == false
                        select hoaDon.DanhSachMonAn;

            var danhSachMonAn = query.SingleOrDefault();

            if (!string.IsNullOrEmpty(danhSachMonAn))
            {
                // Chuyển chuỗi JSON thành danh sách đối tượng
                List<MonAnInfo> monAnInfoList = JsonConvert.DeserializeObject<List<MonAnInfo>>(danhSachMonAn);

                // Lấy danh sách id từ danh sách MonAnInfo
                var monAnIds = monAnInfoList.Select(info => info.id).ToList();

                // Truy vấn thông tin món ăn từ bảng MonAn dựa trên danh sách id
                var monAnDetails = (from monAn in db.MonAns
                                    where monAnIds.Contains(monAn.id)
                                    select monAn)
                                    .ToList();

                // Tạo danh sách kết quả cuối cùng kết hợp thông tin món ăn và amount từ MonAnInfo
                List<MonAnInfo> danhsachmon = (from monAn in monAnDetails
                              join monAnInfo in monAnInfoList on monAn.id equals monAnInfo.id
                              select new MonAnInfo
                              {
                                  id = monAn.id,
                                  TenMon = monAn.TenMon,
                                  GiaTien = monAn.GiaTien ?? 0, // null coalescing
                                  Amount = monAnInfo.Amount,
                                  Anh = monAn.Anh,
                              }).ToList();

                HoaDon hoadon = (from hoaDon in db.HoaDons
                              where hoaDon.MaSoBan == masoban && hoaDon.isPayment == false
                              select hoaDon).SingleOrDefault();
             
                ViewBag.diemtichluy = hoadon.DiemTichLuy;
                ViewBag.VAT = hoadon.VAT;
                ViewBag.ThanhTien = hoadon.ThanhTien;
                ViewBag.soluong = tongsoluong(danhsachmon);

                return View(danhsachmon);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Payment(int sotienthanhtoan, int masoban, FormCollection collection)
        {
            int money = Int32.Parse(collection["money"]);
            System.Diagnostics.Debug.WriteLine("Tien khach tra: " + money);
            System.Diagnostics.Debug.WriteLine("Tien phai thanh toan: " + sotienthanhtoan);
            int tienthua = money - sotienthanhtoan;
            
            // Cập nhật hóa đơn
            HoaDon hoadon = (from hoaDon in db.HoaDons
                             where hoaDon.MaSoBan == masoban && hoaDon.isPayment == false
                             select hoaDon).SingleOrDefault();
            hoadon.isPayment = true;
            db.SubmitChanges();

            return RedirectToAction("SuccessPayment", "Cashier", new { @TienThua = tienthua, @diemtichluy = hoadon.DiemTichLuy});
        }

        private int tongsoluong(List<MonAnInfo> danhsachMonAn)
        {
            int tong = 0;

            foreach(MonAnInfo mon in danhsachMonAn)
            {
                tong += mon.Amount;
            }

            return tong;
        }

        public ActionResult SuccessPayment(int TienThua, int diemtichluy)
        {
            ViewBag.tienthua = TienThua;
            ViewBag.diemtichluy = diemtichluy;

            return View();
        }
    }
}