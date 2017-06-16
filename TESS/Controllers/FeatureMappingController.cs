﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers
{
    public class FeatureMappingController : Controller
    {
        // GET: FeatureMapping
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && m.Expired == false &&
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            ViewData.Add("Products", new ObservableCollection<FeatureService.Product>(FeatureServiceProxy.GetProductClient().GetProducts()));
            ViewData.Add("Systems", GetAllSystemNames(System.Web.HttpContext.Current.GetUser().Area));
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Feature and Article Mapping";
            return View();
        }

        public String MappingData()
        {
            int article_number = -1;
            if(int.TryParse(Request.Form["article_number"], out article_number))
            {
                List<FeatureService.Features> Mapped_Features = view_ModuleFeature.getAllFeatures(article_number);
                List<Dictionary<String, Object>> Return_List = new List<Dictionary<String, Object>>();
                foreach (FeatureService.Features feature in Mapped_Features)
                {
                    Return_List.Add(new Dictionary<String, Object>(){
                        {"Feature_id", feature.Id },
                        {"Feature", feature.Text},
                        {"Warnings", feature.Warnings}, 
                        {"Information", feature.Information}
                    });
                }
                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(Return_List) + "}";
            }
            else
            {
                return "{\"error\", \"Missing article_number parameter or feature_list parameter\"}";
            }

        }

        public JsonResult GetFeatures()
        {
            int productID = -1;
            if(int.TryParse(Request.Form["productID"], out productID))
            {
                return Json(FeatureServiceProxy.GetFeaturesClient().GetFeatures(productID));
            }
            else
            {
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing productID parameter"
                    }
                }); 
            }
        }
        public JsonResult GetFeaturesList()
        {
            int productID = -1;
            if (int.TryParse(Request.Form["productID"], out productID))
            {

                List<String> options = new List<string>()
                {
                    "Id",
                    "Text",
                    "Information"
                };
                List<Dictionary<String, object>> values = new List<Dictionary<string, object>>();
                FeatureService.Features[] features = FeatureServiceProxy.GetFeaturesClient().GetFeatures(productID);

                List<FeatureService.Features> featuresWithChildren = GetAllFeatureChildren(features.ToList());
                featuresWithChildren = featuresWithChildren.OrderBy(f => f.Id).ToList();
                foreach (FeatureService.Features feature in featuresWithChildren)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach(String option in options)
                    {
                        value.Add(option, feature.GetType().GetProperty(option).GetValue(feature, null));
                    }
                    value.Add("Relation", CreateBreadcrumb(GetRelationByParentId(new List<string>(), feature.ParentId, featuresWithChildren)));
                    values.Add(value);
                }
                return Json(new Dictionary<String, object>() {
                    {
                        "options", options
                    },
                    {
                        "values", values
                    }
                });
            }
            else
            {
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing productID parameter"
                    }
                });
            }
        }

        public JsonResult GetMappedFeaturesList()
        {
            int article_number = -1;
            if (int.TryParse(Request.Form["article_number"], out article_number))
            {
                List<Dictionary<String, object>> values = new List<Dictionary<string, object>>();
                List<String> options = new List<string>()
                {
                    "Id",
                    "Text",
                    "Information"
                };
                List<FeatureService.Features> mappedFeatures = view_ModuleFeature.getAllFeatures(article_number);


                foreach (FeatureService.Features feature in mappedFeatures)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach (String option in options)
                    {
                        value.Add(option, feature.GetType().GetProperty(option).GetValue(feature, null));
                    }
                    value.Add("Relation", CreateBreadcrumb(new List<string>()));
                    values.Add(value);
                }
                return Json(new Dictionary<String, object>() {
                    {
                        "options", options
                    },
                    {
                        "values", values
                    }
                });
            }
            else
            {
                Response.StatusCode = 400;
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing article_number parameter"
                    }
                });
            }
        }
        /* 1. Verifiera innehållet
         * 2. parsa listan till en List<int> med featureids
         * 3. skapa nya view_featuremapping för varje id
         * 4. inserta till databas
         * 5. returnera hur det gick
         */
         [HttpPost]
        public JsonResult Map(){

            int article_number = -1;
            if (int.TryParse(Request.Form["article_number"], out article_number) && Request.Form["feature_list"] != null)
            {
                List<int> maplist = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["feature_list"]).ToList();
                view_ModuleFeature moduleFeature = new view_ModuleFeature();
                // First delete all the mappings
                moduleFeature.Delete("article_number = " + article_number);
                // Insert new mappings   
                foreach (int id in maplist)                         
                {
                    moduleFeature.Feature_Id = id;
                    moduleFeature.Article_number = article_number; 
                    moduleFeature.Insert();
                }

                return Json(new Dictionary<String, String>()
                {
                    {
                        "success", "Features successfully mapped to " + article_number
                    }
                });
            }
            else
            {
                Response.StatusCode = 400;
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing article_number parameter or feature_list parameter"
                    }
                });
            }
        }

        public JsonResult GetModulesBySystem()
        {
            String system = null;
            
            if (Request.Form["system"] != null)
            {
                system = Request.Form["system"];
                List<view_Module> modules = view_Module.getAllModules()
                                            .Where(m => m.System == system)
                                            .OrderBy(m => m.Article_number)
                                            .ToList();


                return Json(modules);
            }
            else
            {
                return Json(new Dictionary<String, String>()
                {
                    {
                        "error", "No system"
                    }
                });
            }
        }

        List<FeatureService.Features> GetAllFeatureChildren(List<FeatureService.Features> features)
        {
            List<FeatureService.Features> children = new List<FeatureService.Features>();
            foreach(FeatureService.Features feature in features)
            {
                children.Add(feature);
                if (feature.Children.Length > 0)
                {
                    children = children.Concat(GetAllFeatureChildren(feature.Children.ToList()).ToList()).ToList();
                }
            }
            return children;
        }

        List<String> GetRelationByParentId(List<String> parents, int parentId, List<FeatureService.Features> list)
        {
            List<String> returnList = new List<string>();
            if(parentId == 0)
            {
                return parents;
            } else
            {
                FeatureService.Features parent = list.Where(f => f.Id == parentId).First();
                returnList.Add(parent.Text);
                returnList = returnList.Concat(parents).ToList();
                return GetRelationByParentId(returnList, parent.ParentId, list);
            }
        }

        String CreateBreadcrumb(List<String> items)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<ol class='breadcrumb'>");
            foreach(String item in items)
            {
                sb.Append("<li class='breadcrumb-item'>" + item + "</li>");
            }
            sb.Append("</ol>");
            return sb.ToString();
        }
        List<SelectListItem> GetAllSystemNames(String area)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area)
                 .DistinctBy(a => a.System)
                 .OrderBy(a => a.SortNo);
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();

            return returnList;
        }
    }
}