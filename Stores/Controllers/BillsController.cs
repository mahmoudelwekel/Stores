﻿using CrystalDecisions.CrystalReports.Engine;
using Newtonsoft.Json;
using Stores.Models;
using Stores.Models.CommonClasses;
using Stores.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Stores.Controllers
{
    public class BillsController : Controller
    {
        // GET: Bills
        private ProjectContext _db = new ProjectContext();
        private Security s = new Security();


        #region Burches


        #region index

        public ActionResult Purchases()
        {
            bool res = s.purchasebill();
            if (res == true)
            {
                var model = new BillsWithExten();
                model.prodCategoryX = _db.ProductCategory.ToList();
                model.ClientsX = _db.Clients.Where(f => f.Clients_Type_ID == 1).ToList();



                ViewBag.product = new SelectList(_db.Products.ToList(), "Pro_id", "name");
                ViewBag.productCat = new SelectList(_db.ProductCategory.ToList(), "Cate_ID", "name");

                Session["BillCategory"] = 1;
                return View(model);
            }
            return RedirectToAction("HavntAccess", "Employee");


        }

        #endregion


        // barcode
        public JsonResult Barecode(string _barcode)
        {

            List<Produt_Price> result = new List<Produt_Price>();

            var model = _db.Products.Where(i => i.barcode == _barcode).FirstOrDefault();
            var model2 = _db.Produt_Price.Where(i => i.Pro_ID == model.Pro_id).ToList();
            // decimal result = model.Quantity;
            foreach (var item in model2)
            {
                Produt_Price mo = new Produt_Price();
                mo.Prd_Pri_ID = item.Prd_Pri_ID;
                mo.Quantity = item.Quantity;
                mo.Minmum = item.Minmum;
                mo.cost = item.cost;
                mo.Price = item.Price;
                mo.Pro_ID = item.Pro_ID;
                result.Add(mo);
            }
            return Json(result, JsonRequestBehavior.AllowGet);


        }

        // barcode product name 

        public JsonResult getProNameFromBarcode(string barcode)
        {
            var model = _db.Products.Where(id => id.barcode == barcode).FirstOrDefault();
            return Json(model, JsonRequestBehavior.AllowGet);

        }
        public JsonResult getCateNameFromBarcode(string barcode)
        {
            var model = _db.Products.Where(id => id.barcode == barcode).Select(f => f.Cate_ID).FirstOrDefault();
            var model2 = _db.ProductCategory.Where(id => id.Cate_ID == model).FirstOrDefault();
            return Json(model2, JsonRequestBehavior.AllowGet);

        }



        #region retuern cat


        public JsonResult GetcategoryPro(string Pro_id)
        {

            List<Products> cat = new List<Products>();


            var s = _db.ProductCategory.Where(ss => ss.name == Pro_id).FirstOrDefault();
            var obj = _db.Products.Where(p => p.Cate_ID == s.Cate_ID && p.active == true).ToList();

            if (obj != null && obj.Count() > 0)
            {
                foreach (var item in obj)
                {
                    Products model = new Products();
                    model.Pro_id = item.Pro_id;
                    model.name = item.name;
                    cat.Add(model);
                }
            }

            return Json(cat, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region fillBills

        // return client product name fatoracontent

        public JsonResult ReturnProName(int Product_ID)
        {
            var model = _db.Products.Where(id => id.Pro_id == Product_ID).FirstOrDefault();
            return Json(model, JsonRequestBehavior.AllowGet);

        }


        // return client client for fatoracontent
        public JsonResult ReturnClientName(int BillsContent_ID)
        {
            var model = _db.BillsContent.Where(id => id.BillsContent_ID == BillsContent_ID).FirstOrDefault();
            var modelBill = _db.Bills.Where(id => id.Id == model.Bill_ID).FirstOrDefault();
            int s = modelBill.Client_ID;
            var modelClient = _db.Clients.Where(id => id.Client_ID == s).FirstOrDefault();
            return Json(modelClient, JsonRequestBehavior.AllowGet);

        }




        public JsonResult FillBills()
        {
            List<BillsContent> list = new List<BillsContent>();
            // List<BillsWithExten> list = new List<BillsWithExten>();

            int UserId = int.Parse(Session["userID"].ToString());

            var mo = _db.Bills.Where(p => p.User_ID == UserId && p.Viewed == true).ToList();

            var lastIDInBIlls = mo.OrderByDescending(p => p.Id).FirstOrDefault();




            var obj = _db.BillsContent.Where(ss => ss.Bill_ID == lastIDInBIlls.Id && ss.IsDeleted == false && ss.Viewed == true).ToList();

            if (obj != null && obj.Count() > 0)
            {
                foreach (var item in obj)
                {
                    BillsContent model = new BillsContent();

                    model.Product_ID = item.Product_ID;
                    model.BillsContent_ID = item.BillsContent_ID;
                    model.Price = item.Price;
                    model.Quantity = item.Quantity;
                    model.IsDeleted = item.IsDeleted;

                    list.Add(model);
                }



            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region retuern cat


        public JsonResult ClientCat(string Clients_Type_id)
        {

            List<Products> cat = new List<Products>();


            var s = _db.Clients_Type.Where(ss => ss.name == Clients_Type_id).FirstOrDefault();
            var obj = _db.Clients.Where(p => p.Clients_Type_ID == s.Clients_Type_id).ToList();

            if (obj != null && obj.Count() > 0)
            {
                foreach (var item in obj)
                {
                    Products model = new Products();
                    model.Pro_id = item.Client_ID;
                    model.name = item.name;
                    cat.Add(model);
                }
            }

            return Json(cat, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region add client

        public JsonResult SaveClientData(Clients model)
        {

            try
            {
                model.Clients_Type_ID = 1; ;

                _db.Clients.Add(model);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region return price and quntity value 

        public JsonResult ReturnValue(int Pro_id)
        {
            List<Produt_Price> result = new List<Produt_Price>();

            var model = _db.Produt_Price.Where(i => i.Pro_ID == Pro_id);
            // decimal result = model.Quantity;
            foreach (var item in model)
            {
                Produt_Price mo = new Produt_Price();
                mo.Prd_Pri_ID = item.Prd_Pri_ID;
                mo.Quantity = item.Quantity;
                mo.Minmum = item.Minmum;
                mo.cost = item.cost;
                mo.Price = item.Price;
                result.Add(mo);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        //return barcode and productname value when pro change

        public JsonResult ReturnBarcodeValue(int Pro_id)
        {
            List<Products> result = new List<Products>();

            var model = _db.Products.Where(i => i.Pro_id == Pro_id);
            // decimal result = model.Quantity;
            foreach (var item in model)
            {
                Products mo = new Products();
                mo.Pro_id = item.Pro_id;
                mo.name = item.name;
                mo.barcode = item.barcode;
                mo.active = item.active;

                result.Add(mo);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region SaveBillData

        public JsonResult SaveBillData(BillsContent model)
        {
            bool result = true;

            try
            {
                _db.BillsContent.Add(model);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region AddBillsContent 
        //add client id
        //[HttpPost]
        //public ActionResult AddBillsContent(BillsContent _BillContent, Bills _bill, int pro, int barcodeProduct, Produt_Price pro_price, int Quantity, string Client_ID)
        //{

        //    bool res = s.Users();
        //    if (res == true)
        //    {



        //        if (Session["flag"].ToString() == "true")
        //        {
        //            _bill.date = DateTime.Now;
        //         
        //            var ClintID = _db.Clients.Where(p => p.name == Client_ID).FirstOrDefault();
        //            _bill.Client_ID = ClintID.Client_ID;
        //      
        //            Session["CLientID"]= ClintID.Client_ID;
        //            _bill.Cate_Id = 4;

        //            _bill.User_ID = int.Parse(Session["userID"].ToString());
        //            _bill.Viewed = true;
        //            _db.Bills.Add(_bill);
        //            _db.SaveChanges();
        //        }

        //        var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
        //        _BillContent.Bill_ID = LastId.Id;

        //       

        //        _BillContent.Quantity = _BillContent.Quantity;
        //        _BillContent.Product_ID =   pro;
        //        _BillContent.Cost = _db.Produt_Price.Where(s => s.Pro_ID == pro).Select(p => p.cost).FirstOrDefault();
        //        _BillContent.Viewed = true;

        //        _db.BillsContent.Add(_BillContent);
        //        _db.SaveChanges();

        //        //edit quntity
        //        var proQuantity = _db.Produt_Price.Where(s => s.Pro_ID == pro).FirstOrDefault();
        //        Produt_Price model = _db.Produt_Price.Find(proQuantity.Prd_Pri_ID);

        //        model.Quantity = model.Quantity - Quantity;
        //        model.Store_Id = 1;
        //        model.Pro_ID = pro;
        //        model.Minmum = model.Minmum;
        //        model.many_price = model.many_price;
        //        model.Price = model.Price;
        //        model.cost = model.cost;


        //        _db.Entry(model).State = EntityState.Modified;
        //        _db.SaveChanges();


        //        Session["flag"] = "false";

        //        return RedirectToAction("Purchases");
        //    }

        //    return RedirectToAction("HavntAccess", "Employee");

        //}


        [HttpPost]
        public ActionResult AddBillsContent(BillsContent _BillContent, Bills _bill, string barcodeProduct, Produt_Price pro_price, int Quantity, string Client_ID)
        {

            bool res = s.Users();
            if (res == true)
            {



                if (Session["flag"].ToString() == "true")
                {
                    _bill.date = DateTime.Now;

                    var ClintID = _db.Clients.Where(p => p.name == Client_ID).FirstOrDefault();
                    _bill.Client_ID = ClintID.Client_ID;

                    Session["CLientID"] = ClintID.Client_ID;
                    _bill.Cate_Id = 4;

                    _bill.User_ID = int.Parse(Session["userID"].ToString());
                    _bill.Viewed = true;
                    _db.Bills.Add(_bill);
                    _db.SaveChanges();
                }

                var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
                _BillContent.Bill_ID = LastId.Id;

                //    _BillContent.Price = price;

                _BillContent.Quantity = _BillContent.Quantity;
                var prodids = _db.Products.Where(d => d.name == barcodeProduct).Select(f => f.Pro_id).FirstOrDefault();
                _BillContent.Product_ID = prodids;
                _BillContent.Cost = _db.Produt_Price.Where(s => s.Pro_ID == prodids).Select(p => p.cost).FirstOrDefault();
                _BillContent.Viewed = true;

                _db.BillsContent.Add(_BillContent);
                _db.SaveChanges();

                //edit quntity
                var proQuantity = _db.Produt_Price.Where(s => s.Pro_ID == prodids).FirstOrDefault();
                Produt_Price model = _db.Produt_Price.Find(proQuantity.Prd_Pri_ID);

                model.Quantity = model.Quantity - Quantity;
                model.Store_Id = 1;
                model.Pro_ID = prodids;
                model.Minmum = model.Minmum;
                model.many_price = model.many_price;
                model.Price = model.Price;
                model.cost = model.cost;


                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();


                Session["flag"] = "false";

                return RedirectToAction("Purchases");
            }

            return RedirectToAction("HavntAccess", "Employee");

        }

        #endregion

        // gitt bill by id for edit

        #region  edite record

        public JsonResult GetBillById(int BillsContent_ID)
        {
            BillsContent model = _db.BillsContent.Where(x => x.BillsContent_ID == BillsContent_ID).SingleOrDefault();
            string value = string.Empty;
            value = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region  buttin for new fatora

        //public ActionResult NewFatora( decimal? discount, decimal? elmodfoa,Payments payments)
        //{


        //    decimal percent;
        //    decimal addeddiscount;

        //    //if (Discount.Contains("%"))
        //    //{

        //    //    percent = Convert.ToDecimal(Discount.Replace("%", "")) / 100;
        //    //    //addeddiscount = totalFatora * percent;



        //    //    var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
        //    //    Bills bill = _db.Bills.Find(LastId.Id);

        //    //    bill.price = _db.BillsContent.Where(s=>s.Bill_ID==LastId.Id).Sum(p=>p.Price);
        //    //    bill.cost = _db.BillsContent.Where(s => s.Bill_ID == LastId.Id).Sum(p => p.Cost);

        //    //    _db.Entry(bill).State = EntityState.Modified;
        //    //    _db.SaveChanges();

        //    //}
        //    //else
        //    //{
        //    //    try
        //    //    {
        //    var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
        //    Bills bill = _db.Bills.Find(LastId.Id);

        // //   addeddiscount = Convert.ToDecimal(Discount);

        //    decimal price = 0;
        //    decimal cost = 0;
        //    var model = _db.BillsContent.Where(s => s.Bill_ID == LastId.Id).ToList();
        //    foreach (var item in model)
        //    {
        //        price += item.Quantity * item.Price;
        //    }

        //    foreach (var item in model)
        //    {
        //        cost += item.Quantity * item.Cost;
        //    }

        //    bill.price = price;
        //    bill.cost = cost;
        //  bill.discount = discount ?? default(decimal);

        //    _db.Entry(bill).State = EntityState.Modified;
        //    _db.SaveChanges();


        //    payments.client_id = 1;
        //    payments.client_id = 1;
        //    payments.date = DateTime.Now;

        //    payments.Payment_amount = elmodfoa ?? default(decimal);

        //    _db.Payments.Add(payments);
        //    _db.SaveChanges();

        //    Session["flag"] = "true";

        //         return RedirectToAction("Purchases");
        //    }
        //    catch (Exception)
        //    {
        //        addeddiscount = 0;
        //    }
        //}


        //   


        //  }
        #endregion


        #region Newfatora
        [HttpPost]
        public ActionResult Newfatora(decimal? discount, decimal? elmodfoa, Payments payments, int fatoraID)
        {
            bool res = s.purchasebill();
            if (res == true)
            {
                int userID = int.Parse(Session["userID"].ToString());
                // printer name
                string PrinterID = _db.Users.Where(d => d.Id == userID).Select(f => f.printer_name).FirstOrDefault();
                string printerName = _db.PrintType.Where(id => id.ID.ToString() == PrinterID).Select(f => f.PrinterName).FirstOrDefault();


                var mo = _db.Bills.Where(p => p.User_ID == userID).ToList();

                var lastIDInBIlls = mo.OrderByDescending(p => p.Id).FirstOrDefault();

                //   var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
                Bills bill = _db.Bills.Find(lastIDInBIlls.Id);
                var billCotent = _db.BillsContent.Where(s => s.Bill_ID == lastIDInBIlls.Id).ToList();

                foreach (var item in billCotent)
                {
                    item.Viewed = false;
                    _db.Entry(item).State = EntityState.Modified;
                }


                decimal price = 0;
                decimal cost = 0;
                var model = _db.BillsContent.Where(s => s.Bill_ID == lastIDInBIlls.Id && s.IsDeleted == false).ToList();

                foreach (var item in model)
                {
                    price += item.Quantity * item.Price;
                }

                foreach (var item in model)
                {
                    cost += item.Quantity * item.Cost;
                }




                bill.price = price;
                bill.cost = cost;
                bill.discount = discount ?? default(decimal);
                bill.Viewed = false;
                _db.Entry(bill).State = EntityState.Modified;

                _db.SaveChanges();


                //payment

                payments.client_id = int.Parse(Session["CLientID"].ToString());
                payments.user_id = userID;
                payments.date = DateTime.Now;
                payments.fatoraID = fatoraID;
                payments.Payment_amount = elmodfoa ?? default(decimal);
                _db.Payments.Add(payments);
                _db.SaveChanges();
                Session["flag"] = "true";



                //   print
                Stream stream;
                ReportDocument rd = new ReportDocument();


                rd.Load(Path.Combine(Server.MapPath("~/Report/BillPruches.rpt")));
                rd.SetDataSource(_db.BillsContent.Where(d => d.Bill_ID == lastIDInBIlls.Id && d.IsDeleted == false).Select(p => new
                {
                    id = p.Bill_ID,

                    User_ID = _db.Users.Where(f => f.Id == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.User_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    Client_ID = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    phone = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.phone).FirstOrDefault(),
                    date = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.date).FirstOrDefault(),

                    Discount = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),
                    Expr19 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault(),
                    Expr14 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault() - _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),

                    Price = p.Price,
                    Quantity = p.Quantity,

                    Product_ID = _db.Products.Where(d => d.Pro_id == p.Product_ID).Select(f => f.name).FirstOrDefault(),
                    //info
                    Viewed = _db.PLaceInfo.Select(f => f.Img).FirstOrDefault(),
                    active = _db.PLaceInfo.Select(f => f.PlaceName).FirstOrDefault(),
                    status = _db.PLaceInfo.Select(f => f.Number).FirstOrDefault(),

                }).ToList());


                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                rd.PrintOptions.PrinterName = printerName;
                rd.PrintToPrinter(1, false, 0, 0);
                rd.Refresh();

                //  stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                //stream.Seek(0, SeekOrigin.Begin);
                //return File(stream, "aaplication/pdf", "تفاصيل المستند.pdf");
                return RedirectToAction("Purchases");
            }

            return RedirectToAction("HavntAccess", "Employee");

        }
        #endregion
        //public ActionResult ss()
        //{

        //    int userID = int.Parse(Session["userID"].ToString());

        //    var mo = _db.Bills.Where(p => p.User_ID == userID).ToList();

        //    var lastIDInBIlls = mo.OrderByDescending(p => p.Id).FirstOrDefault();

        //    ReportDocument rd = new ReportDocument();


        //    rd.Load(Path.Combine(Server.MapPath("~/Report/BillPruches.rpt")));
        //    rd.SetDataSource(_db.BillsContent.Where(d => d.Bill_ID == lastIDInBIlls.Id && d.IsDeleted == false).Select(p => new
        //    {
        //        id = p.Bill_ID,

        //        User_ID = _db.Users.Where(f => f.Id == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.User_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
        //        Client_ID = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
        //        phone = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.phone).FirstOrDefault(),
        //        date = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.date).FirstOrDefault(),

        //        Discount = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),
        //        Expr19 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault(),
        //        Expr14 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault() - _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),

        //        Price = p.Price,
        //        Quantity = p.Quantity,

        //        Product_ID = _db.Products.Where(d => d.Pro_id == p.Product_ID).Select(f => f.name).FirstOrDefault(),
        //        //info
        //        Viewed = _db.PLaceInfo.Select(f => f.Img).FirstOrDefault(),
        //        active = _db.PLaceInfo.Select(f => f.PlaceName).FirstOrDefault(),
        //        status = _db.PLaceInfo.Select(f => f.Number).FirstOrDefault(),

        //    }).ToList());

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return File(stream, "aaplication/pdf", "فاتوره بيع.pdf");

        //}

        #region Delete record from data table

        public JsonResult DeleteContentRecord(int BillsContent_ID, Produt_Price _proPrice)
        {
            bool result = false;
            var Stu = _db.BillsContent.SingleOrDefault(x => x.BillsContent_ID == BillsContent_ID);
            var model = _db.Produt_Price.Where(p => p.Pro_ID == Stu.Product_ID).FirstOrDefault();
            if (Stu != null)
            {
                Stu.IsDeleted = true;
                _db.SaveChanges();

                model.Quantity = model.Quantity + Stu.Quantity;

                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region return last id in fatora 

        public JsonResult returnLastid()
        {

            int userID = int.Parse(Session["userID"].ToString());

            var mo = _db.Bills.Where(p => p.User_ID == userID).ToList();

            var lastIDInBIlls = mo.OrderByDescending(p => p.Id).FirstOrDefault();


            //    var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
            return Json(lastIDInBIlls.Id, JsonRequestBehavior.AllowGet);


        }
        #endregion


        #region retuern All Purches

        public ActionResult ReturnPurches()
        {

            return View(_db.BillsContent.ToList());
        }
        #endregion


        #endregion


        #region BUyBiil --secure



        #region index of BuyBill

        public ActionResult BuyBill()
        {
            bool res = s.purchasebill();
            if (res == true)
            {

                var model = new BillsWithExten();
                model.prodCategoryX = _db.ProductCategory.ToList();
                model.ClientsX = _db.Clients.Where(f => f.Clients_Type_ID == 2).ToList();
                model.StorehouseX = _db.Storehouse.ToList();

                ViewBag.product = new SelectList(_db.Products.ToList(), "Pro_id", "name");
                ViewBag.productCat = new SelectList(_db.ProductCategory.ToList(), "Cate_ID", "name");

                Session["BillCategory"] = 5;
                return View(model);



            }

            return RedirectToAction("HavntAccess", "Employee");

        }
        #endregion

        #region add Moward

        public JsonResult SaveMowardtData(Clients model)
        {
            // bool result = true;

            try
            {
                model.Clients_Type_ID = 2;
                _db.Clients.Add(model);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(JsonRequestBehavior.AllowGet);
        }


        #endregion


        #region AddBillsBuyContent

        //[HttpPost]
        //public ActionResult AddBillsBuyContent(BillsContent _BillContent, Bills _bill, int pro, Produt_Price pro_price, int Quantity, string Client_ID)
        //{

        //    bool res = s.purchasebill();
        //    if (res == true)
        //    {
        //        if (Session["flag"].ToString() == "true")
        //        {
        //            _bill.date = DateTime.Now;
        //            // get client id
        //            var ClintID = _db.Clients.Where(p => p.name == Client_ID).FirstOrDefault();
        //            _bill.Client_ID = ClintID.Client_ID;
        //            _bill.Cate_Id = 5;

        //            _bill.User_ID = int.Parse(Session["userID"].ToString());
        //            _bill.Viewed = true;
        //            _db.Bills.Add(_bill);
        //            _db.SaveChanges();
        //        }

        //        var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
        //        _BillContent.Bill_ID = LastId.Id;

        //        //    _BillContent.Price = price;
        //        _BillContent.Quantity = _BillContent.Quantity;
        //        _BillContent.Product_ID = pro;
        //        _BillContent.Cost = _db.Produt_Price.Where(s => s.Pro_ID == pro).Select(p => p.cost).FirstOrDefault();
        //        _BillContent.Viewed = true;

        //        _db.BillsContent.Add(_BillContent);
        //        _db.SaveChanges();

        //        //edit quntity
        //        var proQuantity = _db.Produt_Price.Where(s => s.Pro_ID == pro).FirstOrDefault();
        //        Produt_Price model = _db.Produt_Price.Find(proQuantity.Prd_Pri_ID);

        //        model.Quantity = model.Quantity + Quantity;
        //        model.Store_Id = 1;
        //        model.Pro_ID = pro;
        //        model.Minmum = model.Minmum;
        //        model.many_price = model.many_price;
        //        model.Price = model.Price;
        //        model.cost = model.cost;


        //        _db.Entry(model).State = EntityState.Modified;
        //        _db.SaveChanges();


        //        Session["flag"] = "false";

        //        return RedirectToAction("BuyBill");
        //    }

        //    return RedirectToAction("HavntAccess", "Employee");

        //}








        [HttpPost]
        public ActionResult AddBillsBuyContent(BillsContent _BillContent, Bills _bill, string barcodeProduct, Produt_Price pro_price, int Quantity, string Client_ID)
        {

            bool res = s.purchasebill();
            if (res == true)
            {
                if (Session["flag"].ToString() == "true")
                {
                    _bill.date = DateTime.Now;
                    // get client id
                    var ClintID = _db.Clients.Where(p => p.name == Client_ID).FirstOrDefault();
                    _bill.Client_ID = ClintID.Client_ID;
                    _bill.Cate_Id = 5;

                    _bill.User_ID = int.Parse(Session["userID"].ToString());
                    _bill.Viewed = true;
                    _db.Bills.Add(_bill);
                    _db.SaveChanges();
                }

                var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
                _BillContent.Bill_ID = LastId.Id;

                //    _BillContent.Price = price;
                _BillContent.Quantity = _BillContent.Quantity;
                var prodids = _db.Products.Where(d => d.name == barcodeProduct).Select(f => f.Pro_id).FirstOrDefault();
                _BillContent.Product_ID = prodids;

                _BillContent.Cost = _db.Produt_Price.Where(s => s.Pro_ID == prodids).Select(p => p.cost).FirstOrDefault();
                _BillContent.Viewed = true;

                _db.BillsContent.Add(_BillContent);
                _db.SaveChanges();

                //edit quntity
                var proQuantity = _db.Produt_Price.Where(s => s.Pro_ID == prodids).FirstOrDefault();
                Produt_Price model = _db.Produt_Price.Find(proQuantity.Prd_Pri_ID);

                model.Quantity = model.Quantity + Quantity;
                model.Store_Id = 1;
                model.Pro_ID = prodids;
                model.Minmum = model.Minmum;
                model.many_price = model.many_price;
                model.Price = model.Price;
                model.cost = model.cost;


                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();


                Session["flag"] = "false";

                return RedirectToAction("BuyBill");
            }

            return RedirectToAction("HavntAccess", "Employee");

        }
        #endregion


        #region Delete record from data table

        public JsonResult DeleteContentRecordOfBuy(int BillsContent_ID, Produt_Price _proPrice)
        {
            bool result = false;
            var Stu = _db.BillsContent.SingleOrDefault(x => x.BillsContent_ID == BillsContent_ID);
            var model = _db.Produt_Price.Where(p => p.Pro_ID == Stu.Product_ID).FirstOrDefault();
            if (Stu != null)
            {
                Stu.IsDeleted = true;
                _db.SaveChanges();

                model.Quantity = model.Quantity - Stu.Quantity;

                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region New Buy fatora

        public ActionResult NewBuyfatora(decimal? discount, decimal? elmodfoa, Payments payments, int fatoraID)
        {
            bool res = s.purchasebill();
            if (res == true)
            {

                int userID = int.Parse(Session["userID"].ToString());
                // printer name
                string PrinterID = _db.Users.Where(d => d.Id == userID).Select(f => f.printer_name).FirstOrDefault();
                string printerName = _db.PrintType.Where(id => id.ID.ToString() == PrinterID).Select(f => f.PrinterName).FirstOrDefault();


                var mo = _db.Bills.Where(p => p.User_ID == userID).ToList();

                var lastIDInBIlls = mo.OrderByDescending(p => p.Id).FirstOrDefault();




                // var LastId = _db.Bills.OrderByDescending(u => u.Id).FirstOrDefault();
                Bills bill = _db.Bills.Find(lastIDInBIlls.Id);



                var billCotent = _db.BillsContent.Where(s => s.Bill_ID == lastIDInBIlls.Id).ToList();

                foreach (var item in billCotent)
                {
                    item.Viewed = false;
                    _db.Entry(item).State = EntityState.Modified;
                }


                decimal price = 0;
                decimal cost = 0;
                var model = _db.BillsContent.Where(s => s.Bill_ID == lastIDInBIlls.Id && s.IsDeleted == false).ToList();


                foreach (var item in model)
                {
                    price += item.Quantity * item.Price;
                }

                foreach (var item in model)
                {
                    cost += item.Quantity * item.Cost;
                }

                bill.price = price;
                bill.cost = cost;
                bill.discount = discount ?? default(decimal);
                bill.Viewed = false;
                _db.Entry(bill).State = EntityState.Modified;

                _db.SaveChanges();


                //payments
                var Onemodel = _db.Bills.Where(s => s.Id == lastIDInBIlls.Id).FirstOrDefault();

                payments.client_id = Onemodel.Client_ID;
                payments.user_id = int.Parse(Session["userID"].ToString());
                payments.date = DateTime.Now;

                payments.Payment_amount = elmodfoa ?? default(decimal);

                _db.Payments.Add(payments);
                _db.SaveChanges();

                Session["flag"] = "true";


                //print

                ReportDocument rd = new ReportDocument();


                rd.Load(Path.Combine(Server.MapPath("~/Report/BuyBill.rpt")));
                rd.SetDataSource(_db.BillsContent.Where(d => d.Bill_ID == lastIDInBIlls.Id && d.IsDeleted == false).Select(p => new
                {
                    id = p.Bill_ID,

                    UserID = _db.Users.Where(f => f.Id == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.User_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    Client_ID = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    phone = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == lastIDInBIlls.Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.phone).FirstOrDefault(),
                    date = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.date).FirstOrDefault(),

                    Discount = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),
                    Expr19 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault(),
                    Expr14 = _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.price).FirstOrDefault() - _db.Bills.Where(f => f.Id == lastIDInBIlls.Id).Select(ff => ff.discount).FirstOrDefault(),

                    Price = p.Price,
                    Quantity = p.Quantity,

                    Pro_ID = _db.Products.Where(d => d.Pro_id == p.Product_ID).Select(f => f.name).FirstOrDefault(),

                    //info
                    Viewed = _db.PLaceInfo.Select(f => f.Img).FirstOrDefault(),
                    active = _db.PLaceInfo.Select(f => f.PlaceName).FirstOrDefault(),
                    status = _db.PLaceInfo.Select(f => f.Number).FirstOrDefault(),


                }).ToList());

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                rd.PrintOptions.PrinterName = printerName;

                rd.PrintToPrinter(1, false, 0, 0);
                rd.Refresh();

                //   return File(stream, "aaplication/pdf", "فاتوره بيع.pdf");


                return RedirectToAction("BuyBill");
            }



            //    return RedirectToAction("BuyBill");


            return RedirectToAction("HavntAccess", "Employee");

        }
        #endregion


        #endregion


        #region returnBills -- secured



        #region index


        public ActionResult ReturnBill()
        {
            bool res = s.backbill();
            if (res == true)
            {
                var model = new BillsWithExten();
                model.productX = _db.Products.ToList();
                model.ClientsX = _db.Clients.ToList();
                int UserId = int.Parse(Session["userID"].ToString());
                model.billsX = _db.Bills.Where(p => p.User_ID == UserId).ToList();
                model.billCOntentX = _db.BillsContent.ToList();

                return View(model);
            }

            return RedirectToAction("HavntAccess", "Employee");


        }

        #endregion


        #region  edite record

        public JsonResult EditRetrnFatora(int BillsContent_ID)
        {
            BillsContent model = _db.BillsContent.Where(x => x.BillsContent_ID == BillsContent_ID).SingleOrDefault();
            string value = string.Empty;
            value = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        #endregion





        #region Search

        public ActionResult Search(int id)
        {
            bool res = s.backbill();
            if (res == true)
            {
                int UserId = int.Parse(Session["userID"].ToString());
                var modelX = new BillsWithExten();
                var model = _db.Bills.Where(x => x.Id == id && x.User_ID == UserId).FirstOrDefault();


                if (model != null)
                {

                    modelX.billsY = model;
                    modelX.billCOntentX = _db.BillsContent.Where(p => p.Bill_ID == id).ToList();
                    modelX.prodCategoryX = _db.ProductCategory.ToList();
                    modelX.productX = _db.Products.ToList();

                    var proInContent = _db.BillsContent.Select(p => p.Product_ID).ToList();
                    ViewBag.pro = new SelectList(_db.BillsContent.Select(p => p.Product_ID).ToList(), "Pro_id", "name");




                    return View(modelX);
                }

                return RedirectToAction("HavntAccess", "Employee");

            }
            return RedirectToAction("HavntAccess", "Employee");


        }


        #endregion

        #region saveReturnFatora


        public ActionResult saveReturnFatora(BillsContent content, string pro, Produt_Price pro_price, int Quantity, int price, int Bill_ID, Bills bill)
        {
            // bool result = true;

            int userID = int.Parse(Session["userID"].ToString());

            // printer name
            string PrinterID = _db.Users.Where(d => d.Id == userID).Select(f => f.printer_name).FirstOrDefault();
            string printerName = _db.PrintType.Where(id => id.ID.ToString() == PrinterID).Select(f => f.PrinterName).FirstOrDefault();

            content.Price = price;
            var GetProID = _db.Products.Where(i => i.name == pro).FirstOrDefault();
            content.Product_ID = GetProID.Pro_id;

            content.Quantity = -1 * Quantity;
            _db.BillsContent.Add(content);
            _db.SaveChanges();

            //edit quantity
            pro_price = _db.Produt_Price.Where(p => p.Pro_ID == GetProID.Pro_id).FirstOrDefault();
            pro_price.Quantity = pro_price.Quantity + Quantity;
            _db.Entry(pro_price).State = EntityState.Modified;
            _db.SaveChanges();

            //edit all price
            decimal res = 0;
            var model = _db.BillsContent.Where(s => s.Bill_ID == Bill_ID).ToList();

            foreach (var item in model)
            {
                res += item.Quantity * item.Price;
            }

            bill = _db.Bills.Where(p => p.Id == Bill_ID).FirstOrDefault();
            bill.price = res;
            _db.Entry(bill).State = EntityState.Modified;
            _db.SaveChanges();



            var usernameID = _db.Bills.Where(d => d.Id == Bill_ID).Select(f => f.User_ID).FirstOrDefault();
            //print

            ReportDocument rd = new ReportDocument();


            rd.Load(Path.Combine(Server.MapPath("~/Report/ReturnBill.rpt")));
            rd.SetDataSource(_db.BillsContent.Where(d => d.Bill_ID == Bill_ID).Select(p => new
            {
                id = p.Bill_ID,
                Quantity = Quantity,
                Price = price,
                User_ID = _db.Users.Where(d => d.Id == usernameID).Select(f => f.name).FirstOrDefault(),
                Pro_ID = pro,
                Expr2 = price * Quantity,


                //info
                many_price = _db.PLaceInfo.Select(f => f.Img).FirstOrDefault(),
                active = _db.PLaceInfo.Select(f => f.PlaceName).FirstOrDefault(),
                status = _db.PLaceInfo.Select(f => f.Number).FirstOrDefault(),

            }).ToList());

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);

            rd.PrintOptions.PrinterName = printerName;

            rd.PrintToPrinter(1, false, 0, 0);
            rd.Refresh();
            return RedirectToAction("ReturnBill");

            //return File(stream, "aaplication/pdf", "فاتوره بيع.pdf");

        }

        #endregion

        #region return price and quntity value By name

        public JsonResult ReturnValueByName(string name)
        {
            List<Produt_Price> result = new List<Produt_Price>();

            var model = _db.Products.Where(i => i.name == name).FirstOrDefault();
            var model2 = _db.Produt_Price.Where(i => i.Pro_ID == model.Pro_id).ToList();
            // decimal result = model.Quantity;
            foreach (var item in model2)
            {
                Produt_Price mo = new Produt_Price();
                mo.Prd_Pri_ID = item.Prd_Pri_ID;
                mo.Quantity = item.Quantity;
                mo.Minmum = item.Minmum;
                mo.cost = item.cost;
                mo.Price = item.Price;
                result.Add(mo);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #endregion





        public ActionResult PriintFatoraWithID()
        {
            bool res = s.purchasebill();
            if (res == true)
            {

                return View();
            }
            return RedirectToAction("HavntAccess", "Employee");

        }

        public ActionResult PriintID(int Id)
        {

            bool res = s.purchasebill();
            if (res == true)
            {

                int userID = int.Parse(Session["userID"].ToString());
                // printer name
                string PrinterID = _db.Users.Where(d => d.Id == userID).Select(f => f.printer_name).FirstOrDefault();
                string printerName = _db.PrintType.Where(s => s.ID.ToString() == PrinterID).Select(f => f.PrinterName).FirstOrDefault();


                var mo = _db.Bills.Where(p => p.User_ID == userID).ToList();

                //   print
                Stream stream;
                ReportDocument rd = new ReportDocument();


                rd.Load(Path.Combine(Server.MapPath("~/Report/BillPruches.rpt")));
                rd.SetDataSource(_db.BillsContent.Where(d => d.Bill_ID == Id && d.IsDeleted == false).Select(p => new
                {
                    id = p.Bill_ID,

                    User_ID = _db.Users.Where(f => f.Id == _db.Bills.Where(d => d.Id == Id).Select(ff => ff.User_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    Client_ID = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.name).FirstOrDefault(),
                    phone = _db.Clients.Where(f => f.Client_ID == _db.Bills.Where(d => d.Id == Id).Select(ff => ff.Client_ID).FirstOrDefault()).Select(f => f.phone).FirstOrDefault(),
                    date = _db.Bills.Where(f => f.Id == Id).Select(ff => ff.date).FirstOrDefault(),

                    Discount = _db.Bills.Where(f => f.Id == Id).Select(ff => ff.discount).FirstOrDefault(),
                    Expr19 = _db.Bills.Where(f => f.Id == Id).Select(ff => ff.price).FirstOrDefault(),
                    Expr14 = _db.Bills.Where(f => f.Id == Id).Select(ff => ff.price).FirstOrDefault() - _db.Bills.Where(f => f.Id == Id).Select(ff => ff.discount).FirstOrDefault(),

                    Price = p.Price,
                    Quantity = p.Quantity,

                    Product_ID = _db.Products.Where(d => d.Pro_id == p.Product_ID).Select(f => f.name).FirstOrDefault(),
                    //info
                    Viewed = _db.PLaceInfo.Select(f => f.Img).FirstOrDefault(),
                    active = _db.PLaceInfo.Select(f => f.PlaceName).FirstOrDefault(),
                    status = _db.PLaceInfo.Select(f => f.Number).FirstOrDefault(),

                }).ToList());


                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                rd.PrintOptions.PrinterName = printerName;
                rd.PrintToPrinter(1, false, 0, 0);
                rd.Refresh();

                //stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                //stream.Seek(0, SeekOrigin.Begin);
                //return File(stream, "aaplication/pdf", "تفاصيل المستند.pdf");
                return RedirectToAction("PriintFatoraWithID");
            }
            return RedirectToAction("HavntAccess", "Employee");

        }
    }

}