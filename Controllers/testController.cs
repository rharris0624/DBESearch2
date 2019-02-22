﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBESearch.Models;
using System.Data;
using System.Data.Entity;

using DBESearch.ViewModels;

namespace DBE.Controllers
{
    public class testController : Controller
    {

        private DBESearchDirectoryEntities db = new DBESearchDirectoryEntities();

        public ActionResult company(int? companyid)
        {
            //companyid = 12; 
            List<CompanyViewModel> companyVM = new List<CompanyViewModel>();

            CompanyViewModel company = new CompanyViewModel();

            var companydatalist = db.DBECompanies.Where(c => c.CompanyId == companyid).Select(d => new
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
                companyVM.Add(company);
            }

            //build me a list of the Departmental item codes for a given company and put the list in the object
            var codes = db.CompanyItemCodes.Where(c => c.CompanyID == companyid).Include("ItemCodes").Select(p => new { p.ItemCode, p.ItemCode1.Description, p.Comments }).ToList();

            List<CompanyItemCodeDesc> codelist = new List<CompanyItemCodeDesc>();

            foreach (var item in codes)
            {
                CompanyItemCodeDesc companycode = new CompanyItemCodeDesc();
                companycode.ItemCode = item.ItemCode;
                companycode.Description = item.Description;
                companycode.Comments = item.Comments;
                codelist.Add(companycode);
            }

            company.CompanyItemCodesList = codelist;

            //build me a list of the NAICS codes for a given company and put the list in the object
            List<CompanyNAICSCodeDesc> NAICSList = new List<CompanyNAICSCodeDesc>();

            var NAICSCodes = db.DBECompanies.Join(db.CompanyNAICSCodes, a => a.CompanyId, b => b.Companyid, (a, b) => new { a, b })
                .Join(db.NAICSCodes, c => c.b.NAICSCode, d => d.NAICSCode1, (c, d) => new { c, d })
                .Where(e => e.c.a.CompanyId.Equals(companyid)).Select(p => new { NAICSCode = p.d }).ToList();

            var naicsquery = (from comp in db.DBECompanies
                              join compNaics in db.CompanyNAICSCodes on comp.CompanyId equals compNaics.Companyid
                              join naics in db.NAICSCodes on compNaics.NAICSCode equals naics.NAICSCode1
                              where comp.CompanyId == companyid
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
