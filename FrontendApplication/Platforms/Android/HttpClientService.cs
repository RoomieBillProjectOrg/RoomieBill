using System;
using System.Net.Security;
using Xamarin.Android.Net;

namespace FrontendApplication.Services;

public partial class HttpClientService
{
     public partial HttpMessageHandler GetPlatformSpecificHttpMessageHandler(){
        var androidHttpHandler = new AndroidMessageHandler{
            ServerCertificateCustomValidationCallback = (httpRequestMessage, certificate, chain, sslPolicyErrors) => {
                if(certificate?.Issuer == "CN=localhost" || sslPolicyErrors == SslPolicyErrors.None)
                    return true;
                return false;
            }
        };
        return androidHttpHandler;
     }
}
