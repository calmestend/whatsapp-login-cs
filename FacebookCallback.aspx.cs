using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;
using System.Linq;
using System.Web.UI.WebControls;

public class businessidResponse
{
	public string id   { get; set; }
	public string name { get; set; }
}

public class WebIDResponse
{
	public string id                          { get; set; }
	public string name                        { get; set; }
	public string currency                    { get; set; }
	public string timezone_id                 { get; set; }
	public string message_template_namespace  { get; set; }
}

public class phoneIDResponse
{
	public string id                       { get; set; }
	public string verified_name            { get; set; }
	public string code_verification_status { get; set; }
	public string display_phone_number     { get; set; }
	public string quality_rating           { get; set; }
	public string platform_type            { get; set; }
	public object throughput               { get; set; }
}

public class Cursors
{
	public string Before { get; set; }
	public string After  { get; set; }
}

public class Paging
{
	public Cursors Cursors { get; set; }
}

public class RootObjectbusiness
{
	public List<businessidResponse> Data { get; set; }
	public Paging Paging                 { get; set; }
}

public class RootObjectWebID
{
	public List<WebIDResponse> Data { get; set; }
	public Paging Paging            { get; set; }
}

public class RootObjectPhoneID
{
	public List<phoneIDResponse> Data { get; set; }
	public Paging Paging              { get; set; }
}

public class AccessTokenResponse
{
	public string access_token { get; set; }
	public string token_type   { get; set; }
}

public class Combos
{
	public string Id     { get; set; }
	public string Nombre { get; set; }
}

public class PendingSession
{
	public string AccessToken  { get; set; }
	public string WabaId       { get; set; }
	public string BusinessId   { get; set; }
	public string PartnerAppId { get; set; }
	public List<Combos> Phones { get; set; }
	public DateTime CreatedAt  { get; set; }
}

public partial class FacebookCallback : Page
{
	private string _appId       = "1260594759179359";
	private string _appSecret   = "8c0193eaa98f146a3334212c4d470f7e";
	private string _redirectUri = "https://whatsapp-login-cs-production.up.railway.app/FacebookCallback.aspx";

	private static ConcurrentDictionary<string, PendingSession> _pending
		= new ConcurrentDictionary<string, PendingSession>();

	private static ConcurrentDictionary<string, PendingSession> _webhooks
		= new ConcurrentDictionary<string, PendingSession>();

	protected void Page_Load(object sender, EventArgs e)
	{
		if (Request.HttpMethod == "GET")
		{
			string code = Request.QueryString["code"];
			if (!string.IsNullOrEmpty(code))
			{
				HandleFacebookCode(code);
				return;
			}

			string pollKey = Request.QueryString["poll"];
			if (!string.IsNullOrEmpty(pollKey))
			{
				HandlePoll(pollKey);
				return;
			}

			return;
		}

		if (Request.HttpMethod == "POST")
		{
			string body = new StreamReader(Request.InputStream).ReadToEnd();
			if (!string.IsNullOrEmpty(body))
				HandleWebhookPayload(body);
			return;
		}
	}

