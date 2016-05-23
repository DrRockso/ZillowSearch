using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.XPath;
using ZillowWebApp.Models;

namespace ZillowWebApp.Controllers
{
    public class HomeController : Controller
    {

        private static string zillowUrl = ConfigurationManager.AppSettings["ZillowUrl"];
        private static string zillowUID = ConfigurationManager.AppSettings["ZillowUID"];
        private static string baseURL = zillowUrl + zillowUID;


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Initialize()
        {
            return Json(new Home());
        }

        [HttpPost]
        public JsonResult SearchAddress(Home model)
        {
            try
            {
                //clear if any errors from previous request
                if(model.Errors.Count > 0)
                    model.Errors.Clear();

                string address = string.Empty, city = string.Empty, state = string.Empty, zip = string.Empty;

                //validate there is a street
                if (string.IsNullOrEmpty(model.SearchAddress))
                    model.Errors.Add(new Home.ErrorResult() { ErrorMessage = "Address must be provided" });
                else
                    address = Server.UrlEncode(model.SearchAddress);

                //validate for citystate/zip
                if (string.IsNullOrEmpty(model.SearchZip))
                {
                    if (string.IsNullOrEmpty(model.SearchCity) || string.IsNullOrEmpty(model.SearchState))
                        model.Errors.Add(new Home.ErrorResult() { ErrorMessage = "City and State must be provided, Or the Zip Code" });
                    else
                    {
                        city = Server.UrlEncode(model.SearchCity);
                        state = Server.UrlEncode(model.SearchState);
                    }
                }
                else
                    zip = Server.UrlEncode(model.SearchZip);

                //if errors return
                if (model.Errors.Count > 0)
                    return Json(model);

                var citystatezip = string.IsNullOrEmpty(zip) ? city + "+" + state : zip;

                var url = string.Format(@"{0}&address={1}&citystatezip={2}&rentzestimate={3}", baseURL, address, citystatezip, model.SearchZestimate);

                //get response
                model = MakeRequest(url,model);
            }
            catch(Exception ex)
            {
                model.Errors.Add(new Home.ErrorResult() { ErrorMessage = "An error occured while processing your request" });
            }

            return Json(model);
        }

        /// <summary>
        /// Makes a request to zillow and sets to model
        /// </summary>
        /// <param name="url">zillow url</param>
        /// <param name="model">model to update</param>
        /// <returns>updated model</returns>
        private static Home MakeRequest(string url,Home model)
        {
            string xml;

            using (WebClient client = new WebClient())
            {
                xml = client.DownloadString(url);
            }

            XDocument doc = XDocument.Parse(xml);

            //gets the message Element
            var message = doc.Root.Descendants("message").FirstOrDefault();

            //gets the result element if successful
            var result = doc.Root.Descendants("result").FirstOrDefault();


            //checks for any eroor
            if (message.Element("code").Value != "0")
            {
                //set the error
                model.Errors.Add(new Home.ErrorResult()
                {
                    ErrorCode = message.Element("code").Value,
                    ErrorMessage = message.Element("text").Value
                });
            }
            else
            {
                //sets the result
                model.Result = new Home.SearchResult()
                {
                    ResultCity = result.XPathSelectElement("address/city").Value,
                    ResultState = result.XPathSelectElement("address/state").Value,
                    ResultStreet = result.XPathSelectElement("address/street").Value,
                    ResultZip = result.XPathSelectElement("address/zipcode").Value,
                    ResultLat = result.XPathSelectElement("address/latitude").Value,
                    ResultLong = result.XPathSelectElement("address/longitude").Value,
                    ResultZest = new Home.SearchResult.Zestimate()
                    {
                        Amount = Int32.Parse(result.XPathSelectElement("zestimate/amount").Value),
                        Updated = result.XPathSelectElement("zestimate/last-updated").Value,
                        Currency = result.XPathSelectElement("zestimate/amount").Attribute("currency").Value,
                        OneWeekChange = result.XPathSelectElement("zestimate/oneWeekChange").Attribute("deprecated").Value == "true" ? true : false,
                        Range = new Home.SearchResult.Zestimate.ValuationRange()
                        {
                            High = Int32.Parse(result.XPathSelectElement("zestimate/valuationRange/high").Value),
                            Low = Int32.Parse(result.XPathSelectElement("zestimate/valuationRange/low").Value)
                        }
                    }
                };

                //null check
                if (!string.IsNullOrEmpty(result.XPathSelectElement("zestimate/valueChange").Value))
                    model.Result.ResultZest.Changed.Amount = Int32.Parse(result.XPathSelectElement("zestimate/valueChange").Value);

                //null check
                if (result.XPathSelectElement("zestimate/valueChange").Attribute("duration") != null)
                    model.Result.ResultZest.Changed.Duration = Int32.Parse(result.XPathSelectElement("zestimate/valueChange").Attribute("duration").Value);
            }

            return model;
        }
    }
}