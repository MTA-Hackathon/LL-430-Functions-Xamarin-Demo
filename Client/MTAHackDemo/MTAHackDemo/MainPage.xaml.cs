using Calculator.Data;
using Complaint.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MTAHackDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {

        private const string Url = "https://shm-mtahack-function.azurewebsites.net/api/getcomplainttimeestimate?code=zrtxG2qzKvEJNhbsV9cM3O3U5L51C3tnpHJdZcgaLicap0J/zcxdVg==";

        private HttpClient _client;

        private HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                }

                return _client;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            SubmitButton.Clicked += async (s, e) =>
            {
                int zipCode = 11111, DOM = 0, DOW = 0, Month = 0; 
                double tmp = 0.0; 

                var success = int.TryParse(Zip.Text, out zipCode)
                    && int.TryParse(dayOfMonth.Text, out DOM)
                    && int.TryParse(dayOfWeek.Text, out DOW)
                    && int.TryParse(MonthNum.Text, out Month)
                    && double.TryParse(avgTemp.Text, out tmp);

                if (!success)
                {
                    await DisplayAlert("Error in inputs", "You must enter Integers as indicated", "OK");
                    return;
                }

                var comp = new ComplaintDetails
                {
                    Agency = Agency.Text,
                    complaintType = complaintType.Text,
                    Descriptor = Descriptor.Text,
                    incidentZip = zipCode,
                    dayOfMonth = DOM,
                    dayOfWeek = DOW,
                    Month = Month,
                    avgTemp = tmp
                };


                Result.Text = "Please wait...";
                SubmitButton.IsEnabled = false;
                Exception error = null;

                try
                {

                    var req = new HttpRequestMessage(HttpMethod.Post, new Uri(Url));
                    req.Content = new StringContent(JsonConvert.SerializeObject(comp));
                    req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = Client.SendAsync(req).Result;

                    /*var url = Url.Replace("{num1}", number1.ToString())
                        .Replace("{num2}", number2.ToString());

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    Stream stream = req.GetRequestStream();
                    string json = "{\"name\": \"Azure\" }";
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    stream.Write(buffer, 0, buffer.Length);
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();*/



                    //var json = await Client.GetStringAsync(url);

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    var deserialized = JsonConvert.DeserializeObject<ComplaintDetails>(responseBody);

                    TimeOnServer.Text = "Server Time: " + deserialized.TimeOnServer;
                    Result.Text = "Estimated Time to Resolve Complaint:" + deserialized.complaintTimeToComplete.ToString();

                }
                catch (Exception ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    Result.Text = "Error!!";
                    await DisplayAlert("There was an error", error.Message, "OK");
                }

                SubmitButton.IsEnabled = true;
            };
        }
    }
}