	private void HandleFacebookCode(string code)
	{
		if (string.IsNullOrWhiteSpace(code)) return;

		string sessionKey  = Guid.NewGuid().ToString("N");
		string accessToken = null;

		try
		{
			string url = $"https://graph.facebook.com/v24.0/oauth/access_token"
				+ $"?client_id={_appId}"
				+ $"&redirect_uri={_redirectUri}"
				+ $"&client_secret={_appSecret}"
				+ $"&code={code}";

			string json = GetRequest(url);
			if (string.IsNullOrWhiteSpace(json)) return;

			dynamic obj = JsonConvert.DeserializeObject(json);
			accessToken = (string)obj.access_token;

			if (string.IsNullOrWhiteSpace(accessToken)) return;

			Console.WriteLine("access_token OK");
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR token: " + ex.Message);
			return;
		}

		try
		{
			var candidatos = _webhooks.Values
				.OrderByDescending(s => s.CreatedAt)
				.Take(5)
				.ToList();

			PendingSession matched = null;

			foreach (var candidato in candidatos)
			{
				if (string.IsNullOrWhiteSpace(candidato.WabaId)) continue;

				try
				{
					List<Combos> phones = ObtenerTelefonos(candidato.WabaId, accessToken);

					if (phones != null && phones.Count > 0)
					{
						candidato.AccessToken = accessToken;
						candidato.Phones      = phones;
						matched               = candidato;

						PendingSession removed;
						_webhooks.TryRemove(candidato.WabaId, out removed);

						Console.WriteLine("Match waba_id=" + candidato.WabaId + " phones=" + phones.Count);
						break;
					}
				}
				catch (Exception exMatch)
				{
					Console.WriteLine("ERROR match waba_id=" + candidato.WabaId + ": " + exMatch.Message);
				}
			}

			if (matched != null)
			{
				_pending[sessionKey] = matched;
				Console.WriteLine("Sesion completa sk=" + sessionKey);
			}
			else
			{
				_pending[sessionKey] = new PendingSession
				{
					AccessToken = accessToken,
					CreatedAt   = DateTime.UtcNow
				};
				Console.WriteLine("Sin webhook previo. Guardando token, esperando webhook...");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR emparejar: " + ex.Message);
		}

		Response.Redirect("FacebookCallback.aspx?sk=" + sessionKey, false);
	}

	private void HandleWebhookPayload(string body)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(body))
			{
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			dynamic payload = JsonConvert.DeserializeObject(body);
			if (payload == null)
			{
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			string wabaId       = (string)payload.waba_id;
			string businessId   = (string)payload.owner_business_id;
			string partnerAppId = (string)payload.partner_app_id;

			if (string.IsNullOrWhiteSpace(wabaId))
			{
				Console.WriteLine("ERROR webhook: waba_id vacio. body=" + body);
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			Console.WriteLine("Webhook waba_id=" + wabaId + " business_id=" + businessId);

			var candidatos = _pending.Values
				.Where(s => !string.IsNullOrWhiteSpace(s.AccessToken) && s.WabaId == null)
				.OrderByDescending(s => s.CreatedAt)
				.Take(5)
				.ToList();

			bool emparejado = false;

			foreach (var candidato in candidatos)
			{
				try
				{
					List<Combos> phones = ObtenerTelefonos(wabaId, candidato.AccessToken);

					if (phones != null && phones.Count > 0)
					{
						candidato.WabaId       = wabaId;
						candidato.BusinessId   = businessId;
						candidato.PartnerAppId = partnerAppId;
						candidato.Phones       = phones;
						emparejado             = true;

						Console.WriteLine("Webhook emparejado phones=" + phones.Count);
						break;
					}
				}
				catch (Exception exMatch)
				{
					Console.WriteLine("ERROR pending match: " + exMatch.Message);
				}
			}

			if (!emparejado)
			{
				_webhooks[wabaId] = new PendingSession
				{
					WabaId       = wabaId,
					BusinessId   = businessId,
					PartnerAppId = partnerAppId,
					CreatedAt    = DateTime.UtcNow
				};
				Console.WriteLine("Sin pending. Guardando webhook waba_id=" + wabaId);
			}

			Response.StatusCode  = 200;
			Response.ContentType = "application/json";
			Response.Write(JsonConvert.SerializeObject(new { success = true }));
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR webhook: " + ex.Message);
			Response.StatusCode = 200;
		}
		Response.End();
	}

	private void HandlePoll(string sessionKey)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(sessionKey))
			{
				Response.ContentType = "application/json";
				Response.Write(JsonConvert.SerializeObject(new { ready = false, error = "sessionKey vacia" }));
				Response.End();
				return;
			}

			PendingSession session;
			if (_pending.TryGetValue(sessionKey, out session))
			{
				if (session.Phones != null && session.Phones.Count > 0)
				{
					Console.WriteLine("Poll OK phones=" + session.Phones.Count);
					Response.ContentType = "application/json";
					Response.Write(JsonConvert.SerializeObject(new
								{
								ready  = true,
								phones = session.Phones
								}));
				}
				else
				{
					Response.ContentType = "application/json";
					Response.Write(JsonConvert.SerializeObject(new { ready = false }));
				}
			}
			else
			{
				Console.WriteLine("Poll: sk no encontrada=" + sessionKey);
				Response.ContentType = "application/json";
				Response.Write(JsonConvert.SerializeObject(new { ready = false, error = "sesion no encontrada" }));
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR poll: " + ex.Message);
			Response.ContentType = "application/json";
			Response.Write(JsonConvert.SerializeObject(new { ready = false }));
		}
		Response.End();
	}

	private List<Combos> ObtenerTelefonos(string wabaId, string accessToken)
	{
		if (string.IsNullOrWhiteSpace(wabaId))
			throw new ArgumentException("wabaId vacio");

		if (string.IsNullOrWhiteSpace(accessToken))
			throw new ArgumentException("accessToken vacio");

		string url  = $"https://graph.facebook.com/v24.0/{wabaId}/phone_numbers?access_token={accessToken}";
		string json = GetRequest(url);

		if (string.IsNullOrWhiteSpace(json))
			throw new Exception("Respuesta vacia de Graph API");

		RootObjectPhoneID phoneid = JsonConvert.DeserializeObject<RootObjectPhoneID>(json);

		if (phoneid == null || phoneid.Data == null)
			throw new Exception("Respuesta invalida de Graph API");

		var result = new List<Combos>();
		foreach (phoneIDResponse p in phoneid.Data)
		{
			if (string.IsNullOrWhiteSpace(p.id) || string.IsNullOrWhiteSpace(p.display_phone_number))
				continue;

			Console.WriteLine("Telefono: " + p.display_phone_number);
			result.Add(new Combos { Id = p.id, Nombre = p.display_phone_number });
		}
		return result;
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
