using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.Script.Serialization; 

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

public partial class FacebookCallback : System.Web.UI.Page
{
    private string _appId = "1260594759179359";
		private string _appSecret = "8c0193eaa98f146a3334212c4d470f7e";
		private string _logFile = "/app/whatsapp_callback.log";

		protected void Page_Load(object sender, EventArgs e)
		{
			Log("=== Page_Load ejecutado ===");
			Log("Request Method: " + Request.HttpMethod);

			if (Request.HttpMethod == "POST")
			{
				string code = Request.Form["hfCode"];
				Log("POST recibido. hfCode raw: " + (code ?? "NULL"));

				if (string.IsNullOrEmpty(code))
				{
					Log("ERROR: Code not found in Form");
					return;
				}

				Log($"[Code recibido] = {code}");
				ProcesarCodigoWhatsApp(code);

				Response.Write("OK");
				Response.End();
			}
			else
			{
				Log("GET Request - Pagina inicial");
			}
		}


    private void ProcesarCodigoWhatsApp(string code)
    {
        try
        {
            Log("=== INICIANDO PROCESO ===");
            
            // PASO 1: Obtener Access Token
            string url = $"https://graph.facebook.com/v23.0/oauth/access_token?client_id={_appId}&client_secret={_appSecret}&code={code}";
            
            Log("[PASO 1] Obteniendo Access Token");
            Log("[URL Token] " + url);

            string jsonResponse = HacerPeticionGET(url);
            Log("[Response Token] " + jsonResponse);

            // Parsear access_token manualmente
            string accessToken = ExtraerValorJSON(jsonResponse, "access_token");
            
            if (string.IsNullOrEmpty(accessToken))
            {
                Log("ERROR: No se pudo extraer access_token");
                return;
            }
            
            Log("[Access Token] " + accessToken);

            // PASO 2: Obtener Business ID
            Log("[PASO 2] Obteniendo Business ID");
            url = $"https://graph.facebook.com/v23.0/me/businesses?access_token={accessToken}";
            Log("[URL Businesses] " + url);

            jsonResponse = HacerPeticionGET(url);
            Log("[Response Businesses] " + jsonResponse);

            var serializer = new JavaScriptSerializer();
            RootObjectbusiness businessid = serializer.Deserialize<RootObjectbusiness>(jsonResponse);

            if (businessid?.data == null || businessid.data.Count == 0)
            {
                Log("ERROR: No businesses found");
                return;
            }

            string business_id = businessid.data[businessid.data.Count - 1].id;
            string business_name = businessid.data[businessid.data.Count - 1].name;
            Log($"[Business ID] {business_id}");
            Log($"[Business Name] {business_name}");

            // PASO 3: Obtener WABA ID
            Log("[PASO 3] Obteniendo WABA ID");
            url = $"https://graph.facebook.com/v23.0/{business_id}/owned_whatsapp_business_accounts?access_token={accessToken}";
            Log("[URL WABA] " + url);

            jsonResponse = HacerPeticionGET(url);
            Log("[Response WABA] " + jsonResponse);

            RootObjectWebID webid = serializer.Deserialize<RootObjectWebID>(jsonResponse);

            if (webid?.data == null || webid.data.Count == 0)
            {
                Log("ERROR: No WABA found");
                return;
            }

            string wabaId = webid.data[webid.data.Count - 1].id;
            string wabaName = webid.data[webid.data.Count - 1].name;
            Log($"[WABA ID] {wabaId}");
            Log($"[WABA Name] {wabaName}");

            // PASO 4: Obtener Phone Numbers
            Log("[PASO 4] Obteniendo Phone Numbers");
            url = $"https://graph.facebook.com/v23.0/{wabaId}/phone_numbers?access_token={accessToken}";
            Log("[URL Phone Numbers] " + url);

            jsonResponse = HacerPeticionGET(url);
            Log("[Response Phone Numbers] " + jsonResponse);

            RootObjectPhoneID phoneid = serializer.Deserialize<RootObjectPhoneID>(jsonResponse);

            if (phoneid?.data == null || phoneid.data.Count == 0)
            {
                Log("ERROR: No phone numbers found");
                return;
            }

            Log("=== PHONE NUMBERS ===");
            foreach (phoneIDResponse phone in phoneid.data)
            {
                Log($"[Phone ID] {phone.id}");
                Log($"[Phone Number] {phone.display_phone_number}");
                Log($"[Verified Name] {phone.verified_name}");
                Log($"[Quality Rating] {phone.quality_rating}");
                Log($"[Platform Type] {phone.platform_type}");
                Log("---");
            }

            Log("=== PROCESO COMPLETADO EXITOSAMENTE ===");
        }
        catch (WebException webEx)
        {
            Log("[WEB ERROR] " + webEx.Message);
            if (webEx.Response != null)
            {
                using (Stream errorStream = webEx.Response.GetResponseStream())
                using (StreamReader errorReader = new StreamReader(errorStream))
                {
                    string errorResponse = errorReader.ReadToEnd();
                    Log("[ERROR Response] " + errorResponse);
                }
            }
        }
        catch (Exception ex)
        {
            Log("[ERROR] " + ex.Message);
            Log("[ERROR Stack] " + ex.StackTrace);
            Log("[ERROR Source] " + ex.Source);
        }
    }

    private string HacerPeticionGET(string url)
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

    private string ExtraerValorJSON(string json, string key)
    {
        try
        {
            string searchKey = "\"" + key + "\":\"";
            int startIndex = json.IndexOf(searchKey);
            if (startIndex == -1) return null;
            
            startIndex += searchKey.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return null;
            
            return json.Substring(startIndex, endIndex - startIndex);
        }
        catch
        {
            return null;
        }
    }

    private void Log(string message)
    {
        try
        {
            Console.WriteLine(message);
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(_logFile, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log: {ex.Message}");
        }
    }
}
