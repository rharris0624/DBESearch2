using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBESearch.ViewModels
{
    public class CompanySearchViewModel
    {
        [Display(Name = "Disadvantaged Business Enterprise (DBE)")]
        public bool DBE { get; set; }
        [Display(Name = "Airport Concessionaire Disadvantaged Business Enterprise (ACDBE)")]
        public bool ACDBE { get; set; }
        [Display(Name = "Small Business Enterprise (SBE)")]
        public bool SBE { get; set; }
        [Display(Name = "Minority Business Enterprise (MBE)")]
        public bool MBE { get; set; }
        public List<string> CompanyDescriptions { get; set; }
        public string Company { get; set; }
        public int CompanyId { get; set; }
        [Display(Name ="Cities")]
        public List<string> Cities { get; set; }
        [Display(Name ="Business Description")]
        public string BusinessDescription { get; set; }
        [Display(Name = "Dept. Codes")]
        public List<string> DepartmentCodes { get; set; }
        [Display(Name ="First Name")]
        public string OwnerFirstName { get; set; }
        [Display(Name="Last Name")]
        public string OwnerLastName { get; set; }
        [Display(Name ="Zip Codes")]
        public List<string> OwnerZipCodes { get; set; }
        [Display(Name ="States")]
        public List<string> States { get; set; }
        [Display(Name = "County")]
        public string County { get; set; }
        [Display(Name = "Area Code")]
        public string AreaCode { get; set; }
        [Display(Name = "Item Code")]
        public List<int> ItemCode { get; set; }
        [Display(Name = "NAICS Codes")]
        public List<string> NAICS { get; set; }
    }
}