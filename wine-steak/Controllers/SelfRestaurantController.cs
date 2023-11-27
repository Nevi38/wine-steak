using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using wine_steak.Models;

namespace SelfRestaurant.Controllers
{
    public class SelfRestaurantController : Controller
    {
        private int defaultTableID = 778;
        private int defaultVAT = 4;

        // GET: SelfRestaurant
        dbDataContext db = new dbDataContext();
        public int MaSoBan()
        {
            // Lấy giá trị từ CookieIndex
            var maSoBanCookie = HttpContext.Request.Cookies["MaSoBan"]?.Value;

            // Kiểm tra xem giá trị có tồn tại không
            if (!string.IsNullOrEmpty(maSoBanCookie))
            {
                // Chuyển đổi giá trị thành kiểu int và gán cho biến defaultTableID
                if (int.TryParse(maSoBanCookie, out int tableID))
                {
                    defaultTableID = tableID;
                }
            }
            return defaultTableID;
        }

        //Tạo List Food từ string hoadon.DanhSachMonAn
        public List<Food> LayDSMonAn(string listFood)
        {
            List<Food> foods = JsonConvert.DeserializeObject<List<Food>>(listFood);

            // Lay them anh mon an
            foreach (Food food in foods)
            {
                MonAn monAn = (db.MonAns.Where(n => n.id == food.id)).SingleOrDefault();

                food.Anh = monAn.Anh;
            }

            return foods;
        }

        private int SoLuongMon(List<Food> danhsachmon)
        {
            int sum = 0;

            for (int i = 0; i < danhsachmon.Count; i++)
                sum += danhsachmon[i].amount;

            return sum;
        }

