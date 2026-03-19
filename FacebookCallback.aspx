<%@ Page Language="C#" AutoEventWireup="true" Async="true"
    CodeFile="FacebookCallback.aspx.cs"
    Inherits="FacebookCallback"
    ValidateRequest="false"
    EnableEventValidation="false" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
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
    <div style="background-image: url('imagenes/patron1.png'); width: 100%; height: 100%; background-repeat: repeat;">

    <form id="form1" runat="server">
        <h2>Conectar WhatsApp Business</h2>
        <button type="button" onclick="launchWhatsAppSignup()">
            Conectar WhatsApp
        </button>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
       <table cellpadding="0" cellspacing="0" class="style1">
        <tr>
            <td align="center" width="10%" valign="middle">
                
                <table cellpadding="0" cellspacing="0" class="style1" style="height: 550px">
                    <tr>
                        <td valign="top">
                            <img id="Img1" runat="server" alt="" src="imagenes/pleca_02.png" height="550"/>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="right">
                            &nbsp;</td>
                    </tr>
                </table></td>
            <td width="80%" valign="middle">
                <table cellpadding="0" cellspacing="0" class="style1">
                    <tr>
                        <td>
                            &nbsp;&nbsp; &nbsp;</td>
                    </tr>
                    <tr>
                        <td>
                            Bienvenido a Smuebleria.com</td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="EtiquetaTitulo" Text="Seleecione la opción a la que desea dar acceso"></asp:Label>
                            <asp:Label runat="server" ID="EtiquetaaccessToken" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="Etiquetabusinessid" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="EtiquetawabaId" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="EtiquetaPhoneId" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="EtiquetaPhone" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="EtiquetaIdEmpresa" Visible="false" Text=""></asp:Label>
                            <asp:Label runat="server" ID="EtiquetaIdIdSucursal" Visible="false" Text=""></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp; &nbsp;</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:GridView ID="RadGridEmpresas" runat="server"
                                AutoGenerateColumns="False" GridLines="None" Visible="false"
                                OnRowCommand="RadGridEmpresas_ItemCommand"
                                DataKeyNames="Id">
                                <Columns>
                                    <asp:ButtonField ButtonType="Image" CommandName="Ingresar"
                                        HeaderText="Ingresar" ImageUrl="imagenes/boton_login.gif">
                                        <HeaderStyle Width="132px" />
                                        <ItemStyle Height="37px" Width="132px" />
                                    </asp:ButtonField>
                                    <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                                </Columns>
                            </asp:GridView>

                            <asp:Label runat="server" ID="EtiquetaError" ForeColor="Red" Visible="false" Text="La opción seleccionada no es válida. Seleecione otra opción"></asp:Label>
                        </td>
                    </tr>
                </table>

                </td>
            <td align="center" width="10%" valign="bottom" >
              
            </td>
        </tr>
    </table> 
    </form>
    
    <script>
        // const REDIRECT_URI = 'https://sistema.smuebleria.com/FacebookCallback.aspx';
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

