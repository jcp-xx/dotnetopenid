using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Janrain.OpenId.Server;

/// <summary>
/// This is the primary page for this open-id server.
/// This page is responsible for handling all open-id compliant requests:
///
/// CheckIdRequest:
///   - when openid.mode='checkid_immediate' or openid.mode='checkid_setup'
///   - this is the initial message request sent to the server via server side call from the server
///   - this is stored in session in State.Session.LastRequest
/// 
/// AssociateRequest
///   - when openid.mode='associate'
///   - this is optionally sent by the consumer who is implementing smart mode to obtain the shared secret before an actual CheckIDRequest
///   - this is sent via a HTTP server side call from the consumer
///  
/// CheckAuthRequest
///   - when open.mode='check_authentication'
///   - this is request from the consumer to authenticate the user 
///   - this is sent via a HTTP 302 redirect by the consumer
/// </summary>
public partial class server : System.Web.UI.Page
{
    protected void Page_Load(object src, System.EventArgs evt)
    {
        Server openIDServer = new Janrain.OpenId.Server.Server();
        Janrain.OpenId.Server.Request request = null;

        // determine what incoming message was received
        try
        {
            if (Request.HttpMethod == "GET")

                request = Decoder.Decode(Request.QueryString);
            else
                request = Decoder.Decode(Request.Form);
        }
        catch (Janrain.OpenId.Server.ProtocolException e)
        {
            Util.GenerateHttpResponse(e);
            return;
        }
        if (request == null)
        {
            contentForWebBrowsers.Visible = true;
            return;
        }
            

        // process the incoming message appropriately and send the response
        Janrain.OpenId.Server.Response response = null;
        if (request is Janrain.OpenId.Server.CheckIdRequest)
        {
            Janrain.OpenId.Server.CheckIdRequest idrequest = (Janrain.OpenId.Server.CheckIdRequest)request;
            if (idrequest.Immediate)
            {
                String s = Util.ExtractUserName(idrequest.IdentityUrl);                
                bool allow = (s != User.Identity.Name);
                response = idrequest.Answer(allow, State.ServerUri);
            }
            else
            {
                State.Session.LastRequest = (CheckIdRequest)request;
                Response.Redirect("decide.aspx");
            }
        }
        else if (request is Janrain.OpenId.Server.CheckAuthRequest)
        {
            response = openIDServer.HandleRequest((Janrain.OpenId.Server.CheckAuthRequest)request);
        }
        else if (request is Janrain.OpenId.Server.AssociateRequest)
        {
            response = openIDServer.HandleRequest((Janrain.OpenId.Server.AssociateRequest)request);
        }        
        Util.GenerateHttpResponse(response);
    }






}