        public ActionResult Index()
        {
            //Hiển thị danh sách món ăn
            var listFood = from f in db.MonAns select f;
            return View(listFood);
        }
        public double TongTien(string listFood)
        {
            //Tính tổng tiền từ List food
            List<Food> foods = LayDSMonAn(listFood);
            double Total = 0;
            Total = foods.Sum(n => n.ThanhTien);
            return Total;
        }
        public ActionResult OrderPartial()
        {
            // Lấy hóa đơn isPayMent=fasle và MaSoBan == MaSoBan() hiện tại
            double Total = 0;
            HoaDon hoadon = db.HoaDons.Where(hd => hd.isPayment == false && hd.MaSoBan == MaSoBan())
                .OrderByDescending(hd => hd.id).FirstOrDefault();
            if (hoadon != null)
            {
                string listFood = hoadon.DanhSachMonAn;
                List<Food> foods = LayDSMonAn(listFood);
                ViewBag.Total = TongTien(listFood);
                return PartialView(foods);
            }
            else
            {
                return null;
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Addfood(int id)
        {
            int maxId = db.HoaDons.Max(n => n.id);
            HoaDon hoadon = db.HoaDons.Where(hd => hd.MaSoBan == MaSoBan())
                .OrderByDescending(hd => hd.id).FirstOrDefault();
            MonAn f = db.MonAns.FirstOrDefault(n => n.id == id);
            if (hoadon == null)
            {
                if (f.SoLuongMon > 0)
                {
                    var jsonObject = new { id = id, amount = 1 };
                    string InitlistFood = JsonConvert.SerializeObject(new[] { jsonObject });
                    HoaDon newHoaDon = new HoaDon();
                    newHoaDon.id = maxId + 1;
                    newHoaDon.DanhSachMonAn = InitlistFood;
                    newHoaDon.MaKhachHang = 1;
                    newHoaDon.ThanhTien = TongTien(InitlistFood);
                    newHoaDon.NgayDat = null;
                    newHoaDon.MaSoBan = MaSoBan();
                    newHoaDon.isPayment = false;
                    db.HoaDons.InsertOnSubmit(newHoaDon);
                    f.SoLuongMon = f.SoLuongMon - 1;
                    db.SubmitChanges();
                    return Json(new { code = 100, msg = "Thêm món thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 200, msg = "Món này đã hết xin vui lòng chọn món khác" }, JsonRequestBehavior.AllowGet);
                }

            }
            if (hoadon != null)
            {
                if (hoadon.isPayment == false && hoadon.NgayDat == null)
                {

                    if (f.SoLuongMon > 0)
                    {
                        List<Food> foods = LayDSMonAn(hoadon.DanhSachMonAn);
                        Food existingFood = foods.FirstOrDefault(n => n.id == id);

                        if (existingFood != null)//Nếu món đó có trong danh sách món ăn trong hóa đơn thì tăng SL
                        {
                            existingFood.amount += 1;
                            existingFood.ThanhTien = existingFood.amount * existingFood.GiaTien;
                        }
                        else
                        {
                            var newFood = new Food(id, 1);
                            foods.Add(newFood);
                        }
                        var listTemp = foods.Select(n => new { id = n.id, amount = n.amount }).ToList();

                        hoadon.DanhSachMonAn = JsonConvert.SerializeObject(listTemp);
                        hoadon.ThanhTien = TongTien(hoadon.DanhSachMonAn);
                        f.SoLuongMon = f.SoLuongMon - 1;
                        db.SubmitChanges();
                        return Json(new { code = 100, msg = "Thêm món thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { code = 200, msg = "Món này đã hết xin vui lòng chọn món khác" }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (hoadon.isPayment == false && hoadon.NgayDat != null)
                {
                    return Json(new { code = 300, msg = "Hóa đơn đang chờ thanh toán không thể gọi thêm" }, JsonRequestBehavior.AllowGet);
                }
                if (hoadon.isPayment == true)
                {
                    if (f.SoLuongMon > 0)
                    {
                        var jsonObject = new { id = id, amount = 1 };
                        string InitlistFood = JsonConvert.SerializeObject(new[] { jsonObject });
                        HoaDon newHoaDon = new HoaDon();
                        newHoaDon.id = maxId + 1;
                        newHoaDon.DanhSachMonAn = InitlistFood;
                        newHoaDon.MaKhachHang = 1;
                        newHoaDon.ThanhTien = TongTien(InitlistFood);
                        newHoaDon.NgayDat = null;
                        newHoaDon.MaSoBan = MaSoBan();
                        newHoaDon.isPayment = false;
                        db.HoaDons.InsertOnSubmit(newHoaDon);
                        f.SoLuongMon = f.SoLuongMon - 1;
                        db.SubmitChanges();
                        return Json(new { code = 100, msg = "Thêm món thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { code = 200, msg = "Món này đã hết xin vui lòng chọn món khác" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { });

        }
        [HttpPost]
        public JsonResult Increase(int id)
        {
            //nhấn nút + để tăng số lượng
            HoaDon hoadon = db.HoaDons.Where(hd => hd.isPayment == false
            && hd.MaSoBan == MaSoBan() && hd.NgayDat == null).OrderByDescending(hd => hd.id).FirstOrDefault();
            MonAn f = db.MonAns.FirstOrDefault(n => n.id == id);
            if (hoadon != null)
            {
                List<Food> foods = LayDSMonAn(hoadon.DanhSachMonAn);
                Food food = foods.FirstOrDefault(n => n.id == id);
                if (f.SoLuongMon > 0)//kiểm tra sl món còn hay không
                {
                    food.amount += 1;
                    food.ThanhTien = food.amount * food.GiaTien;
                    var listTemp = foods.Select(n => new { id = n.id, amount = n.amount }).ToList();
                    hoadon.DanhSachMonAn = JsonConvert.SerializeObject(listTemp);
                    hoadon.ThanhTien = TongTien(hoadon.DanhSachMonAn);
                    f.SoLuongMon -= 1;
                    db.SubmitChanges();
                    return Json(new { code = 100 }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { code = 200, msg = "Số lượng đã hết" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = 500, msg = "Đã xác nhận đặt không thể gọi thêm" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult Decrease(int id)
        {
            //Khi nhấn nút -
            HoaDon hoadon = db.HoaDons.Where(hd => hd.isPayment == false
            && hd.MaSoBan == MaSoBan() && hd.NgayDat == null).OrderByDescending(hd => hd.id).FirstOrDefault();
            List<Food> foods = LayDSMonAn(hoadon.DanhSachMonAn);
            MonAn f = db.MonAns.FirstOrDefault(n => n.id == id);
            Food food = foods.FirstOrDefault(n => n.id == id);
            if (hoadon != null)
            {
                if (food.amount > 0)// Nếu món đó không DanhSachMonAn thì chỉ trừ
                {
                    food.amount -= 1;
                    if (food.amount > 0)
                    {
                        food.ThanhTien = food.amount * food.GiaTien;
                        var listTemp = foods.Select(n => new { id = n.id, amount = n.amount }).ToList();
                        hoadon.DanhSachMonAn = JsonConvert.SerializeObject(listTemp);
                        hoadon.ThanhTien = TongTien(hoadon.DanhSachMonAn);
                    }
                    else//Nếu như số lượng bằng 0 thì xóa món đó ra khỏi DanhSachMonAn
                    {
                        foods.RemoveAll(n => n.id == food.id);
                        if (foods.Count > 0)
                        {
                            var listTemp = foods.Select(n => new { id = n.id, amount = n.amount }).ToList();
                            hoadon.DanhSachMonAn = JsonConvert.SerializeObject(listTemp);
                            hoadon.ThanhTien = TongTien(hoadon.DanhSachMonAn);
                        }
                        else//Nếu DSMonAn không còn thì xóa hóa đơn
                        {
                            db.HoaDons.DeleteOnSubmit(hoadon);
                        }
                    }
                }
                f.SoLuongMon += 1;// Cập nhật lại số lượng tồn 
                db.SubmitChanges();
                return Json(new { code = 200, msg = "" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = 500, msg = "" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SubNavPartial()
        {
            return PartialView();
        }
        public ActionResult CategoryPartial()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult PayMen()
        {
            try
            {
                HoaDon hoadon = db.HoaDons.Where(hd => hd.isPayment == false
                && hd.MaSoBan == MaSoBan() && hd.NgayDat == null).OrderByDescending(hd => hd.id).FirstOrDefault();
                if (hoadon != null)
                {
                    hoadon.NgayDat = DateTime.Now;
                    List<Food> danhsachMon = LayDSMonAn(hoadon.DanhSachMonAn);
                    hoadon.DiemTichLuy = SoLuongMon(danhsachMon);
                    hoadon.VAT = defaultVAT;
                    hoadon.isServed = false;
                    db.SubmitChanges();
                    return Json(new { code = 200, msg = "Xác nhận thành công" }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new { code = 500, msg = "Hóa đơn hiện tại đang chờ thanh toán không thể gọi thêm" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Xác nhận thất bại" + ex.Message }, JsonRequestBehavior.DenyGet);
            }
        }
        [HttpPost]
        public JsonResult CheckAccount(string username, string password)
        {
            // Thực hiện kiểm tra username và password ở đây
            TaiKhoan tk = db.TaiKhoans.FirstOrDefault(t => t.username == username && t.password == password);
            if (tk != null)
            {
                tk.role.Equals("admin");
                return Json(new { code = 200, msg = "Đăng nhập thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { code = 500, msg = "Đăng nhập thất bại" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckPayMent(string username, string password)
        {
            HoaDon hoadon = db.HoaDons.Where(hd => hd.MaSoBan == MaSoBan()).OrderByDescending(hd => hd.id).FirstOrDefault();

            if (hoadon.isPayment == true)
            {
                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { code = 500, msg = "Có hóa đơn chưa thanh toán không thể cập nhật" }, JsonRequestBehavior.AllowGet);
        }
    }
}