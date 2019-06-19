using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADM.WebAPI.Test.Client
{
    class WebAPIClient
    {
        //Obtained from the server earlier, APIKey MUST be stored securly and in App.Config
        static private readonly string HttpMethod = ConfigurationManager.AppSettings["HttpMethod"];
        static private readonly string BaseURL = ConfigurationManager.AppSettings["BaseURL"];
        static private string AbsolutePath = ConfigurationManager.AppSettings["AbsolutePath"];
        static private readonly string APPId = ConfigurationManager.AppSettings["APPId"];
        static private readonly string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
        static private readonly string AuthenticationScheme = ConfigurationManager.AppSettings["Scheme"];

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            Console.WriteLine("===================================================================================");
            Console.WriteLine("= HTTP Method : " + HttpMethod);
            Console.WriteLine("= Base URL : " + BaseURL);
            Console.WriteLine("= Absolute Path : " + AbsolutePath);
            Console.WriteLine("= APP Id : " + APPId);
            Console.WriteLine("= Secret Key : " + SecretKey);
            Console.WriteLine("===================================================================================");

            try { 
                validateAPICall(HttpMethod, AbsolutePath);

                //string APIKey = null;
                string RequestURL = null;
                string queryParam = null;
                string BodyContent = null;
                string APIKey = null;

                AbsolutePath = GenPathParam(HttpMethod, AbsolutePath);
                RequestURL = BaseURL + AbsolutePath;

                queryParam = GenQueryParam(HttpMethod, AbsolutePath);
                if (queryParam != null)
                {
                    Console.WriteLine();
                    RequestURL += "?" + queryParam;
                }

                Console.WriteLine("Request URL:");
                Console.WriteLine(RequestURL);
                Console.WriteLine();

                BodyContent = GenBodyContent(HttpMethod, AbsolutePath);

                if (BodyContent != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Body Content:");
                    Console.WriteLine(BodyContent);
                    Console.WriteLine();
                }

                RequestURL = System.Web.HttpUtility.UrlEncode(RequestURL.ToLower());
                APIKey = GenAPIKey(APPId, HttpMethod, RequestURL, SecretKey);
                Console.WriteLine("API Key:");
                Console.WriteLine(AuthenticationScheme + " " + APIKey);
                Console.WriteLine();

                Console.WriteLine("-- Press Enter to continue... --");
                Console.WriteLine();
                Console.ReadLine();

                ////////////// Preparing send API request ///////////////////////////////////////////////////

                Console.WriteLine("Calling the Web API...");
                Console.WriteLine();

                HttpClient client = new HttpClient() { BaseAddress = new Uri(BaseURL) };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, APIKey);
                HttpResponseMessage response = null;

                if (HttpMethod.ToLower() == "post")
                {
                    HttpContent content = new StringContent(BodyContent, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(AbsolutePath, content);
                }
                else if (HttpMethod.ToLower() == "get")
                {
                    if (queryParam != null)
                    {
                        AbsolutePath += "?" + queryParam;
                    }
                    response = await client.GetAsync(AbsolutePath);
                }
                else if (HttpMethod.ToLower() == "put")
                {
                    HttpContent content = new StringContent(BodyContent, Encoding.UTF8, "application/json");
                    response = await client.PutAsync(AbsolutePath, content);
                }
                else if (HttpMethod.ToLower() == "delete")
                {
                    response = await client.DeleteAsync(AbsolutePath);
                }

                string responseContentType = response.Content.Headers.ContentType.ToString();
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content-Type: ");
                Console.WriteLine(responseContentType);
                Console.WriteLine();
                Console.WriteLine("Response: ");
                Console.WriteLine(responseContent);
                Console.WriteLine();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("HTTP Status: {0}, Reason: {1}. ", (int)response.StatusCode, response.ReasonPhrase);
                }
                else
                {
                    Console.WriteLine("Failed to call the API. HTTP Status: {0}, Reason: {1}", (int)response.StatusCode, response.ReasonPhrase);
                }

                Console.WriteLine();
                Console.WriteLine("-- Press Enter to exit --");
                Console.ReadLine();

            }
            catch (Exception e)
            {
                while(e.InnerException != null)
                {
                    e = e.InnerException;
                }
                Console.WriteLine("\n***********Exception***********");
                Console.WriteLine("Message :{0} ", e.Message);
                Console.ReadLine();
            }
        
        }

        static string GenBodyContent(string HttpMethod, string AbsolutePath)
        {
            if (new string[] { "post", "put" }.Contains(HttpMethod.ToLower()))
            {
                // POST /api/devices/fwassignments
                if (AbsolutePath.StartsWith("/api/devices/fwassignments"))
                {
                    Console.WriteLine("--- Input Required Body Parameter ---");

                    Console.WriteLine("Firmware:");
                    string fw = Console.ReadLine();
                    while (string.IsNullOrEmpty(fw))
                    {
                        Console.WriteLine("Firmware:");
                        fw = Console.ReadLine();
                    }

                    Console.WriteLine("IMEI: (separated by COMMA ',')");
                    string IMEI = Console.ReadLine();
                    while (string.IsNullOrEmpty(IMEI))
                    {
                        Console.WriteLine("IMEI: (separated by COMMA ',')");
                        IMEI = Console.ReadLine();
                    }

                    FirmwareAssignmentModel model = new FirmwareAssignmentModel()
                    {
                        Firmware = fw,
                        IMEIs = IMEI.Split(',').ToList(),
                    };

                    return JsonConvert.SerializeObject(model);
                }

                // POST /api/devices/configassignments
                if (AbsolutePath.StartsWith("/api/devices/configassignments"))
                {
                    Console.WriteLine("--- Input Required Body Parameter ---");

                    Console.WriteLine("Configuration:");
                    string config = Console.ReadLine();
                    while (string.IsNullOrEmpty(config))
                    {
                        Console.WriteLine("Configuration:");
                        config = Console.ReadLine();
                    }

                    Console.WriteLine("IMEI: (separated by COMMA ',')");
                    string IMEI = Console.ReadLine();
                    while (string.IsNullOrEmpty(IMEI))
                    {
                        Console.WriteLine("IMEI: (separated by COMMA ',')");
                        IMEI = Console.ReadLine();
                    }

                    ConfigurationAssignmentModel model = new ConfigurationAssignmentModel()
                    {
                        Configuration = config,
                        IMEIs = IMEI.Split(',').ToList(),
                    };

                    return JsonConvert.SerializeObject(model);
                }

                // POST /api/devices/groupassignments
                if (AbsolutePath.StartsWith("/api/devices/groupassignments"))
                {
                    Console.WriteLine("--- Input Required Body Parameter ---");

                    Console.WriteLine("Group Name:");
                    string group = Console.ReadLine();
                    while (string.IsNullOrEmpty(group))
                    {
                        Console.WriteLine("Group Name:");
                        group = Console.ReadLine();
                    }

                    Console.WriteLine("IMEI: (separated by COMMA ',')");
                    string IMEI = Console.ReadLine();
                    while (string.IsNullOrEmpty(IMEI))
                    {
                        Console.WriteLine("IMEI: (separated by COMMA ',')");
                        IMEI = Console.ReadLine();
                    }

                    GroupAssignmentModel model = new GroupAssignmentModel()
                    {
                        Group = group,
                        IMEIs = IMEI.Split(',').ToList(),
                    };

                    return JsonConvert.SerializeObject(model);
                }

                // POST /api/devices/companyassignments
                if (AbsolutePath.StartsWith("/api/devices/companyassignments"))
                {
                    Console.WriteLine("--- Input Required Body Parameter ---");

                    Console.WriteLine("Company Alias Name:");
                    string company = Console.ReadLine();
                    while (string.IsNullOrEmpty(company))
                    {
                        Console.WriteLine("Company Alias Name:");
                        company = Console.ReadLine();
                    }

                    Console.WriteLine("Group Name:");
                    string group = Console.ReadLine();
                    while (string.IsNullOrEmpty(company))
                    {
                        Console.WriteLine("Group Name:");
                        group = Console.ReadLine();
                    }

                    Console.WriteLine("IMEI: (separated by COMMA ',')");
                    string IMEI = Console.ReadLine();
                    while (string.IsNullOrEmpty(IMEI))
                    {
                        Console.WriteLine("IMEI: (separated by COMMA ',')");
                        IMEI = Console.ReadLine();
                    }

                    CompanyAssignmentModel model = new CompanyAssignmentModel()
                    {
                        CompanyAliasName = company,
                        GroupName = group,
                        IMEIs = IMEI.Split(',').ToList(),
                    };

                    return JsonConvert.SerializeObject(model);
                }

                // POST /api/groups
                if (AbsolutePath.StartsWith("/api/groups"))
                {
                    Console.WriteLine("--- Input Required Body Parameter ---");

                    Console.WriteLine("Group Name:");
                    string group = Console.ReadLine();
                    while (string.IsNullOrEmpty(group))
                    {
                        Console.WriteLine("Group Name:");
                        group = Console.ReadLine();
                    }

                    Console.WriteLine("Remark:");
                    string remark = Console.ReadLine();
                    while (string.IsNullOrEmpty(remark))
                    {
                        Console.WriteLine("Remark:");
                        remark = Console.ReadLine();
                    }

                    GroupModel model = new GroupModel()
                    {
                        Name = group,
                        Remark = remark,
                    };

                    return JsonConvert.SerializeObject(model);
                }
            }

            return null;
        }

        static string GenPathParam(string HttpMethod, string AbsolutePath)
        {
            string pathParam = null;
            string pathParamName = null;
            // Refer to http://hant.ask.helplib.com/c++/post_1615063
            Regex regex = new Regex(@"(?<={)[^}]*(?=})", RegexOptions.IgnoreCase);  

            if (new string[]{ "get","put","delete"}.Contains(HttpMethod.ToLower()))
            {
                if (regex.IsMatch(AbsolutePath))
                {
                    pathParamName = regex.Match(AbsolutePath).Groups[0].ToString(); // 取得大括號{}裡面的內容

                    Console.WriteLine("--- Input Required Path Parameter Value ---");

                    Console.WriteLine(pathParamName + ":");
                    pathParam = Console.ReadLine();
                    while(string.IsNullOrEmpty(pathParam))
                    {
                        Console.WriteLine(pathParamName + ":");
                        pathParam = Console.ReadLine();
                    }
                    
                    pathParam = Uri.EscapeDataString(pathParam);

                    // Append path parameter value
                    return AbsolutePath.Replace("{", "").Replace("}", "").Replace(pathParamName, pathParam);
                }
            }

            return AbsolutePath;
        }

        static string GenQueryParam(string HttpMethod, string AbsolutePath)
        {
            string queryParam = null;

            if (HttpMethod.ToLower() == "get")
            {
                if (AbsolutePath == "/api/devices")
                {
                    Console.WriteLine("--- Input Query Parameters ---");
                    Console.WriteLine("Company Alias Name:");
                    string companyName = Console.ReadLine();
                    Console.WriteLine("IMEI: (separated by comman ',')");
                    string IMEI = Console.ReadLine();
                    Console.WriteLine("Group Name:");
                    string groupName = Console.ReadLine();
                    Console.WriteLine("Firmware Version:");
                    string fwVer = Console.ReadLine();
                    Console.WriteLine("Config Version:");
                    string configVer = Console.ReadLine();
                    Console.WriteLine("Config Status: (0=Created, 1=Pending, 3=Completed, 4=Failed, -1=All) ");
                    string configStatus = Console.ReadLine();
                    Console.WriteLine("Firmware Status: (0=Created, 1=Pending, 3=Completed, 4=Failed, -1=All)");
                    string fwStatus = Console.ReadLine();
                    Console.WriteLine("Is Online: (0=Online, 1=Timeout, -1=All) ");
                    string online = Console.ReadLine();
                    Console.WriteLine("Use Status: (0=New, 1=In Use, 3=Suspended, 5=Recalled, -1=All) ");
                    string use = Console.ReadLine();
                    Console.WriteLine("Model Name:");
                    string model = Console.ReadLine();

                    if (!string.IsNullOrEmpty(companyName))
                    {
                        queryParam += "conditions.companyName=" + Uri.EscapeDataString(companyName);
                    }
                    if (!string.IsNullOrEmpty(IMEI))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.iMEI=" + Uri.EscapeDataString(IMEI);
                    }
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.groupName=" + Uri.EscapeDataString(groupName);
                    }
                    if (!string.IsNullOrEmpty(fwVer))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.firmwareVersion=" + Uri.EscapeDataString(fwVer);
                    }
                    if (!string.IsNullOrEmpty(configVer))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.configVersion=" + Uri.EscapeDataString(configVer);
                    }
                    if (!string.IsNullOrEmpty(configStatus))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.configStatus=" + Uri.EscapeDataString(configStatus);
                    }
                    if (!string.IsNullOrEmpty(fwStatus))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.firmwareStatus=" + Uri.EscapeDataString(fwStatus);
                    }
                    if (!string.IsNullOrEmpty(online))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.isOnline=" + Uri.EscapeDataString(online);
                    }
                    if (!string.IsNullOrEmpty(use))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.useStatus=" + Uri.EscapeDataString(use);
                    }
                    if (!string.IsNullOrEmpty(model))
                    {
                        queryParam += (queryParam != null ? "&" : "") + "conditions.modelName=" + Uri.EscapeDataString(model);
                    }
                }
            }

            return queryParam;
        }

        static string GenAPIKey(string APPId, string HttpMethod, string RequestURL, string SecretKey)
        {
            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime nowTime = DateTime.UtcNow;
            TimeSpan timeSpan = nowTime - epochStart;
            string RequestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            Console.WriteLine("UTC Time Now: " + nowTime);
            Console.WriteLine();
            
            //create random nonce for each request
            string Nonce = Guid.NewGuid().ToString("N");

            //Creating the raw signature string
            string RawData = String.Format("{0}{1}{2}{3}{4}",
                APPId, HttpMethod, RequestURL, RequestTimeStamp, Nonce);

            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(SecretKey);

            byte[] RawDataBytes = Encoding.UTF8.GetBytes(RawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(RawDataBytes);

                string signature = Convert.ToBase64String(signatureBytes);

                return string.Format("{0}:{1}:{2}:{3}", APPId, signature, Nonce, RequestTimeStamp);
            }

        }

        static void validateAPICall(string HttpMethod, string AbsolutePath)
        {
            Exception ex = new Exception("Invalid API call. Please verify HTTP-Method and Absolute-Path.");

            if (HttpMethod.ToLower().Equals( "get" )) {
                if (!new string[]{
                        "/api/devices",
                        "/api/devices/{IMEI}",
                        "/api/groups",
                        "/api/groups/{id}"}.Contains(AbsolutePath )) {
                    throw ex;
                }
            }

            if (HttpMethod.ToLower().Equals( "post" )) {
                if (!new string[]{
                        "/api/devices/fwassignments",
                        "/api/devices/configassignments",
                        "/api/devices/groupassignments",
                        "/api/devices/companyassignments",
                        "/api/groups"}.Contains(AbsolutePath )) {
                    throw ex;
                }
            }

            if (HttpMethod.ToLower().Equals( "delete" )) {
                if (!new string[]{
                        "/api/groups/{id}"}.Contains(AbsolutePath )) {
                    throw ex;
                }
            }
        }

        public class GroupModel
        {
            [Required]
            [MaxLength(40)]
            public string Name { get; set; }

            public string Remark { get; set; }
        }

        public class FirmwareAssignmentModel
        {
            [Required]
            public string Firmware { get; set; }
            [Required]
            public List<string> IMEIs { get; set; }
        }

        public class ConfigurationAssignmentModel
        {
            [Required]
            public string Configuration { get; set; }
            [Required]
            public List<string> IMEIs { get; set; }
        }

        public class GroupAssignmentModel
        {
            [Required]
            public string Group { get; set; }
            [Required]
            public List<string> IMEIs { get; set; }
        }

        public class CompanyAssignmentModel
        {
            [Required]
            public string CompanyAliasName { get; set; }
            [Required]
            public string GroupName { get; set; }
            [Required]
            public List<string> IMEIs { get; set; }
        }

    }
}
