using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBESearch.Models;
//using AHTD.Linq;
using DBESearch.ViewModels;
using System.Data;
using System.Data.Entity;

namespace DBESearch.Controllers
{
    public class DBECompanyController : Controller
    {
        private DBESearchDirectoryEntities db = new DBESearchDirectoryEntities();
        // GET: DBECompany
        public ActionResult Index()
        {
            var zipcodes = db.DBECompanies.OrderBy(z => z.Zip).Select(z => z.Zip).Distinct();
            ViewBag.ZipCodeList = zipcodes.ToList();

            var states = db.DBECompanies.OrderBy(s => s.State).Select(s => s.State).Distinct();
            ViewBag.StateList = states.ToList();

            var naicss = db.NAICSCodes.OrderBy(z => z.NAICSCode1).Select(s =>  s.NAICSCode1 + " - " + s.Description).Distinct();
            ViewBag.NAICSList = naicss.ToList();

            var itemcodes = db.ItemCodes.OrderBy(z => z.ItemCode1).Select(s => s.ItemCode1 + " - " + s.Description).Distinct();
            ViewBag.ItemCodeList = itemcodes.ToList();

            var cities = db.DBECompanies.OrderBy(z => z.City).Select(s => s.City).Distinct();
            ViewBag.CityList = cities.ToList();

            return View();
        }

