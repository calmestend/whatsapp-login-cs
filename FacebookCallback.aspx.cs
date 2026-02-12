using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;

public class businessidResponse
{
    public string id { get; set; }
    public string name { get; set; }
}

public class WebIDResponse
{
    public string id { get; set; }
    public string name { get; set; }
    public string currency { get; set; }
    public string timezone_id { get; set; }
    public string message_template_namespace { get; set; }
}

public class phoneIDResponse
{
    public string id { get; set; }
    public string verified_name { get; set; }
    public string code_verification_status { get; set; }
    public string display_phone_number { get; set; }
    public string quality_rating { get; set; }
    public string platform_type { get; set; }
    public object throughput { get; set; }
}

public class Cursors
{
    public string Before { get; set; }
    public string After { get; set; }
}

public class Paging
{
    public Cursors Cursors { get; set; }
}

public class RootObjectbusiness
{
    public List<businessidResponse> data { get; set; }
    public Paging paging { get; set; }
}

public class RootObjectWebID
{
    public List<WebIDResponse> data { get; set; }
    public Paging paging { get; set; }
}

public class RootObjectPhoneID
{
    public List<phoneIDResponse> data { get; set; }
    public Paging paging { get; set; }
}

public class AccessTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
}

public partial class FacebookCallback : Page
{
    private string _appId = "1260594759179359";
    private string _appSecret = "8c0193eaa98f146a3334212c4d470f7e";
    //private string _redirectUri = "https://sistema.smuebleria.com/FacebookCallback.aspx";
    private string _redirectUri = "https://whatsapp-login-cs-production.up.railway.app/FacebookCallback.aspx";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.HttpMethod == "POST")
        {
            string code = Request.Form["hfCode"];
            
            Console.WriteLine("code: " + (code ?? "NULL"));
            Console.WriteLine("redirect_uri: " + _redirectUri);

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("ERROR: Code not found");
                Response.StatusCode = 400;
                Response.End();
                return;
            }

            ProcesarCodigoWhatsApp(code);

            Response.Write("OK");
            Response.End();
        }
    }

    private void ProcesarCodigoWhatsApp(string code)
    {
        try
        {
            string url = $"https://graph.facebook.com/v23.0/oauth/access_token" +
                         $"?client_id={_appId}" +
                         $"&client_secret={_appSecret}" +
                         $"&code={code}" +
                         $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";

            string jsonResponse = GetRequest(url);
            Console.WriteLine("response token:" + jsonResponse);

            AccessTokenResponse tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(jsonResponse);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                Console.WriteLine("ERROR: access_token not found");
                return;
            }

            string accessToken = tokenResponse.access_token;
            Console.WriteLine("access_token: " + accessToken);

            url = $"https://graph.facebook.com/v23.0/me/businesses?access_token={accessToken}";

            jsonResponse = GetRequest(url);
            Console.WriteLine("response businesses: " + jsonResponse);

            RootObjectbusiness business = JsonConvert.DeserializeObject<RootObjectbusiness>(jsonResponse);
            if (business?.data == null || business.data.Count == 0)
            {
                Console.WriteLine("ERROR: businesses not found");
                return;
            }

            string businessId = business.data[business.data.Count - 1].id;
            string businessName = business.data[business.data.Count - 1].name;

            url = $"https://graph.facebook.com/v23.0/{businessId}/owned_whatsapp_business_accounts?access_token={accessToken}";

            jsonResponse = GetRequest(url);
            Console.WriteLine("response waba: " + jsonResponse);

            RootObjectWebID waba = JsonConvert.DeserializeObject<RootObjectWebID>(jsonResponse);
            if (waba?.data == null || waba.data.Count == 0)
            {
                Console.WriteLine("ERROR: WABA not found");
                return;
            }

            string wabaId = waba.data[waba.data.Count - 1].id;
            string wabaName = waba.data[waba.data.Count - 1].name;

            url = $"https://graph.facebook.com/v23.0/{wabaId}/phone_numbers?access_token={accessToken}";

            jsonResponse = GetRequest(url);
            Console.WriteLine("response phones: " + jsonResponse);

            RootObjectPhoneID phones = JsonConvert.DeserializeObject<RootObjectPhoneID>(jsonResponse);
            if (phones?.data == null || phones.data.Count == 0)
            {
                Console.WriteLine("ERROR: phone numbers not found");
                return;
            }
        }
        catch (WebException wex)
        {
            Console.WriteLine("[WEB EXCEPTION] " + wex.Message);
            if (wex.Response != null)
            {
                using (var sr = new StreamReader(wex.Response.GetResponseStream()))
                {
                    Console.WriteLine("[ERROR RESPONSE] " + sr.ReadToEnd());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
            Console.WriteLine("[STACK] " + ex.StackTrace);
        }
    }

    private string GetRequest(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
