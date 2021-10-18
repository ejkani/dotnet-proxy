using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Map services first
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Then add middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapGet("/", () => "Hello World from the API!");

app.MapGet("/proxytest/{**catch-all}",
async ([FromServices] IHttpContextAccessor context) =>
{

    ArgumentNullException.ThrowIfNull(context.HttpContext, nameof(context.HttpContext));

    var httpContext = context.HttpContext;
    var request = httpContext.Request;
    var connection = httpContext.Connection;
    var feature = httpContext.Features.Get<IHttpConnectionFeature>();
    var serverIp = feature?.LocalIpAddress?.ToString();
    var serverParsedIp = feature?.LocalIpAddress;
    var remoteIp = connection.RemoteIpAddress;
    var remoteParsedIp = connection.RemoteIpAddress;
    var localIp = connection.LocalIpAddress;
    var remoteParsedIpResult = "";
    var serverParsedIpResult = "";

    if (remoteParsedIp != null)
    {
        // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
        // This usually only happens when the browser is on the same machine as the server.
        if (remoteParsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            remoteParsedIp = System.Net.Dns
                .GetHostEntry(remoteParsedIp)
                .AddressList
                .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
        remoteParsedIpResult = remoteParsedIp.ToString();
    }

    if (serverParsedIp != null)
    {
        // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
        // This usually only happens when the browser is on the same machine as the server.
        if (serverParsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            serverParsedIp = System.Net.Dns
                .GetHostEntry(serverParsedIp)
                .AddressList
                .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
        serverParsedIpResult = serverParsedIp.ToString();
    }

    var requestHeaders = new Dictionary<string, string>();

    var sb = new StringBuilder();
    sb.AppendLine($"Path: {request.Path}");
    sb.AppendLine($"QueryString: {request.QueryString}");
    sb.AppendLine("---------------------------------------------------");
    sb.AppendLine($"RemoteIpAddress: {remoteIp}");
    sb.AppendLine($"LocalIpAddress: {localIp}");
    sb.AppendLine($"remoteParsedIp: {remoteParsedIp}");
    sb.AppendLine($"serverIp: {serverIp}");
    sb.AppendLine($"serverParsedIp: {serverParsedIp}");
    sb.AppendLine("---------------------------------------------------");

    foreach (var header in request.Headers)
    {
        sb.AppendLine($"Header: {header.Key}={header.Value}");
    }
    sb.AppendLine("---------------------------------------------------");

    return sb.ToString();
});


app.Run();