        [HttpGet]
        public JsonResult GetBusinessDescription(String term)
        {
            var t = db.ItemCodes.Where(c => c.Description.Contains(term))
                                    .Select(c => c.Description );
            var u = db.NAICSCodes.Where(c => c.Description.Contains(term))
                                    .Select(c => c.Description);
            return Json(t.Union(u).Select(i => new { label = i, data = i }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult companies(String term)
        {

            var t = db.DBECompanies.Where(c => c.CompanyName.Contains(term) || c.DBAName.Contains(term))
                                    .Select(c => new {c.CompanyId, c.CompanyName});
            return Json(t.Select(i => new { label = i.CompanyName, data = i }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult lastName(String term)
        {
            object[] lastNames = db.DBECompanies.Where(c => c.OwnersLastName.ToUpper().Contains(term.ToUpper())).Select(c => c.OwnersLastName).Distinct().ToArray();
            return Json(lastNames, JsonRequestBehavior.AllowGet);
        }

        public JsonResult firstName(String term)
        {
            var t = db.DBECompanies.Where(c => c.OwnersFirstName.Contains(term)).Select(c => c.OwnersFirstName).Distinct();
            return Json(t.Select( i => new { label = i, data = i}), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Partialform()
        {

            var results = db.NAICSCodes.ToList();
            return PartialView("~/Views/DBECompany/_NAICS.cshtml", results);
        }

        public ActionResult ItemPartialform()
        {
            var results = db.ItemCodes.ToList();
            return PartialView("~/Views/DBECompany/_ItemCodes.cshtml", results);
        }

        //need to send an array rather than individual items...ask Dick about this
        //public ActionResult SearchResults(Boolean DBE, Boolean ACDBE, Boolean SBE, Boolean MBE, String companylist, String BusinessDescription, String NAICS, String Item, String OwnersFirstName, String OwnersLastName, String City, String State, String Zip, String AreaCode, String County)
        public ActionResult SearchResults(CompanySearchViewModel searchData)
        {
            int? CompanyId2 = 1;
            int? ItemCode = 805;
            CompanyItemCode stuff = db.CompanyItemCodes.Find(CompanyId2, ItemCode);
            if (stuff != null )
                Console.WriteLine("wooHoo!");
            IQueryable<DBECompany> searchquery = db.DBECompanies;

            if (searchData != null)
            {
                if (!(searchData.DBE))
                {
                    searchquery = searchquery.Where(s => s.DBE != true);
                }
                if (!(searchData.ACDBE))
                {
                    searchquery = searchquery.Where(s => s.ACDBE != true);
                }

                if (!(searchData.MBE))
                {
                    searchquery = searchquery.Where(s => s.MBE != true);
                }
                if (!(searchData.SBE))
                {
                    searchquery = searchquery.Where(s => s.SBP != true);
                }

                if (!String.IsNullOrEmpty(searchData.Company))
                {
                    searchquery = searchquery.Where(c => c.CompanyName.Contains(searchData.Company)).Select(c => c);
                }
                if (!String.IsNullOrEmpty(searchData.BusinessDescription))
                {
                    IQueryable<DBECompany> dBECompanies = searchquery.Join(db.CompanyNAICSCodes, a => a.CompanyId, b => b.Companyid, (a, b) => new { a, b })
                                            .Join(db.NAICSCodes, c => c.b.NAICSCode, d => d.NAICSCode1, (c, d) => new { c, d })
                                            .Where(e => e.d.Description.Contains(searchData.BusinessDescription))
                                            .Select(g => g.c.a);
                    IQueryable<DBECompany> NAICSCompanies = searchquery.Join(db.CompanyItemCodes, a => a.CompanyId, b => b.CompanyID, (a, b) => new { a, b })
                                           .Join(db.ItemCodes, c => c.b.ItemCode, d => d.ItemCode1, (c, d) => new { c, d })
                                           .Where(e => e.d.Description.Contains(searchData.BusinessDescription))
                                           .Select(g => g.c.a);
                    searchquery = dBECompanies.Union(NAICSCompanies);
                }

                if (searchData.NAICS != null && searchData.NAICS.Count > 0 && !String.IsNullOrEmpty(searchData.NAICS[0]))
                {
                    //var searchCodes = searchData.NAICS.Select(p => p.Substring(0, 6)).ToList();
                    searchquery = from company in searchquery
                                  join compNAICSCode in db.CompanyNAICSCodes on company.CompanyId equals compNAICSCode.Companyid
                                  join searchCode in searchData.NAICS/*searchCodes*/ on compNAICSCode.NAICSCode equals searchCode
                                  select company;
                                  
                }

                if (searchData.ItemCode != null && searchData.ItemCode.Count > 0 && searchData.ItemCode[0] > 0)
                {
                    //var searchCodes = searchData.ItemCode.Select(p => p.Substring(0, 6)).ToList();
                    searchquery = from company in searchquery
                                  join compItemCodeCode in db.CompanyItemCodes on company.CompanyId equals compItemCodeCode.CompanyID
                                  join searchCode in searchData.ItemCode/*searchCodes*/ on compItemCodeCode.ItemCode equals searchCode
                                  select company;

                }

                if (!String.IsNullOrEmpty(searchData.OwnerFirstName))
                {
                    searchquery = searchquery.Where(c => c.OwnersFirstName.Contains(searchData.OwnerFirstName)).Select(c => c);
                }
                if (!String.IsNullOrEmpty(searchData.OwnerLastName))
                {
                    searchquery = searchquery.Where(c => c.OwnersLastName.Contains(searchData.OwnerLastName)).Select(c => c);
                }
                if (searchData.Cities != null && searchData.Cities.Count > 0 && !String.IsNullOrEmpty(searchData.Cities[0]))
                {
                    searchquery = searchquery.Join(searchData.Cities, a => a.City, b => b, (a, b) => a ).Select(c => c);
                }
                if (searchData.States != null && searchData.States.Count > 0 && !String.IsNullOrEmpty(searchData.States[0]))
                {
                    searchquery = searchquery.Join(searchData.States, a => a.State, b => b, (a, b) => a ).Select(c => c);
                }
                if (searchData.OwnerZipCodes != null && searchData.OwnerZipCodes.Count > 0 && !String.IsNullOrEmpty(searchData.OwnerZipCodes[0]))
                {
                    searchquery = searchquery.Join(searchData.OwnerZipCodes, a => a.Zip, b => b, (a, b) => a).Select(c => c);
                }
                if (!string.IsNullOrEmpty(searchData.AreaCode))
                {
                    searchquery = searchquery.Where(c => c.Phone.StartsWith(searchData.AreaCode)); //Area Code is not split out in DB Need to string length check first????
                }



                //if (!(string.IsNullOrEmpty(County))) //currently, county does not exist in the database
                //{

                //}

                //done with the querybuilding
                List<DBECompany> results = searchquery.Distinct().ToList();
                //var results = db.DBECompany.Where(c => c.CompanyName.ToUpper().Contains(companylist.ToUpper())).Select(c => c).ToArray();         
                return PartialView("~/Views/DBECompany/_SearchResults.cshtml", results);
            }
            return View();
        }

        public ActionResult Contact()
        {
            return PartialView("~/Views/DBECompany/_SearchResults.cshtml");
        }

        public JsonResult cities(String term)
        {
            //object[] cities = db.DBECompanies.Where(c => c.City.ToUpper().Contains(term.ToUpper())).Select(c => c.City).Distinct().ToArray();
            var t = db.DBECompanies.Where(c => c.City.Contains(term)).Select(c => new { c.City }).Distinct();
            return Json(t.Select(i => new { label = i.City, data = i }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DisplayAll()
        {
            ViewBag.Message = "List all DBE Companies here.";
            return View();
        }

        /// 
        /// This is the view that is used to show the company contact information, NAICS codes, and departmental codes for a given company.
        /// The company id comes from the row click of the search results on Search page.
        /// 
        public ActionResult companyDetails(int? id)
        {
            List<CompanyViewModel> companyVM = new List<CompanyViewModel>();

            CompanyViewModel company = new CompanyViewModel();

            var companydatalist = db.DBECompanies.Where(c => c.CompanyId == id).Select(d => new
            {
                CompanyName = d.CompanyName,
                DBAName = d.DBAName,
                OwnersFirstName = d.OwnersFirstName,
                OwnersLastName = d.OwnersLastName,
                CompanyAddress = d.CompanyAddress,
                City = d.City,
                State = d.State,
                zip = d.Zip,
                Email = d.Email,
                Fax = d.Fax,
                AltFax = d.AltFax,
                District = d.District,
                DBE = d.DBE,
                ACDBE = d.ACDBE,
                SBP = d.SBP,
                MBE = d.MBE,
                Certified = d.Certified,
                CertificationDate = d.CertificationDate,
                DecertificationDate = d.DecertificationDate
            }).ToList();

            foreach (var item in companydatalist)
            {
                company.CompanyName = item.CompanyName;
                company.DBAName = item.DBAName;
                company.OwnersFirstName = item.OwnersFirstName;
                company.OwnersLastName = item.OwnersLastName;
                company.CompanyAddress = item.CompanyAddress + ", " + item.City + ", " + item.State;
                company.DBE = item.DBE;
                company.ACDBE = item.ACDBE;
                company.SBP = item.SBP;
                company.MBE = item.MBE;
                company.Certified = item.Certified;
                company.CertificationDate = item.CertificationDate;
                company.DecertificationDate = item.DecertificationDate;
                companyVM.Add(company);
            }

            //build me a list of the Departmental item codes for a given company and put the list in the object
            var codes = db.CompanyItemCodes.Where(c => c.CompanyID == id).Include("ItemCode1").ToList();

            List<CompanyItemCodeDesc> codelist = new List<CompanyItemCodeDesc>();

            foreach (var item in codes)
            {
                CompanyItemCodeDesc companycode = new CompanyItemCodeDesc();
                companycode.ItemCode = item.ItemCode;
                companycode.Description = item.ItemCode1.Description;
                companycode.Comments = item.Comments;
                codelist.Add(companycode);
            }

            company.CompanyItemCodesList = codelist;

            //build me a list of the NAICS codes for a given company and put the list in the object
            List<CompanyNAICSCodeDesc> NAICSList = new List<CompanyNAICSCodeDesc>();

            //var NAICSCodes = db.DBECompanies.Join(db.CompanyNAICSCodes, a => a.CompanyId, b => b.Companyid, (a, b) => new { a, b })
            //    .Join(db.NAICSCodes, c => c.b.NAICSCode, d => d.NAICSCode1, (c, d) => new {c, d })
            //    .Where(e => e.c.a.CompanyId.Equals(id)).Select(p => new { NAICSCode = p.d.NAICSCode1 }).ToList();

            var naicsquery = (from comp in db.DBECompanies
                              join compNaics in db.CompanyNAICSCodes on comp.CompanyId equals compNaics.Companyid
                              join naics in db.NAICSCodes on compNaics.NAICSCode equals naics.NAICSCode1
                              where comp.CompanyId == id
                              select new { comp.CompanyId, naics.NAICSCode1, naics.Description }).ToList();

            foreach (var x in naicsquery)
            {
                CompanyNAICSCodeDesc naicsitem = new CompanyNAICSCodeDesc();
                naicsitem.NAICSCode = x.NAICSCode1;
                naicsitem.Description = x.Description;
                NAICSList.Add(naicsitem);
            }

            company.CompanyNAICSCodesList = NAICSList;

            return View(companyVM);
        }

    }
}

