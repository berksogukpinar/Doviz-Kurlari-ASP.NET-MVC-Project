using DovizKurlari2.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace DovizKurlari2.Controllers
{
    public class HomeController : Controller
    {
        #region [DÖVİZ Kurları Ana Sayfa]

        /// <summary>
        /// <param>name="submit"</param>
        /// <param>name="model"</param>
        /// Döviz Kurları Tabloları Sayfası
        /// SaveSixMonthData bir kere çalıştırılarak TCMB'deki 6 Aylık Dolar,Euro ve Sterlin verilerini veritabanına kaydetmeye yarar.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {           
            // İlk defa projeyi açacaksanız alttaki yorum kodunu kaldırın
            //SaveSixMonthData();
            return View();
        }

        #endregion

        #region [Veritabanına Altı Aylık Veriyi Çekme]
        /// <summary>
        /// <param>name="submit"</param>
        /// <param>name="model"</param>
        /// Döviz Kurları 6 Aylık Verinin TCMB'den çekilmesi ve veritabanına kaydedilmesi.
        /// <param>name="submit"</param>
        /// <param>name="model"</param>
        /// </summary>
        /// <returns></returns>
        public void SaveSixMonthData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                for (int i = 180; i > 0; i--)
                {
                    DateTime CurrentDate = DateTime.Now;
                    Doviz doviz = new Doviz();
                    CurrentDate = CurrentDate.AddDays(-i);
                    var NewDate = CurrentDate.ToString("ddMM");
                    var Month = CurrentDate.ToString("MM");
                    var MyUrl = $"https://www.tcmb.gov.tr/kurlar/2021{Month}/{NewDate}2021.xml"; // Veriyi dinamik olarak çekmek için url.
                    
                    try
                    {
                        XDocument Doc = XDocument.Load(MyUrl);
                        var tarihYediGun = Doc.Descendants().Where(x => x.Name == "Tarih_Date").Select(x => x.Attribute("Tarih"));//Tarih verisinin çekilmesi.
                        var tarihYediGunDeger = tarihYediGun.ToArray()[0].Value;
                        var Dox = Doc.Descendants()
                        .Where(r => r.Name == "Currency")
                        .Select(r => new
                        {
                            Isim = r.Element("Isim").Value,
                            Kod = r.Attribute("Kod").Value,
                            AlisKur = r.Element("BanknoteBuying").Value,
                            SatisKur = r.Element("BanknoteSelling").Value,
                            Tarih = tarihYediGunDeger
                        });

                        doviz.Isim = Dox.ToArray()[4].Isim;
                        doviz.Kod = Dox.ToArray()[4].Kod;
                        doviz.AlisKur = Dox.ToArray()[4].AlisKur;
                        doviz.SatisKur = Dox.ToArray()[4].SatisKur;
                        doviz.Tarih = Dox.ToArray()[4].Tarih;
                        db.Doviz.Add(doviz); // Çekilen veriler veritabanına kaydedilir.
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        #endregion

        #region [Günün Verilerini Dinamik Olarak Çekme]
        /// <summary>
        /// Ana Sayfa ilk açıldığında 7 günlük verilerin grafiklerde gözükmesi için hazırlanan fonksiyon.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetData()
        {
            XDocument Doc = XDocument.Load("https://www.tcmb.gov.tr/kurlar/today.xml");
            var TarihYediGun = Doc.Descendants().Where(x => x.Name == "Tarih_Date").Select(x => x.Attribute("Tarih"));
            var TarihYediGunDeger = TarihYediGun.ToArray()[0].Value;
            var Dox = Doc.Descendants()
                .Where(r => r.Name == "Currency")
                .Select(r => new
                {
                    Isim = r.Element("Isim").Value,
                    Kod = r.Attribute("Kod").Value,
                    AlisKur = r.Element("BanknoteBuying").Value,
                    SatisKur = r.Element("BanknoteSelling").Value,
                    Tarih = TarihYediGunDeger
                });
            // Eger son kaydedilmiş verinin tarihi güncel zamandan farklı ise aşağıdaki güncelleme işlemleri yapılır.
            using (DovizEntities db = new DovizEntities())
            {
                DateTime CurrentDate2 = DateTime.Now;
                var SonVeriDolar = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").FirstOrDefault().Tarih;//Son olarak veritabanına kaydedilmiş Dolar verisini kontrol eder.
                SonVeriDolar.ToString();
                DateTime VeritabanindaYaziliSonGun = DateTime.Parse(SonVeriDolar);
                var FarkGunSayisi = (CurrentDate2 - VeritabanindaYaziliSonGun).TotalDays;//Kullanıcının uygulamayı girmediği gün sayısının hesaplanması.
                FarkGunSayisi = Convert.ToInt32(Math.Floor(FarkGunSayisi));
                if (FarkGunSayisi != 0)
                {
                    for (var i = FarkGunSayisi; i > 0; i--) // Döngü yardımıyla çekilmemiş verilerin çekilip veritabanına kaydedilmesi.
                    {
                        DateTime CurrentDate = DateTime.Now;
                        Doviz doviz = new Doviz();
                        CurrentDate = CurrentDate.AddDays(-i + 1);
                        var NewDate = CurrentDate.ToString("ddMM");
                        var Month = CurrentDate.ToString("MM");
                        var MyUrl = $"https://www.tcmb.gov.tr/kurlar/2021{Month}/{NewDate}2021.xml";
                        try
                        {
                            XDocument DocDolar = XDocument.Load(MyUrl);
                            var Tarih = DocDolar.Descendants().Where(x => x.Name == "Tarih_Date").Select(x => x.Attribute("Tarih"));
                            var TarihDeger = Tarih.ToArray()[0].Value;
                            var DoxDolar = DocDolar.Descendants()
                            .Where(r => r.Name == "Currency")
                            .Select(r => new
                            {
                                Isim = r.Element("Isim").Value,
                                Kod = r.Attribute("Kod").Value,
                                AlisKur = r.Element("BanknoteBuying").Value,
                                SatisKur = r.Element("BanknoteSelling").Value,
                                Tarih = TarihDeger
                            });
                            doviz.Isim = DoxDolar.ToArray()[0].Isim;
                            doviz.Kod = DoxDolar.ToArray()[0].Kod;
                            doviz.AlisKur = DoxDolar.ToArray()[0].AlisKur;
                            doviz.SatisKur = DoxDolar.ToArray()[0].SatisKur;
                            doviz.Tarih = DoxDolar.ToArray()[0].Tarih;
                            db.Doviz.Add(doviz);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            using (DovizEntities db = new DovizEntities())
            {
                DateTime CurrentDate2 = DateTime.Now;
                var SonVeriEuro = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").FirstOrDefault().Tarih;//Son olarak veritabanına kaydedilmiş Euro verisini kontrol eder.
                SonVeriEuro.ToString();
                DateTime VeritabanindaYaziliSonGun = DateTime.Parse(SonVeriEuro);
                var FarkGunSayisi = (CurrentDate2 - VeritabanindaYaziliSonGun).TotalDays;//Kullanıcının uygulamayı girmediği gün sayısının hesaplanması.
                FarkGunSayisi = Convert.ToInt32(Math.Floor(FarkGunSayisi));
                if (FarkGunSayisi != 0)
                {
                    for (var i = FarkGunSayisi; i > 0; i--) // Döngü yardımıyla çekilmemiş verilerin çekilip veritabanına kaydedilmesi.
                    {
                        DateTime CurrentDate = DateTime.Now;
                        Doviz doviz = new Doviz();
                        CurrentDate = CurrentDate.AddDays(-i + 1);
                        var NewDate = CurrentDate.ToString("ddMM");
                        var Month = CurrentDate.ToString("MM");
                        var MyUrl = $"https://www.tcmb.gov.tr/kurlar/2021{Month}/{NewDate}2021.xml";

                        try
                        {
                            XDocument DocEuro = XDocument.Load(MyUrl);
                            var Tarih = DocEuro.Descendants().Where(x => x.Name == "Tarih_Date").Select(x => x.Attribute("Tarih"));
                            var TarihDeger = Tarih.ToArray()[0].Value;
                            var DoxEuro = DocEuro.Descendants()
                            .Where(r => r.Name == "Currency")
                            .Select(r => new
                            {
                                Isim = r.Element("Isim").Value,
                                Kod = r.Attribute("Kod").Value,
                                AlisKur = r.Element("BanknoteBuying").Value,
                                SatisKur = r.Element("BanknoteSelling").Value,
                                Tarih = TarihDeger
                            });
                            doviz.Isim = DoxEuro.ToArray()[3].Isim;
                            doviz.Kod = DoxEuro.ToArray()[3].Kod;
                            doviz.AlisKur = DoxEuro.ToArray()[3].AlisKur;
                            doviz.SatisKur = DoxEuro.ToArray()[3].SatisKur;
                            doviz.Tarih = DoxEuro.ToArray()[3].Tarih;
                            db.Doviz.Add(doviz);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            using (DovizEntities db = new DovizEntities())
            {
                DateTime CurrentDate2 = DateTime.Now;
                var SonVeriSterlin = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").FirstOrDefault().Tarih; //Son olarak veritabanına kaydedilmiş sterlin verisini kontrol eder.
                SonVeriSterlin.ToString();
                DateTime VeritabanindaYaziliSonGun = DateTime.Parse(SonVeriSterlin);
                var farkGunSayisi = (CurrentDate2 - VeritabanindaYaziliSonGun).TotalDays;//Kullanıcının uygulamayı girmediği gün sayısının hesaplanması.
                farkGunSayisi = Convert.ToInt32(Math.Floor(farkGunSayisi));

                if (farkGunSayisi != 0)
                {
                    for (var i = farkGunSayisi; i > 0; i--) // Döngü yardımıyla çekilmemiş verilerin çekilip veritabanına kaydedilmesi.
                    {
                        DateTime CurrentDate = DateTime.Now;
                        Doviz doviz = new Doviz();
                        CurrentDate = CurrentDate.AddDays(-i + 1);
                        var NewDate = CurrentDate.ToString("ddMM");
                        var Month = CurrentDate.ToString("MM");
                        var MyUrl = $"https://www.tcmb.gov.tr/kurlar/2021{Month}/{NewDate}2021.xml";
                        try
                        {
                            XDocument DocSterlin = XDocument.Load(MyUrl);
                            var Tarih = DocSterlin.Descendants().Where(x => x.Name == "Tarih_Date").Select(x => x.Attribute("Tarih"));
                            var TarihDeger = Tarih.ToArray()[0].Value;
                            var DoxSterlin = DocSterlin.Descendants()
                            .Where(r => r.Name == "Currency")
                            .Select(r => new
                            {
                                Isim = r.Element("Isim").Value,
                                Kod = r.Attribute("Kod").Value,
                                AlisKur = r.Element("BanknoteBuying").Value,
                                SatisKur = r.Element("BanknoteSelling").Value,
                                Tarih = TarihDeger
                            });
                            doviz.Isim = DoxSterlin.ToArray()[4].Isim;
                            doviz.Kod = DoxSterlin.ToArray()[4].Kod;
                            doviz.AlisKur = DoxSterlin.ToArray()[4].AlisKur;
                            doviz.SatisKur = DoxSterlin.ToArray()[4].SatisKur;
                            doviz.Tarih = DoxSterlin.ToArray()[4].Tarih;
                            db.Doviz.Add(doviz);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            return Json(Dox, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region [Haftalık,15 Günlük, 1 Aylık, 3 Aylık ve 6 Aylık Dolar Verilerini Veritabanından Çekme]
        /// <summary>
        /// Dolar verilerinin veritabanından alınıp Index sayfasında kullanılmasını sağlayan fonksiyonlar.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WeeklyData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki dolar verilerini Id'lerine göre sıralayıp son 7 veriyi alır.
                var HaftalikDolarListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(7).ToList();                
                return Json(HaftalikDolarListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult FifteenDolarData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki dolar verilerini Id'lerine göre sıralayıp son 15 veriyi alır.
                var OnBesGunlukDolarListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(15).ToList();               
                return Json(OnBesGunlukDolarListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult MonthlyData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki dolar verilerini Id'lerine göre sıralayıp son 30 veriyi alır.
                var AylikDolarListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "USD").Take(30).ToList();               
                return Json(AylikDolarListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ThreeMonthData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki dolar verilerini Id'lerine göre sıralayıp son 3 Aylık veriyi alır.
                var UcAylikDolarListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "USD").Take(90).ToList();                
                return Json(UcAylikDolarListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SixMonthData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki dolar verilerini Id'lerine göre sıralayıp son 6 Aylık veriyi alır.
                var AltiAylikDolarListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "USD").Take(180).ToList();                
                return Json(AltiAylikDolarListe, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region [Haftalık,15 Günlük, 1 Aylık, 3 Aylık ve 6 Aylık Tüm Verileri Çekme]
        /// <summary>
        /// Veritabanında 6 Aylık olarak kaydedilmiş Dolar,Euro ve Sterlin verilerinin çekilmesi.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WeeklyAllData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var HaftalikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(7).ToList();
                var HaftalikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(7).ToList();
                var Result = HaftalikDolarTumListe.Concat(HaftalikEuroTumListe).ToList(); 
                var HaftalikSterlinTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(7).ToList();
                var ResultSon = Result.Concat(HaftalikSterlinTumListe).ToList();                
                return Json(ResultSon, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult FifteenAllData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var OnBesGunlukDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(15).ToList();
                var OnBesGunlukEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(15).ToList();
                var Result = OnBesGunlukDolarTumListe.Concat(OnBesGunlukEuroTumListe).ToList();
                var OnBesGunlukSterlinTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(15).ToList();
                var ResultSon = Result.Concat(OnBesGunlukSterlinTumListe).ToList();                
                return Json(ResultSon, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult MonthlyAllData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var AylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(30).ToList();
                var AylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(30).ToList();
                var Result = AylikDolarTumListe.Concat(AylikEuroTumListe).ToList();
                var AylikSterlinTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(30).ToList();
                var ResultSon = Result.Concat(AylikSterlinTumListe).ToList();               
                return Json(ResultSon, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ThreeMonthlyAllData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var UcAylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(90).ToList();
                var UcAylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(90).ToList();
                var Result = UcAylikDolarTumListe.Concat(UcAylikEuroTumListe).ToList();
                var UcAylikSterlinTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(90).ToList();
                var ResultSon = Result.Concat(UcAylikSterlinTumListe).ToList();                
                return Json(ResultSon, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SixMonthlyAllData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var AltiAylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(180).ToList();
                var AltiAylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(180).ToList();
                var Result = AltiAylikDolarTumListe.Concat(AltiAylikEuroTumListe).ToList();
                var AltiAylikSterlinTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(180).ToList();
                var ResultSon = Result.Concat(AltiAylikSterlinTumListe).ToList();               
                return Json(ResultSon, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region [1 Haftalık, 15 Günlük, 1 Aylık, 3 Aylık ve 6 Aylık Tüm Euro ve Dolar Verilerinin Veritabanından Çekilmesi]
        /// <summary>
        /// Euro/Dolar Grafiğinin Çizdirilmesi için verilerin birleştirilmesinin yapıldığı fonksiyonlar
        /// Euro/Dolar bölümü Index sayfasında yaptırılmıştır.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WeeklyEurUsdData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var haftalikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(7).ToList();
                var haftalikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(7).ToList();
                var Result = haftalikDolarTumListe.Concat(haftalikEuroTumListe).ToList();                
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult FifteenEurUsdData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var OnBesGunlukDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(15).ToList();
                var OnBesGunlukEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(15).ToList();
                var Result = OnBesGunlukDolarTumListe.Concat(OnBesGunlukEuroTumListe).ToList();               
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult MonthlyEurUsdData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var AylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(30).ToList();
                var AylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(30).ToList();
                var Result = AylikDolarTumListe.Concat(AylikEuroTumListe).ToList();               
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ThreeMonthlyEurUsdData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var UcAylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(90).ToList();
                var UcAylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(90).ToList();
                var Result = UcAylikDolarTumListe.Concat(UcAylikEuroTumListe).ToList();               
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SixMonthlyEurUsdData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Verilerin birleştirilip bir grafikte verilmesinin sağlanması için çekilen veriler tek bir listede birleştirilir.
                var AltiAylikDolarTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "USD").Take(180).ToList();
                var AltiAylikEuroTumListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(180).ToList();
                var Result = AltiAylikDolarTumListe.Concat(AltiAylikEuroTumListe).ToList();               
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region [1 Haftalık, 15 Günlük, 1 Aylık, 3 Aylık ve 6 Aylık Tüm Euro Verilerini Veritabanından Çekme]
        /// <summary>
        /// TCMB'den veritabanına çekilen Euro verilerinin veritabanından çekilmesini sağlayan fonksiyonlar.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WeeklyEuroData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki euro verilerini Id'lerine göre sıralayıp son 7 veriyi alır.
                var HaftalikEuroListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(7).ToList();                
                return Json(HaftalikEuroListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult FifteenEuroData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki euro verilerini Id'lerine göre sıralayıp son 15 veriyi alır.
                var OnBesGunlukEuroListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(15).ToList();                
                return Json(OnBesGunlukEuroListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult MonthlyEuroData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki euro verilerini Id'lerine göre sıralayıp son 1 Aylık veriyi alır.
                var AylikEuroListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "EUR").Take(30).ToList();               
                return Json(AylikEuroListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ThreeMonthEuroData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki euro verilerini Id'lerine göre sıralayıp son 3 Aylık veriyi alır.
                var UcAylikEuroListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(90).ToList();               
                return Json(UcAylikEuroListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SixMonthEuroData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki euro verilerini Id'lerine göre sıralayıp son 6 Aylık veriyi alır.
                var AltiAylikEuroListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "EUR").Take(180).ToList();                
                return Json(AltiAylikEuroListe, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region [1 Haftalık, 15 Günlük, 1 Aylık, 3 Aylık ve 6 Aylık Tüm Sterlin Verilerinin Veritabanından Çekilmesi]
        /// <summary>
        /// TCMB'den veritabanına çekilen Sterlin verilerinin veritabanından çekilmesini sağlayan fonksiyonlar.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WeeklySterlinData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki sterlin verilerini Id'lerine göre sıralayıp son 7 veriyi alır.
                var HaftalikSterlinListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(7).ToList();                
                return Json(HaftalikSterlinListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult FifteenSterlinData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki sterlin verilerini Id'lerine göre sıralayıp son 15 veriyi alır.
                var OnBesGunlukSterlinListe = db.Doviz.OrderByDescending(u => u.DovizId).Where(x => x.Kod == "GBP").Take(15).ToList();               
                return Json(OnBesGunlukSterlinListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult MonthlySterlinData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki sterlin verilerini Id'lerine göre sıralayıp son 1 Aylık veriyi alır.
                var AylikSterlinListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "GBP").Take(30).ToList();               
                return Json(AylikSterlinListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ThreeMonthSterlinData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki sterlin verilerini Id'lerine göre sıralayıp son 3 Aylık veriyi alır.
                var UcAylikSterlinListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "GBP").Take(90).ToList();                
                return Json(UcAylikSterlinListe, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SixMonthSterlinData()
        {
            using (DovizEntities db = new DovizEntities())
            {
                //Veritabanındaki sterlin verilerini Id'lerine göre sıralayıp son 6 Aylık veriyi alır.
                var AltiAylikSterlinListe = db.Doviz.OrderByDescending(t => t.DovizId).Where(x => x.Kod == "GBP").Take(180).ToList();                
                return Json(AltiAylikSterlinListe, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}