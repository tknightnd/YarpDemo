using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy.Transforms;
using YarpTest1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add the reverse proxy capability to the server
builder.Services.AddReverseProxy()
    // Initialize the reverse proxy from the "ReverseProxy" section of configuration
    //.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    .AddTransforms(ctx =>
    {
        // TODO: how to only apply this transform to selected requests/destinations?
        
        ctx.AddRequestTransform(async reqCtx =>
        {
            // TODO: this is executed on EVERY request!
            reqCtx.ProxyRequest.Headers.Authorization = null;
            // reqCtx.DestinationPrefix has destination... but what happens when in multiple routes?
            await Task.Delay(0); // TODO: 

            // for the PR introducing rate limiting to YARP: https://github.com/dotnet/yarp/pull/1967/files
            // https://learn.microsoft.com/en-gb/aspnet/core/performance/rate-limit?view=aspnetcore-9.0

            return;
        });
    })
    
    .LoadFromMemory(ProgramBootstrap.GetRoutes(), ProgramBootstrap.GetClusters());
// To kick a refresh: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/config-providers?view=aspnetcore-9.0#in-memory-config
// httpContext.RequestServices.GetRequiredService<InMemoryConfigProvider>().Update(routes, clusters);


builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("oneRequestPer10Secs", o2 =>
    {
        // TODO: partition key???
        o2.PermitLimit = 20;
        o2.QueueLimit = 1;
        o2.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o2.Window = TimeSpan.FromSeconds(10);
    });
    opt.RejectionStatusCode = 429;
});
builder.WebHost.UseUrls("http://localhost:5010");

var app = builder.Build();

// Register the reverse proxy routes
 app.UseRateLimiter();
app.MapReverseProxy();

app.Run();

