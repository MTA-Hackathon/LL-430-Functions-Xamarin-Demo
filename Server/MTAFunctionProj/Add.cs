using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Calculator.Data;
using Complaint.Data;

namespace MTA_BackOnTrackHack
{
    public static class Add
    {      
        [FunctionName("Add")]
        public static IActionResult Run(
            [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "add/num1/{num1}/num2/{num2}")]
        HttpRequest req,
            int num1,
            int num2,
            ILogger log)
            //TraceWriter log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request with {num1} and {num2}");

            var result = new AdditionResult
            {
                Result = num1 + num2,
                TimeOnServer = DateTime.Now
            };

            return new OkObjectResult(result);
        }
    }

    internal class InputData
    {
        [JsonProperty("data")]
        // The service used by this example expects an array containing
        //   one or more arrays of doubles
        internal object[,] data;
    }
    public static class GetComplaintTimeEstimate
    {      
        [FunctionName("GetComplaintTimeEstimate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for GetComplaintTimeEstimate");

            if (req.Method.ToLower() == "post")
            {
                log.LogInformation($"POST method was used to invoke the function");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Request Body: " + requestBody);
                dynamic b = JsonConvert.DeserializeObject(requestBody);
                var comp = new ComplaintDetails
                {
                    Agency = b.Agency,
                    complaintType = b.complaintType,
                    Descriptor = b.Descriptor,
                    incidentZip = b.incidentZip,
                    dayOfMonth = b.dayOfMonth,
                    dayOfWeek = b.dayOfWeek,
                    Month = b.Month,
                    avgTemp = b.avgTemp,
                    TimeOnServer = DateTime.Now
                };

                string scoringUri = "http://26c756f8-7413-4bfd-829e-296f8b139d79.eastus2.azurecontainer.io/score";
                //string authKey = "<your key or token>";

                // Set the data to be sent to the service.
                // In this case, we are sending two sets of data to be scored.
                InputData payload = new InputData();
                payload.data = new object[,] {
                    {
                        comp.Agency,
                        comp.complaintType,
                        comp.Descriptor,
                        comp.incidentZip,
                        comp.dayOfMonth,
                        comp.dayOfWeek,
                        comp.Month,
                        comp.avgTemp
                    }
                };

                // Create the HTTP client
                HttpClient client = new HttpClient();
                // Set the auth header. Only needed if the web service requires authentication.
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authKey);

                // Make the request
                try {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(scoringUri));
                    request.Content = new StringContent(JsonConvert.SerializeObject(payload));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.SendAsync(request).Result;
                    //log.LogInformation("ML Response: " +  response.Content.ReadAsStringAsync().Result.GetType());
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    log.LogInformation("Response Body: " + responseBody);
                    responseBody = responseBody.Replace("\\\"","\'");
                    log.LogInformation("New Response Body: " + responseBody);
                    responseBody = responseBody.Replace("\"","");
                    log.LogInformation("New Response Body: " + responseBody);
                    dynamic r = JsonConvert.DeserializeObject(responseBody);
                    //dynamic r = JsonConvert.DeserializeObject(json);
                    //var json = JsonConvert.DeserializeObject(responseBody);
                    //dynamic r = JObject.Parse(json);
                    comp.complaintTimeToComplete = r.result[0];
                    //log.LogInformation("Result Type: " + timeResult);
                    //comp.complaintTimeToComplete = r.result;
                    //var timeResult = r.result[0];
                    // Display the response from the web service
                    //log.LogInformation("ML Response:");
                    log.LogInformation("ML Result: " + comp.complaintTimeToComplete);

                    return new OkObjectResult(comp);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.Message);
                }

                return new BadRequestObjectResult("Error While Running");

            }
            else
            {
                log.LogInformation("Method: " + req.Method + "was used to invoke the function");
                return new BadRequestObjectResult("Only POST can be used");
            }

            /*var comp = new ComplaintDetails
            {
                Agency = "DEP",
                complaintType = "Water System",
                Descriptor = "Hydrant Running (WC3)",
                incidentZip = 11249,
                dayOfMonth = 31,
                dayOfWeek = 3,
                Month = 3,
                avgTemp = 44.78,
                TimeOnServer = DateTime.Now
            };*/
            
        }
    }
}
