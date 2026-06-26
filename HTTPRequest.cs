using AnalyticsLibrary2;
using JsonFileConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;

namespace BioDoseUI
{
    public class HTTPRequest
    {
        public static async Task<string> TestCall()
        {
            string urlBase = JsonConfig.ReadSetting<string>("urlBase");

            string userName = JsonConfig.ReadSetting<string>("userName");

            string password = JsonConfig.ReadSetting<string>("password");

            string url = "https://uhroappwebspr1.umhs.med.umich.edu:8094/?ShowConfig=true";

            // Convert user ID and password to a base64 encoded string
            string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));

            using (HttpClient client = new HttpClient())
            {
                // Set up the HTTP headers with basic authentication
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                try
                {
                    // Make the HTTPS call
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Ensure we receive a successful response.
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string.
                    string content = await response.Content.ReadAsStringAsync();

                    // Output the response content to the console.
                    Console.WriteLine(content);

                    return content;
                }
                catch (HttpRequestException e)
                {
                    // Handle any errors that occur during the call
                    Console.WriteLine($"Request exception: {e.Message}");

                    return "Error";
                }
            }
        }



        public static async Task<string> GetRequest(string url, string userName, string password)
        { 
        
            // User ID and Password

            //string urlBase = JsonConfig.ReadSetting<string>("urlBase");

            if (JsonConfig.ReadSetting<bool>("useIndividualLogin") == false)
            {
                userName = JsonConfig.ReadSetting<string>("userName");

                password = JsonConfig.ReadSetting<string>("password");

                Log3_static.Information($"Use pre-set User name: [{userName}] with password xxxxxx");

            }

            // Convert user ID and password to a base64 encoded string
            string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));

            using (HttpClient client = new HttpClient())
            {
                // Set up the HTTP headers with basic authentication
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                try
                {
                    Log3_static.Information($"send request ...");

                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    Log3_static.Information($"read response ...");

                    string content = await response.Content.ReadAsStringAsync();

                    Log3_static.Information($"Result: {response.StatusCode} - {content}");

                    if(response.IsSuccessStatusCode == false)
                    {
                        return $"{Error_Prefix}: {response.StatusCode} - {content}";
                    }

                    return content;
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(e.ToString(), "Error - EQD2Gy");

                    Log3_static.Error($"{Error_Prefix}: {e.ToString()}");

                    return $"{Error_Prefix}: {e.Message}";
                }
            }
        }

        public const string Error_Prefix = "HTTP Error: ";


    }

}


