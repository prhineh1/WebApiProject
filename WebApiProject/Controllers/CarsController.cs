using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;


namespace WebApiProject.Controllers
{
    public class CarsController : ApiController
    {
        private CarFinderEntities context = new CarFinderEntities();
        private HttpResponseMessage httpResponse = new HttpResponseMessage();

        private class RecallInfo
        {
            public dynamic Year { get; set; }
            public dynamic Make { get; set; }
            public dynamic Model { get; set; }
            public dynamic Summary { get; set; }
            public dynamic Consequence { get; set; }
            public dynamic Component { get; set; }
        }

        private class Image
        {
            public dynamic ContextLink { get; set; }
            public dynamic ThumbnailLink { get; set; }
            public int ThumbnailHeight { get; set; }
            public int ThumbnailWidth { get; set; }
        }

        private class Cars
        {
            public string Year { get; set; }
            public string Make { get; set; }
            public string Model { get; set; }
            public string Weight { get; set; }
            public string EnginePower { get; set; }
            public string EngineSize { get; set; }
            public string trim { get; set; }
        }
        
        /// <summary>
        /// returns all unique car makes
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetCarMakes()
        {
            var result = context.GetCarMakes();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// returns models for given year and make
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <param name="make">make of the car</param>
        /// <returns></returns>
        public IHttpActionResult GetModelsForYearAndMake(int? year, string make)
        {
            var result = context.GetModelsForYearAndMake(year, make);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// returns all unique car years
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetModelYears()
        {
            var result = context.GetModelYears();
            var json = JsonConvert.SerializeObject(result);
            if (result == null)
            {
                httpResponse = Request.CreateResponse(HttpStatusCode.NotFound);
                return httpResponse;
            }
            httpResponse = Request.CreateResponse(HttpStatusCode.OK);
            httpResponse.Content = new StringContent(json, Encoding.UTF8, "text/html");
            return httpResponse;
        }

        /// <summary>
        /// returns all Mid-engined cars
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetMidEnginedCars()
        {
            var result = context.GetMidEnginedCars();
            if (result == null)
            {
                return NotFound();
            }
            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }
            return Ok(carList);
        }

        /// <summary>
        /// returns all unique trims
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetTrimLevels()
        {
            var result = context.GetTrimLevels();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// returns car makes for a given year
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <returns></returns>
        public IHttpActionResult GetMakesForYear(int? year)
        {
            var result = context.GetMakesForYear(year);
            if (result == null)
            {
                return NotFound();
            }
            return Content(HttpStatusCode.OK, result, new JsonMediaTypeFormatter(), "application/json");
        }

        /// <summary>
        /// returns list of cars by year, make and model
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <param name="make">make of the car</param>
        /// <param name="model">model of the car</param>
        /// <returns></returns>
        public IHttpActionResult GetCarsByYearAndMakeAndModel(string year, string make, string model)
        {
            var result = context.GetCarsByYearAndMakeAndModel(year, make, model);
            if (result == null)
            {
                return NotFound();
            }

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Content(HttpStatusCode.OK, carList, new JsonMediaTypeFormatter(), "application/json");
        }

        /// <summary>
        /// Returns links to google image search results
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <param name="make">make of the car</param>
        /// <param name="model">model of the car</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetGoogleImages(string year, string make, string model)
        {
            var searchParams = year + "+" + make + "+" + model;
            var queryString = "https://www.googleapis.com/customsearch/v1?key=AIzaSyD1Ti8_nUHzamUKdqlJ6hmx3zRnUdVKpjY&cx=012019432613858739685:wgufdxmecnk&searchType=image&q=" + searchParams;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(queryString);
            if (response != null)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                var data = (JObject)JsonConvert.DeserializeObject(result);

                if (data["items"].Count() == 0)
                {
                    httpResponse = Request.CreateResponse(HttpStatusCode.OK);
                    httpResponse.Content = new StringContent("No images found for this vehicle", Encoding.UTF8, "text/html");
                }

                var imageList = new List<Image>();

                foreach (var item in data["items"])
                {
                    imageList.Add(new Image() { ContextLink = item["image"]["contextLink"].ToString(), ThumbnailLink = item["image"]["thumbnailLink"].ToString(), ThumbnailHeight = Convert.ToInt32(item["image"]["thumbnailHeight"]), ThumbnailWidth = Convert.ToInt32(item["image"]["thumbnailWidth"]) });
                }
                var json = JsonConvert.SerializeObject(imageList);
                httpResponse = Request.CreateResponse(HttpStatusCode.OK);
                httpResponse.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return httpResponse;
            }

            httpResponse = Request.CreateResponse(HttpStatusCode.NotFound);
            return httpResponse;
        }

        /// <summary>
        /// returns recall information for a car
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <param name="make">mkae of the car</param>
        /// <param name="model">model of the car</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetRecallInfo(string year, string make, string model)
        {
            var queryString = "https://one.nhtsa.gov/webapi/api/Recalls/vehicle/modelyear/" + year + "/make/" + make + "/model/" + model;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(queryString);
            if (response != null)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                var data = (JObject)JsonConvert.DeserializeObject(result);
                if (data["Results"].Count() == 0)
                {
                    this.httpResponse = Request.CreateResponse(HttpStatusCode.OK);
                    this.httpResponse.Content = new StringContent("No recall information was found for this vehicle.", Encoding.UTF8, "text/html");
                    return this.httpResponse;
                }

                var recallInfoList = new List<RecallInfo>();
                foreach (var item in data["Results"])
                {
                    recallInfoList.Add( new RecallInfo() { Consequence = item["Conequence"].ToString(), Make = item["Make"].ToString(),
                                                                         Year = item["ModelYear"].ToString(), Model = item["Model"].ToString(), Summary = item["Summary"].ToString(), Component = item["Component"].ToString()
                    });
                }
                var Json = JsonConvert.SerializeObject(recallInfoList);
                httpResponse = Request.CreateResponse(HttpStatusCode.OK);
                httpResponse.Content = new StringContent(Json, Encoding.UTF8, "application/json");
                return httpResponse;
            }
            httpResponse = Request.CreateResponse(HttpStatusCode.NotFound);
            return httpResponse;
        }

