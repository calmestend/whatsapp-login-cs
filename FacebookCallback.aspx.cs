using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;
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
	private string _appId      = "1260594759179359";
	private string _appSecret  = "8c0193eaa98f146a3334212c4d470f7e";
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

			string getKey = Request.QueryString["getkey"];
			if (getKey == "1")
			{
				string sk = Session["sessionKey"] as string;
				Response.ContentType = "application/json";
				Response.Write(JsonConvert.SerializeObject(new { sessionKey = sk }));
				Response.End();
				return;
			}

			return;
		}

		if (Request.HttpMethod == "POST")
		{
			string body = new StreamReader(Request.InputStream).ReadToEnd();
			if (!string.IsNullOrEmpty(body))
			{
				HandleWebhookPayload(body);
			}
			return;
		}
	}

	private void HandleFacebookCode(string code)
	{
		// Validar code
		if (string.IsNullOrWhiteSpace(code))
		{
			Console.WriteLine("ERROR HandleFacebookCode: code vacío");
			return;
		}

		string sessionKey = Guid.NewGuid().ToString("N");
		string accessToken = null;

		try
		{
			string url = $"https://graph.facebook.com/v24.0/oauth/access_token"
				+ $"?client_id={_appId}"
				+ $"&redirect_uri={_redirectUri}"
				+ $"&client_secret={_appSecret}"
				+ $"&code={code}";

			string json = GetRequest(url);

			if (string.IsNullOrWhiteSpace(json))
			{
				Console.WriteLine("ERROR HandleFacebookCode: respuesta vacía de Facebook");
				return;
			}

			dynamic obj = JsonConvert.DeserializeObject(json);
			accessToken = (string)obj.access_token;

			if (string.IsNullOrWhiteSpace(accessToken))
			{
				Console.WriteLine("ERROR HandleFacebookCode: access_token vacío en respuesta: " + json);
				return;
			}

			Console.WriteLine("access_token obtenido: " + accessToken);
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR HandleFacebookCode al obtener token: " + ex.Message);
			return;
		}

		try
		{
			// Ver si ya llegó un webhook antes que el code
			// Intentar con el más reciente, luego los últimos 5
			var candidatos = _webhooks.Values
				.OrderByDescending(s => s.CreatedAt)
				.Take(5)
				.ToList();

			PendingSession matched = null;

			foreach (var candidato in candidatos)
			{
				if (string.IsNullOrWhiteSpace(candidato.WabaId))
				{
					Console.WriteLine("Candidato sin waba_id, saltando...");
					continue;
				}

				Console.WriteLine("Intentando emparejar con waba_id=" + candidato.WabaId);

				try
				{
					// Prueba real: intentar obtener teléfonos con este waba_id + access_token
					List<Combos> phones = ObtenerTelefonos(candidato.WabaId, accessToken);

					if (phones != null && phones.Count > 0)
					{
						// Emparejado correctamente
						candidato.AccessToken = accessToken;
						candidato.Phones      = phones;
						matched               = candidato;

						// Limpiar de _webhooks
						PendingSession removed;
						_webhooks.TryRemove(candidato.WabaId, out removed);

						Console.WriteLine("Emparejado con waba_id=" + candidato.WabaId
								+ ", teléfonos=" + phones.Count);
						break;
					}
					else
					{
						Console.WriteLine("waba_id=" + candidato.WabaId
								+ " no devolvió teléfonos, descartando candidato");
					}
				}
				catch (Exception exMatch)
				{
					Console.WriteLine("waba_id=" + candidato.WabaId
							+ " falló prueba: " + exMatch.Message + ", descartando candidato");
				}
			}

			if (matched != null)
			{
				_pending[sessionKey] = matched;
				Console.WriteLine("Sesión completa guardada con sessionKey=" + sessionKey);
			}
			else
			{
				// Webhook aún no llegó o ningún candidato coincidió
				// Guardar solo el token y esperar el webhook via polling
				_pending[sessionKey] = new PendingSession
				{
					AccessToken = accessToken,
					CreatedAt   = DateTime.UtcNow
				};
				Console.WriteLine("Ningún webhook coincidió. Guardando token, esperando webhook...");
			}

			Session["sessionKey"] = sessionKey;
			Console.WriteLine("sessionKey generada: " + sessionKey);
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR HandleFacebookCode al emparejar: " + ex.Message);
		}
	}

	private void HandleWebhookPayload(string body)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(body))
			{
				Console.WriteLine("ERROR HandleWebhookPayload: body vacío");
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			dynamic payload = JsonConvert.DeserializeObject(body);

			if (payload == null)
			{
				Console.WriteLine("ERROR HandleWebhookPayload: payload nulo tras deserializar");
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			string wabaId       = (string)payload.waba_id;
			string businessId   = (string)payload.owner_business_id;
			string partnerAppId = (string)payload.partner_app_id;

			if (string.IsNullOrWhiteSpace(wabaId))
			{
				Console.WriteLine("ERROR HandleWebhookPayload: waba_id vacío. body=" + body);
				Response.StatusCode = 200;
				Response.End();
				return;
			}

			if (string.IsNullOrWhiteSpace(businessId))
				Console.WriteLine("ADVERTENCIA HandleWebhookPayload: owner_business_id vacío");

			if (string.IsNullOrWhiteSpace(partnerAppId))
				Console.WriteLine("ADVERTENCIA HandleWebhookPayload: partner_app_id vacío");

			Console.WriteLine("Webhook recibido: waba_id=" + wabaId
					+ ", business_id=" + businessId
					+ ", partner_app_id=" + partnerAppId);

			// Ver si ya hay un _pending con access_token sin waba_id aún
			var candidatos = _pending.Values
				.Where(s => !string.IsNullOrWhiteSpace(s.AccessToken) && s.WabaId == null)
				.OrderByDescending(s => s.CreatedAt)
				.Take(5)
				.ToList();

			bool emparejado = false;

			foreach (var candidato in candidatos)
			{
				Console.WriteLine("Intentando emparejar pending con access_token existente...");

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

						Console.WriteLine("Emparejado webhook con pending existente, teléfonos=" + phones.Count);
						break;
					}
					else
					{
						Console.WriteLine("Pending no coincidió (sin teléfonos), descartando candidato");
					}
				}
				catch (Exception exMatch)
				{
					Console.WriteLine("Pending falló prueba: " + exMatch.Message + ", descartando");
				}
			}

			if (!emparejado)
			{
				// El code aún no llegó, guardar en _webhooks para emparejarlo después
				_webhooks[wabaId] = new PendingSession
				{
					WabaId       = wabaId,
					BusinessId   = businessId,
					PartnerAppId = partnerAppId,
					CreatedAt    = DateTime.UtcNow
				};
				Console.WriteLine("Ningún pending coincidió. Guardando webhook para emparejamiento futuro.");
			}

			Response.StatusCode  = 200;
			Response.ContentType = "application/json";
			Response.Write(JsonConvert.SerializeObject(new { success = true }));
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR HandleWebhookPayload: " + ex.Message);
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
				Console.WriteLine("ERROR HandlePoll: sessionKey vacía");
				Response.ContentType = "application/json";
				Response.Write(JsonConvert.SerializeObject(new { ready = false, error = "sessionKey vacía" }));
				Response.End();
				return;
			}

			PendingSession session;
			if (_pending.TryGetValue(sessionKey, out session))
			{
				if (session.Phones != null && session.Phones.Count > 0)
				{
					Console.WriteLine("Poll: listo, enviando " + session.Phones.Count + " teléfonos");
					Response.ContentType = "application/json";
					Response.Write(JsonConvert.SerializeObject(new
								{
								ready  = true,
								phones = session.Phones
								}));
				}
				else
				{
					Console.WriteLine("Poll: sesión encontrada pero teléfonos aún no listos");
					Response.ContentType = "application/json";
					Response.Write(JsonConvert.SerializeObject(new { ready = false }));
				}
			}
			else
			{
				Console.WriteLine("Poll: sessionKey no encontrada: " + sessionKey);
				Response.ContentType = "application/json";
				Response.Write(JsonConvert.SerializeObject(new { ready = false, error = "sesión no encontrada" }));
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("ERROR HandlePoll: " + ex.Message);
			Response.ContentType = "application/json";
			Response.Write(JsonConvert.SerializeObject(new { ready = false }));
		}
		Response.End();
	}

	// ---------------------------------------------------------------
	// Obtener teléfonos desde Graph API
	// ---------------------------------------------------------------
	private List<Combos> ObtenerTelefonos(string wabaId, string accessToken)
	{
		if (string.IsNullOrWhiteSpace(wabaId))
			throw new ArgumentException("wabaId vacío");

		if (string.IsNullOrWhiteSpace(accessToken))
			throw new ArgumentException("accessToken vacío");

		string url  = $"https://graph.facebook.com/v24.0/{wabaId}/phone_numbers?access_token={accessToken}";
		string json = GetRequest(url);

		if (string.IsNullOrWhiteSpace(json))
			throw new Exception("Respuesta vacía de Graph API phone_numbers");

		Console.WriteLine("phone_numbers response: " + json);

		RootObjectPhoneID phoneid = JsonConvert.DeserializeObject<RootObjectPhoneID>(json);

		if (phoneid == null || phoneid.Data == null)
			throw new Exception("Respuesta inválida de Graph API phone_numbers");

		var result = new List<Combos>();
		foreach (phoneIDResponse p in phoneid.Data)
		{
			if (string.IsNullOrWhiteSpace(p.id) || string.IsNullOrWhiteSpace(p.display_phone_number))
			{
				Console.WriteLine("ADVERTENCIA: teléfono con campos vacíos, saltando");
				continue;
			}
			Console.WriteLine("Telefono encontrado: " + p.display_phone_number);
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
