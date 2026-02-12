<%@ Page Language="C#" AutoEventWireup="true" Async="true"
    CodeFile="FacebookCallback.aspx.cs"
    Inherits="FacebookCallback"
    ValidateRequest="false"
    EnableEventValidation="false" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Smuebleria - WhatsApp Login</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 40px;
            max-width: 600px;
            margin: auto;
        }
        button {
            background-color: #25D366;
            color: white;
            border: none;
            padding: 12px 24px;
            font-size: 16px;
            border-radius: 5px;
            cursor: pointer;
        }
        button:hover {
            background-color: #128C7E;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Conectar WhatsApp Business</h2>
        <button type="button" onclick="launchWhatsAppSignup()">
            Conectar WhatsApp
        </button>
    </form>
    
    <script>
        const REDIRECT_URI = 'https://whatsapp-login-cs-production.up.railway.app/FacebookCallback.aspx';
        
        function launchWhatsAppSignup() {
            const appId = '1260594759179359';
            const configId = '1482981399558000';
            const state = Math.random().toString(36).substring(7);
            
            const authUrl = `https://www.facebook.com/v24.0/dialog/oauth?` +
                `client_id=${appId}` +
                `&redirect_uri=${encodeURIComponent(REDIRECT_URI)}` +
                `&response_type=code` +
                `&config_id=${configId}` +
                `&state=${encodeURIComponent(state)}` +
                `&extras=${encodeURIComponent(JSON.stringify({
                    version: 'v3',
                    featureType: 'whatsapp_business_app_onboarding'
                }))}`;
            
            sessionStorage.setItem('fb_state', state);
            
            window.location.href = authUrl;
        }
        
        window.addEventListener('load', function() {
            const urlParams = new URLSearchParams(window.location.search);
            const code = urlParams.get('code');
            const state = urlParams.get('state');
            const savedState = sessionStorage.getItem('fb_state');
            
            if (code && state === savedState) {
                console.log('Code received:', code);
                
                sessionStorage.removeItem('fb_state');
                
                fetch('FacebookCallback.aspx', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    },
                    body: 'hfCode=' + encodeURIComponent(code)
                })
                .then(r => r.text())
                .then(data => {
                    console.log('Response:', data);
                    alert('WhatsApp conectado exitosamente');
                    window.history.replaceState({}, document.title, window.location.pathname);
                })
                .catch(err => {
                    console.error(err);
                    alert('Error al conectar WhatsApp');
                });
            }
        });
    </script>
</body>
</html>
