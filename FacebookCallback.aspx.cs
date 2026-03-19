using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;

// using Muebleria.Componentes;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

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
    public List<businessidResponse> Data { get; set; }
    public Paging Paging { get; set; }
}

public class RootObjectWebID
{
    public List<WebIDResponse> Data { get; set; }
    public Paging Paging { get; set; }
}

public class RootObjectPhoneID
{
    public List<phoneIDResponse> Data { get; set; }
    public Paging Paging { get; set; }
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
    // private string _redirectUri = "https://sistema.smuebleria.com/FacebookCallback.aspx";
    private string _redirectUri = "https://whatsapp-login-cs-production.up.railway.app/FacebookCallback.aspx";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.HttpMethod == "POST")
        {
            string code = Request.Form["hfCode"];
            
            Console.WriteLine("code: " + (code ?? "NULL"));
            // Console.WriteLine("redirect_uri: " + _redirectUri);

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
        // Errores err = new Errores();
        //    err.GenerarErrores("Login WhatsApp", "Entro", 0);
        //    Console.WriteLine("entro");
        //    var code = Request.QueryString["code"];

            Etiquetabusinessid.Text = "";
            EtiquetaPhoneId.Text = "";
            EtiquetawabaId.Text = "";
            EtiquetaError.Visible = false;

            // Console.WriteLine("codigo: " + code.ToString());
            // err.GenerarErrores("Login WhatsApp", "codigo " + code, 0);
            if (!string.IsNullOrEmpty(code))
            {
                //var fb = new FacebookClient();
                //dynamic result = fb.Post("oauth/access_token", new
                //{
                //    client_id = _appId,
                //    client_secret = _appSecret,
                //    redirect_uri = Request.Url.AbsoluteUri.Split('?')[0],
                //    code = code
                //});

                string url = "https://graph.facebook.com/v24.0/oauth/access_token?client_id=" + _appId
                       + "&redirect_uri=" + _redirectUri + "&client_secret=" + _appSecret + "&code=" + code;
                //WebClient client = new WebClient();

                // err.GenerarErrores("Login WhatsApp", "url " + url, 0);
                // Console.WriteLine("url " + url);
                //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                //client.Headers[HttpRequestHeader.UserAgent] = "ASP.NET-App";

                //// Descargar string
                //string response = client.DownloadString(url);
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                string jsonResponse = reader.ReadToEnd();

                // err.GenerarErrores("Login WhatsApp", "respuesta " + jsonResponse, 0);



                // Deserializar el JSON a un objeto dinámico
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonResponse);

                string accessToken = jsonObject.access_token;
								Console.WriteLine("access_token " + accessToken);

                EtiquetaaccessToken.Text = accessToken;

                reader.Close();
                reader.Dispose();
                stream.Close();
                stream.Dispose();
                response.Close();
                response.Dispose();
                // err.GenerarErrores("Login WhatsApp", "accessToken " + accessToken, 0);

                //Session["FacebookAccessToken"] = accessToken;
                // Usuarios usu = new Usuarios();
                int IdEmpresa = 0;
                int IdSucursal = 0;
                //if (Application["Empresa"] != null)
                //    IdEmpresa = Convert.ToInt32(Application["Empresa"]);
                //if (Application["Sucursal"] != null)
                //    IdSucursal = Convert.ToInt32(Application["Sucursal"]);
                //if (Request.QueryString.AllKeys.Contains("Empresa"))
                //{
                //    IdEmpresa = Convert.ToInt32(Request.QueryString["Empresa"]);
                //}
                //if (Request.QueryString.AllKeys.Contains("Sucursal"))
                //{
                //    IdSucursal = Convert.ToInt32(Request.QueryString["Sucursal"]);
                //}

                // System.Data.DataSet dtUsuarios =  usu.ObtenerEmpresaConfigWhats();

                // IdEmpresa = Convert.ToInt32(dtUsuarios.Tables[0].Rows[0]["IdEmpresa"]);
                // IdSucursal = Convert.ToInt32(dtUsuarios.Tables[0].Rows[0]["IdSucursal"]);

                EtiquetaIdEmpresa.Text = IdEmpresa.ToString();
                EtiquetaIdIdSucursal.Text = IdSucursal.ToString();

                // err.GenerarErrores("Login WhatsApp", "IdEmpresa " + IdEmpresa.ToString(), 0);

                // err.GenerarErrores("Login WhatsApp", "IdSucursal " + IdSucursal.ToString(), 0);

                // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, "", accessToken, "", "", "");

                // Obtener todas las cuentas ordenadas por ID descendente (los IDs más nuevos suelen ser mayores)
                //dynamic responseCuenta = fb.Get("/me/owned_whatsapp_business_accounts?fields=id,name,timezone_id&limit=1&order=reverse_chronological");

                //string wabaId = "";

                //// La última cuenta registrada será el primer elemento
                //if (responseCuenta.data.Count > 0)
                //{
                //    dynamic lastAccount = responseCuenta.data[0];
                //    wabaId = lastAccount.id;                
                //}
           
                url = @"https://graph.facebook.com/v24.0/me/businesses?access_token=" + accessToken;

                // err.GenerarErrores("Login WhatsApp", "url 2 " + url, 0);
                WebRequest request2 = WebRequest.Create(url);
                WebResponse response2 = request2.GetResponse();

                // err.GenerarErrores("Login WhatsApp", "regreso del servidor ", 0);

                Stream stream2 = response2.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);

                string jsonResponse2 = reader2.ReadToEnd();

                // err.GenerarErrores("Login WhatsApp", "jsonResponse 2 " + jsonResponse2, 0);

                // Deserializar el JSON a un objeto dinámico
                //jsonObject = JsonConvert.DeserializeObject(jsonResponse2);
                RootObjectbusiness businessid = JsonConvert.DeserializeObject<RootObjectbusiness>(jsonResponse2);
                if (businessid.Data.Count > 0)
                {
                    string business_id = businessid.Data[businessid.Data.Count - 1].id;


                    reader2.Close();
                    reader2.Dispose();
                    stream2.Close();
                    stream2.Dispose();
                    response2.Close();
                    stream2.Dispose();
                    if (businessid.Data.Count > 1)
                    {
                        List<Combos> negocios = new List<Combos>();
                        foreach (businessidResponse number in businessid.Data)
                        {
                            negocios.Add(new Combos
                            {
                                Id = number.id,
                                Nombre = number.name
                            });
                        }

                        RadGridEmpresas.DataSource = negocios;
                        RadGridEmpresas.DataBind();

                    }
                    else
                    {
                        Etiquetabusinessid.Text = business_id;

                        url = @"https://graph.facebook.com/v24.0/" + business_id + @"/owned_whatsapp_business_accounts?access_token=" + accessToken;
                        // err.GenerarErrores("Login WhatsApp", "url 3 " + url, 0);
                        WebRequest request3 = WebRequest.Create(url);
                        WebResponse response3 = request3.GetResponse();
                        Stream stream3 = response3.GetResponseStream();
                        StreamReader reader3 = new StreamReader(stream3);

                        jsonResponse = reader3.ReadToEnd();

                        // err.GenerarErrores("Login WhatsApp", "jsonResponse 3 " + jsonResponse, 0);

                        // Deserializar el JSON a un objeto dinámico
                        //jsonObject = JsonConvert.DeserializeObject(jsonResponse);
                        RootObjectWebID webid = JsonConvert.DeserializeObject<RootObjectWebID>(jsonResponse);
												Console.WriteLine("webid: " + webid.Data);

                        if (webid.Data.Count > 0)
                        {
                            string wabaId = webid.Data[webid.Data.Count - 1].id;



                            reader3.Close();
                            reader3.Dispose();
                            stream3.Close();
                            stream3.Dispose();
                            response3.Close();
                            stream3.Dispose();

                            if (businessid.Data.Count > 1)
                            {
                                List<Combos> negocios = new List<Combos>();
                                foreach (businessidResponse number in businessid.Data)
                                {
                                    negocios.Add(new Combos
                                    {
                                        Id = number.id,
                                        Nombre = number.name
                                    });
                                }

                                RadGridEmpresas.DataSource = negocios;
                                RadGridEmpresas.DataBind();

                            }
                            else
                            {
                                EtiquetawabaId.Text = wabaId;

                                // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, wabaId, accessToken, "", Etiquetabusinessid.Text, "");

                                url = @"https://graph.facebook.com/v24.0/" + wabaId + @"/phone_numbers?access_token=" + accessToken;
                                // err.GenerarErrores("Login WhatsApp", "url 4 " + url, 0);

                                WebRequest request4 = WebRequest.Create(url);
                                WebResponse response4 = request3.GetResponse();
                                Stream stream4 = response3.GetResponseStream();
                                StreamReader reader4 = new StreamReader(stream4);

                                jsonResponse = reader4.ReadToEnd();

                                reader4.Close();
                                reader4.Dispose();
                                stream4.Close();
                                stream4.Dispose();
                                response4.Close();
                                stream4.Dispose();

                                // err.GenerarErrores("Login WhatsApp", "jsonResponse 4 " + jsonResponse, 0);
                                // Deserializar el JSON a un objeto dinámico
                                //jsonObject = JsonConvert.DeserializeObject(jsonResponse);
                                RootObjectPhoneID phoneid = JsonConvert.DeserializeObject<RootObjectPhoneID>(jsonResponse);
																Console.WriteLine("phoneid: " + phoneid.Data );

                                var phoneNumbers = new List<Combos>();

                                foreach (phoneIDResponse number in phoneid.Data)
                                {
                                    phoneNumbers.Add(new Combos
                                    {
                                        Id = number.id,
                                        Nombre = number.display_phone_number
                                    });
                                }
                                string PhoneId = "";
                                string Phone = "";
                                if (phoneNumbers.Count > 1)
                                {
                                    RadGridEmpresas.DataSource = phoneNumbers;
                                    RadGridEmpresas.DataBind();

                                }
                                else if (phoneNumbers.Count == 0)
                                {
                                    EtiquetaError.Text = "La cuenta no tiene un telefono asociado";
                                    EtiquetaError.Visible = true;

                                }
                                else
                                {
                                    PhoneId = phoneNumbers[0].Id;

                                    EtiquetaPhoneId.Text = PhoneId;
                                    Phone = phoneNumbers[0].Nombre;
                                    EtiquetaPhone.Text = Phone;


                                    // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, wabaId, accessToken, PhoneId, Etiquetabusinessid.Text, Phone);

                                    // var formFields = new Dictionary<string, string>
                                    // {
                                    //     {"phone_id", PhoneId},
                                    //     {"wba_id", wabaId},
                                    //     {"app_id", _appId},
                                    //     {"token", accessToken}
                                    // };

                                    // string apiUrl = "http://192.168.103.12:8080/api/v1/templates/create";

                                    // Registrar la tarea asíncrona
                                    // var capturedFields1 = formFields;
                                    // var capturedApiUrl1 = apiUrl;
                                    // RegisterAsyncTask(new System.Web.UI.PageAsyncTask(
                                    //     (sender2, e2, cb, extraData) => ExportToPDFWhatsApp(capturedApiUrl1, "Cotizacion.pdf", capturedFields1).ContinueWith(t => { cb(t.AsyncState); }),
                                    //     (ar) => { Response.Redirect("http://sistema.smuebleria.com"); },
                                    //     (ar) => { },
                                    //     null));
                                    // ExecuteRegisteredAsyncTasks();
                                }
                            }
                        }
                        else
                        {
                            EtiquetaError.Text = "La cuenta no tiene un webid";
                            EtiquetaError.Visible = true;
                        }
                    }
                }
                else 
                {
                    EtiquetaError.Text = "La cuenta no tiene un negocio";
                    EtiquetaError.Visible = true;
                }
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

    protected void RadGridEmpresas_ItemCommand(object source, GridViewCommandEventArgs e)
    {
        if (Etiquetabusinessid.Text == "")
        {
            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = RadGridEmpresas.Rows[rowIndex];
            string business_id = row.Cells[1].Text;

            Etiquetabusinessid.Text = business_id;

            string url = @"https://graph.facebook.com/v24.0/" + business_id + @"/owned_whatsapp_business_accounts?access_token=" + EtiquetaaccessToken.Text;
            // Errores err = new Errores();
            // err.GenerarErrores("Login WhatsApp", "url 3 " + url, 0);
            WebRequest request3 = WebRequest.Create(url);
            WebResponse response3 = request3.GetResponse();
            Stream stream3 = response3.GetResponseStream();
            StreamReader reader3 = new StreamReader(stream3);

            string jsonResponse = reader3.ReadToEnd();

            // err.GenerarErrores("Login WhatsApp", "jsonResponse 3 " + jsonResponse, 0);

            // Deserializar el JSON a un objeto dinámico
            //jsonObject = JsonConvert.DeserializeObject(jsonResponse);
            RootObjectWebID webid = JsonConvert.DeserializeObject<RootObjectWebID>(jsonResponse);

            EtiquetaError.Visible = false;

            if (webid.Data.Count == 0)
            {
                EtiquetaError.Text = "La cuenta no tiene un webid";
                EtiquetaError.Visible = true;
                return;
            }
            if (webid.Data.Count == 1)
            {
                string wabaId = webid.Data[webid.Data.Count - 1].id;
                EtiquetawabaId.Text = wabaId;

                // Usuarios usu = new Usuarios();
                int IdEmpresa = 0;
                int IdSucursal = 0;
                //if (Application["Empresa"] != null)
                //    IdEmpresa = Convert.ToInt32(Application["Empresa"]);
                //if (Application["Sucursal"] != null)
                //    IdSucursal = Convert.ToInt32(Application["Sucursal"]);

                IdEmpresa = Convert.ToInt32(EtiquetaIdEmpresa.Text);
                IdSucursal = Convert.ToInt32(EtiquetaIdIdSucursal.Text);


                // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, wabaId, EtiquetaaccessToken.Text, "", Etiquetabusinessid.Text, "");

                reader3.Close();
                reader3.Dispose();
                stream3.Close();
                stream3.Dispose();
                response3.Close();
                stream3.Dispose();

                url = @"https://graph.facebook.com/v24.0/" + wabaId + @"/phone_numbers?access_token=" + EtiquetaaccessToken.Text;
                // err.GenerarErrores("Login WhatsApp", "url 4 " + url, 0);

                WebRequest request4 = WebRequest.Create(url);
                WebResponse response4 = request4.GetResponse();
                Stream stream4 = response4.GetResponseStream();
                StreamReader reader4 = new StreamReader(stream4);

                jsonResponse = reader4.ReadToEnd();

                reader4.Close();
                reader4.Dispose();
                stream4.Close();
                stream4.Dispose();
                response4.Close();
                response4.Dispose();

                // err.GenerarErrores("Login WhatsApp", "jsonResponse 4 " + jsonResponse, 0);
                // Deserializar el JSON a un objeto dinámico
                //jsonObject = JsonConvert.DeserializeObject(jsonResponse);
                RootObjectPhoneID phoneid = JsonConvert.DeserializeObject<RootObjectPhoneID>(jsonResponse);

                var phoneNumbers = new List<Combos>();

                foreach (phoneIDResponse number in phoneid.Data)
                {
                    phoneNumbers.Add(new Combos
                    {
                        Id = number.id,
                        Nombre = number.display_phone_number
                    });
                }
                string PhoneId = "";

                EtiquetaError.Visible = false;

                if (phoneNumbers.Count == 0)
                {
                    EtiquetaError.Text = "La cuenta no tiene un telefono asociado";

                    EtiquetaError.Visible = true;
                    return;
                }

                string Phone = "";


                if (phoneNumbers.Count > 1)
                {
                    reader4.Close();
                    reader4.Dispose();
                    stream4.Close();
                    stream4.Dispose();
                    response4.Close();
                    response4.Dispose();

                    RadGridEmpresas.DataSource = phoneNumbers;
                    RadGridEmpresas.DataBind();
                }
                else
                {
                    PhoneId = phoneNumbers[0].Id;
                    EtiquetaPhoneId.Text = PhoneId;

                    Phone = phoneNumbers[0].Nombre;
                    EtiquetaPhone.Text = Phone;


                    // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, EtiquetawabaId.Text, EtiquetaaccessToken.Text, PhoneId, Etiquetabusinessid.Text, Phone);

                    // var formFields = new Dictionary<string, string>
                    // {
                    //     {"phone_id", PhoneId},
                    //     {"wba_id", wabaId},
                    //     {"app_id", _appId},
                    //     {"token", EtiquetaaccessToken.Text}
                    // };

                    // string apiUrl = "http://192.168.103.12:8080/api/v1/templates/create";

                    // Registrar la tarea asíncrona
                    // var capturedFields2 = formFields;
                    // var capturedApiUrl2 = apiUrl;
                    // RegisterAsyncTask(new System.Web.UI.PageAsyncTask(
                    //     (sender2, e2, cb, extraData) => ExportToPDFWhatsApp(capturedApiUrl2, "Cotizacion.pdf", capturedFields2).ContinueWith(t => { cb(t.AsyncState); }),
                    //     (ar) => { Response.Redirect("http://sistema.smuebleria.com"); },
                    //     (ar) => { },
                    //     null));

                    // ExecuteRegisteredAsyncTasks();
                }
            }
            else
            {
                reader3.Close();
                reader3.Dispose();
                stream3.Close();
                stream3.Dispose();
                response3.Close();
                response3.Dispose();



                List<Combos> negocios = new List<Combos>();
                foreach (WebIDResponse number in webid.Data)
                {
                    negocios.Add(new Combos
                    {
                        Id = number.id,
                        Nombre = number.name
                    });
                }

                RadGridEmpresas.DataSource = negocios;
                RadGridEmpresas.DataBind();
            }
        }
        else if (EtiquetawabaId.Text == "")
        {
            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = RadGridEmpresas.Rows[rowIndex];
            string wabaId = row.Cells[1].Text;
            EtiquetawabaId.Text = wabaId;

            // Usuarios usu = new Usuarios();
            int IdEmpresa = 0;
            int IdSucursal = 0;
            //if (Application["Empresa"] != null)
            //    IdEmpresa = Convert.ToInt32(Application["Empresa"]);
            //if (Application["Sucursal"] != null)
            //    IdSucursal = Convert.ToInt32(Application["Sucursal"]);

            IdEmpresa = Convert.ToInt32(EtiquetaIdEmpresa.Text);
            IdSucursal = Convert.ToInt32(EtiquetaIdIdSucursal.Text);

            // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, EtiquetawabaId.Text, EtiquetaaccessToken.Text, "", Etiquetabusinessid.Text, "");

            string url = @"https://graph.facebook.com/v24.0/" + wabaId + @"/phone_numbers?access_token=" + EtiquetaaccessToken.Text;
            // Errores err = new Errores();

            // err.GenerarErrores("Login WhatsApp", "url 4 " + url, 0);

            WebRequest request4 = WebRequest.Create(url);
            WebResponse response4 = request4.GetResponse();
            Stream stream4 = response4.GetResponseStream();
            StreamReader reader4 = new StreamReader(stream4);

            string jsonResponse = reader4.ReadToEnd();

            reader4.Close();
            reader4.Dispose();
            stream4.Close();
            stream4.Dispose();
            response4.Close();
            stream4.Dispose();

            // err.GenerarErrores("Login WhatsApp", "jsonResponse 4 " + jsonResponse, 0);
            // Deserializar el JSON a un objeto dinámico
            //jsonObject = JsonConvert.DeserializeObject(jsonResponse);
            RootObjectPhoneID phoneid = JsonConvert.DeserializeObject<RootObjectPhoneID>(jsonResponse);

            var phoneNumbers = new List<Combos>();


            foreach (phoneIDResponse number in phoneid.Data)
            {
                phoneNumbers.Add(new Combos
                {
                    Id = number.id,
                    Nombre = number.display_phone_number
                });
            }

            EtiquetaError.Visible = false;
            if (phoneNumbers.Count == 0)
            {
                EtiquetaError.Text = "La cuenta no tiene un telefono asociado";

                EtiquetaError.Visible = true;
                return;
            }

            string PhoneId = "";
            string Phone = "";
            if (phoneNumbers.Count > 1)
            {
                reader4.Close();
                reader4.Dispose();
                stream4.Close();
                stream4.Dispose();
                response4.Close();
                response4.Dispose();

                RadGridEmpresas.DataSource = phoneNumbers;
                RadGridEmpresas.DataBind();
            }
            else
            {
                PhoneId = phoneNumbers[0].Id;
                EtiquetaPhoneId.Text = PhoneId;
                Phone = phoneNumbers[0].Nombre;
                EtiquetaPhone.Text = Phone;


                // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, EtiquetawabaId.Text, EtiquetaaccessToken.Text, PhoneId, Etiquetabusinessid.Text, Phone);

                // var formFields = new Dictionary<string, string>
                // {
                //     {"phone_id", PhoneId},
                //     {"wba_id", wabaId},
                //     {"app_id", _appId},
                //     {"token", EtiquetaaccessToken.Text}
                // };

                // string apiUrl = "http://192.168.103.12:8080/api/v1/templates/create";

                // Registrar la tarea asíncrona
                // var capturedFields3 = formFields;
                // var capturedApiUrl3 = apiUrl;
                // RegisterAsyncTask(new System.Web.UI.PageAsyncTask(
                //     (sender2, e2, cb, extraData) => ExportToPDFWhatsApp(capturedApiUrl3, "Cotizacion.pdf", capturedFields3).ContinueWith(t => { cb(t.AsyncState); }),
                //     (ar) => { Response.Redirect("http://sistema.smuebleria.com"); },
                //     (ar) => { },
                //     null));

                // ExecuteRegisteredAsyncTasks();
            }
        }
        else
        {
            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = RadGridEmpresas.Rows[rowIndex];
            string Phone = "";
            string PhoneId = row.Cells[1].Text;
            EtiquetaPhoneId.Text = PhoneId;
            Phone = row.Cells[2].Text;
            EtiquetaPhone.Text = Phone;

            // Usuarios usu = new Usuarios();
            int IdEmpresa = 0;
            int IdSucursal = 0;
            //if (Application["Empresa"] != null)
            //    IdEmpresa = Convert.ToInt32(Application["Empresa"]);
            //if (Application["Sucursal"] != null)
            //    IdSucursal = Convert.ToInt32(Application["Sucursal"]);

            IdEmpresa = Convert.ToInt32(EtiquetaIdEmpresa.Text);
            IdSucursal = Convert.ToInt32(EtiquetaIdIdSucursal.Text);

            // usu.InsertarDatosWhatsApp(IdEmpresa, IdSucursal, EtiquetawabaId.Text, EtiquetaaccessToken.Text, PhoneId, Etiquetabusinessid.Text, Phone);

            // var formFields = new Dictionary<string, string>
            // {
            //     {"phone_id", PhoneId},
            //     {"wba_id", EtiquetawabaId.Text},
            //     {"app_id", _appId},
            //     {"token", EtiquetaaccessToken.Text}
            // };

            // string apiUrl = "http://192.168.103.12:8080/api/v1/templates/create";

            // Registrar la tarea asíncrona
            // var capturedFields4 = formFields;
            // var capturedApiUrl4 = apiUrl;
            // RegisterAsyncTask(new System.Web.UI.PageAsyncTask(
            //     (sender2, e2, cb, extraData) => ExportToPDFWhatsApp(capturedApiUrl4, "Cotizacion.pdf", capturedFields4).ContinueWith(t => { cb(t.AsyncState); }),
            //     (ar) => { Response.Redirect("http://sistema.smuebleria.com"); },
            //     (ar) => { },
            //     null));

            // ExecuteRegisteredAsyncTasks();
        }
    }

    // public async Task<string> ExportToPDFWhatsApp(string apiUrl, string fileName, Dictionary<string, string> formData)
    // {
    //     using (var client = new HttpClient())
    //
    //     using (var content = new MultipartFormDataContent())
    //     {
    //
    //         //// Agregar archivo
    //         //byte[] emptyFileBytes = new byte[0]; //Array.Empty<byte>();
    //         //var fileContent = new ByteArrayContent(emptyFileBytes);
    //         //fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/octet-stream");
    //         //content.Add(fileContent, "file", fileName); // "file" es el nombre del parámetro que espera el servicio
    //
    //         // Agregar otros campos de formulario
    //         foreach (var field in formData)
    //         {
    //             content.Add(new StringContent(field.Value), field.Key);
    //         }
    //
    //         // Enviar la solicitud
    //         var response = await client.PostAsync(apiUrl, content);
    //
    //         if (response.IsSuccessStatusCode)
    //         {
    //             return await response.Content.ReadAsStringAsync();
    //         }
    //         else
    //         {
    //             // Errores err = new Errores();
    //             // err.GenerarErrores("Error al enviar el mensaje de whatsapp", response.StatusCode.ToString(), 0);
    //             throw new HttpRequestException("Existen problemas para enviar su mesnaje de WhatsApp, consulte a su administrador. " + response.StatusCode);
    //         }
    //     }
    // }


    public class Combos
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
    }
}