        /// <summary>
        /// returns list of cars by engine size
        /// </summary>
        /// <param name="engineSize">size of engine in cylinders</param>
        /// <returns></returns>
        public IHttpActionResult GetCarsForEngineSizeCylinders(string engineSize)
        {
            var result = context.GetCarsForEngineSizeCylinders(engineSize);

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Ok(carList);
        }

        /// <summary>
        /// returns a list of cars of engine size less than 300
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetCarsForHorsepowerLessthan300()
        {
            var result = context.GetCarsForHorsepowerLessthan300();

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Ok(carList);
        }

        /// <summary>
        /// returns a list of cars of weight less than 2000kg
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetCarsForWeightLessthan2000kg()
        {
            var result = context.GetCarsForWeightLessthan2000kg();

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Ok(carList);
        }

        /// <summary>
        /// returns a list of cars of weight less than 2000kg and engine power greater than 300
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetCarsForWeigthLessThan2000kgAndHorsepowerGreaterThan300()
        {
            var result = context.GetCarsForWeigthLessThan2000kgAndHorsepowerGreaterThan300();

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Ok(carList);
        }

        /// <summary>
        /// returns a list of cars that are SUVs
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult GetSUVs()
        {
            var result = context.GetSUVs();

            var carList = new List<Cars>();
            foreach (var item in result)
            {
                carList.Add(new Cars() { Year = item.model_year, Make = item.make_display, Model = item.model_name, Weight = item.weight_kg, EnginePower = item.engine_power_ps, EngineSize = item.engine_num_cyl, trim = item.model_trim });
            }

            return Ok(carList);
        }

        /// <summary>
        /// returns car trims by year, make and model
        /// </summary>
        /// <param name="year">year of the car</param>
        /// <param name="make">mkae of the car</param>
        /// <param name="model">model of the car</param>
        /// <returns></returns>
        public IHttpActionResult GetModelTrimForYearAndMakeAndModel(int year, string make, string model)
        {
            var result = context.ModelTrimForYearAndMakeAndModel(year, make, model);

            return Ok(result);
        }
    }
}
