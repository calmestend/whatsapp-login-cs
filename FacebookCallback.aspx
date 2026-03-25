<%@ Page Language="C#" AutoEventWireup="true" Async="true"
    CodeFile="FacebookCallback.aspx.cs"
    Inherits="FacebookCallback"
    ValidateRequest="false"
    EnableEventValidation="false" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Smuebleria - WhatsApp Login</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; max-width: 600px; margin: auto; }
        button {
            background-color: #25D366; color: white; border: none;
            padding: 12px 24px; font-size: 16px; border-radius: 5px; cursor: pointer;
        }
        button:hover { background-color: #128C7E; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Conectar WhatsApp Business</h2>
        <button type="button" onclick="launchWhatsAppSignup()">Conectar WhatsApp</button>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <asp:Label runat="server" ID="EtiquetaTitulo"       Text="Seleccione la opción a la que desea dar acceso"></asp:Label>
        <asp:Label runat="server" ID="EtiquetaaccessToken"  Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="Etiquetabusinessid"   Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetawabaId"       Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetaPhoneId"      Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetaPhone"        Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetaIdEmpresa"    Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetaIdIdSucursal" Visible="false" Text=""></asp:Label>
        <asp:Label runat="server" ID="EtiquetaError"        ForeColor="Red" Visible="false"
            Text="La opción seleccionada no es válida."></asp:Label>
    </form>

    <div id="estadoDiv"    style="margin-top:20px;font-size:16px;color:#333"></div>
    <div id="telefonosDiv" style="margin-top:10px"></div>

    <script>
        const REDIRECT_URI = 'https://whatsapp-login-cs-production.up.railway.app/FacebookCallback.aspx';

        async function launchWhatsAppSignup() {
            // 1. Despertar el backend antes de ir a Facebook
            document.getElementById('estadoDiv').innerText = 'Iniciando...';
            try { await fetch('FacebookCallback.aspx?ping=1'); } catch(e) {}
            await new Promise(resolve => setTimeout(resolve, 1500));

            // 2. Redirigir a Facebook
            const appId    = '1260594759179359';
            const configId = '1482981399558000';
            const state    = Math.random().toString(36).substring(7);

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

        window.addEventListener('load', function () {
            const urlParams  = new URLSearchParams(window.location.search);
            const code       = urlParams.get('code');
            const state      = urlParams.get('state');
            const savedState = sessionStorage.getItem('fb_state');

            if (code && state === savedState) {
                sessionStorage.removeItem('fb_state');
                window.history.replaceState({}, document.title, window.location.pathname);
                document.getElementById('estadoDiv').innerText = 'Conectando con WhatsApp...';
                iniciarPolling();
            }
        });

        function iniciarPolling() {
            fetch('FacebookCallback.aspx?getkey=1')
                .then(r => r.json())
                .then(data => {
                    if (data.sessionKey) {
                        console.log('sessionKey: ' + data.sessionKey);
                        pollConKey(data.sessionKey);
                    } else {
                        document.getElementById('estadoDiv').innerText = 'Error al iniciar sesión';
                    }
                });
        }

        function pollConKey(sessionKey) {
            let intentos = 0;
            const intervalo = setInterval(function () {
                intentos++;
                console.log('Poll intento ' + intentos);

                fetch('FacebookCallback.aspx?poll=' + sessionKey)
                    .then(r => r.json())
                    .then(data => {
                        if (data.ready) {
                            clearInterval(intervalo);
                            mostrarTelefonos(data.phones);
                        }
                    })
                    .catch(err => {
                        clearInterval(intervalo);
                        console.error('Error polling:', err);
                    });

                if (intentos >= 30) {
                    clearInterval(intervalo);
                    document.getElementById('estadoDiv').innerText = 'Tiempo de espera agotado. Intente de nuevo.';
                }
            }, 2000);
        }

        function mostrarTelefonos(phones) {
            document.getElementById('estadoDiv').innerText = 'Selecciona tu número:';
            let html = '<ul style="list-style:none;padding:0">';
            phones.forEach(function (p) {
                html += `<li style="margin:8px 0">
                    <button onclick="seleccionarTelefono('${p.Id}', '${p.Nombre}')"
                        style="background:#25D366;color:white;border:none;padding:10px 20px;
                               font-size:15px;border-radius:5px;cursor:pointer">
                        ${p.Nombre}
                    </button>
                </li>`;
            });
            html += '</ul>';
            document.getElementById('telefonosDiv').innerHTML = html;
        }

        function seleccionarTelefono(phoneId, phone) {
            console.log('Seleccionado: phoneId=' + phoneId + ', phone=' + phone);
            document.getElementById('estadoDiv').innerText = '✅ Número seleccionado: ' + phone;
            document.getElementById('telefonosDiv').innerHTML = '';
            // Aquí puedes guardar en DB o continuar el flujo
        }
    </script>
</body>
</html>
