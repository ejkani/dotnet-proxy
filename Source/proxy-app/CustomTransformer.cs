using Yarp.ReverseProxy.Forwarder;

namespace ProxyApp
{
    public class CustomTransformer : HttpTransformer
    {
        public override async ValueTask TransformRequestAsync(HttpContext httpContext,
            HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            // Copy headers normally and then remove the original host.
            // Use the destination host from proxyRequest.RequestUri instead.
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
            proxyRequest.Headers.Host = null;
        }
    }
}
