<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FacebookCallback.aspx.cs" Inherits="FacebookCallback" %>
<!DOCTYPE html>
<html>
<head>
    <title>Smuebleria - WhatsApp Login</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            max-width: 800px;
            margin: 0 auto;
        }
        button {
            background-color: #25D366;
            color: white;
            padding: 12px 24px;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            cursor: pointer;
        }
        button:hover {
            background-color: #128C7E;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="fb-root"></div>
        <h1>Smuebleria - WhatsApp Login</h1>
        
        <asp:HiddenField ID="hfCode" runat="server" />
        <button type="button" onclick="launchWhatsAppSignup()">Conectar WhatsApp Business</button>
    </form>
    
    <script>
        window.fbAsyncInit = function() {
            FB.init({
                appId: '1260594759179359',
                autoLogAppEvents: true,
                xfbml: true,
                version: 'v20.0'   
            });
        };

			function launchWhatsAppSignup() {
					FB.login(function(response) {
							console.log('FB.login full response:', JSON.stringify(response, null, 2));

							if (response && response.authResponse) {
									var code = response.authResponse.code;
									console.log('Code recibido en JS:', code);

									if (code) {
											// Envía el code por AJAX POST
											fetch('FacebookCallback.aspx', {
													method: 'POST',
													headers: {
															'Content-Type': 'application/x-www-form-urlencoded'
													},
													body: 'hfCode=' + encodeURIComponent(code)  // Envía como si fuera el hidden field
											})
											.then(response => response.text())
											.then(data => {
													console.log('Respuesta del servidor:', data);
													alert('Código enviado al servidor. Revisa los logs para detalles.');
											})
											.catch(err => {
													console.error('Error en AJAX:', err);
											});
									} else {
											console.warn('No hay code en authResponse');
									}
							} else {
									console.warn('No authResponse o login fallido', response);
							}
					}, {
							config_id: '1482981399558000',
							response_type: 'code',
							override_default_response_type: true,
							extras: {
									"version": "v3",
									"featureType": "whatsapp_business_app_onboarding"
							}
					});
			}

        (function(d, s, id){
            var js, fjs = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) return;
            js = d.createElement(s); 
            js.id = id;
            js.src = "https://connect.facebook.net/es_LA/sdk.js";
            fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
    </script>
</body>
</html>
