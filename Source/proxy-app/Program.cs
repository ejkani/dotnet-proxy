using Microsoft.AspNetCore.HttpOverrides;
using ProxyApp;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpForwarder();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Add app middleware

var transformer = new CustomTransformer(); // or HttpTransformer.Default;
var requestOptions = new ForwarderRequestConfig { Timeout = TimeSpan.FromSeconds(100) };
var forwarder = app.Services.GetRequiredService<IHttpForwarder>();

var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = true,
    AllowAutoRedirect = true,
    AutomaticDecompression = DecompressionMethods.All,
    UseCookies = false
});

app.UseForwardedHeaders();
app.UseRouting();

app.MapGet("/", () => "Hello World from the PROXY!");

app.UseEndpoints(endpoints =>
{
    endpoints.Map("/{**catch-all}", async httpContext =>
    {
        var error = await forwarder.SendAsync(
            httpContext,
            app.Configuration["ApiAppUrl"], 
            httpClient,
            requestOptions,
            transformer);
        // Check if the proxy operation was successful
        if (error != ForwarderError.None)
        {
            var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
            var exception = errorFeature.Exception;
        }
    });
});


app.Run();
