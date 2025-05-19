using Yarp.ReverseProxy.Configuration;

namespace YarpTest1
{

    public static class ProgramBootstrap
    {
        const string DEBUG_HEADER = "Debug";
        const string DEBUG_METADATA_KEY = "debug";
        const string DEBUG_VALUE = "true";

        public static RouteConfig[] GetRoutes()
        {
            return
            [
                new RouteConfig()
                {
                    RouteId = "route",
                    ClusterId = "cluster1",
                    RateLimiterPolicy = "oneRequestPer10Secs",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Path = "{**catch-all}"
                    }
                }
            ];
        }

        public static ClusterConfig[] GetClusters()
        {
            return
            [
                new ClusterConfig()
                {
                    ClusterId = "cluster1",
                    HealthCheck = new HealthCheckConfig()
                    {
                        Active = new ActiveHealthCheckConfig()
                        {
                            Enabled = true,
                            Interval = TimeSpan.FromSeconds(5),
                            Timeout = TimeSpan.FromSeconds(30),
                            Policy = "ConsecutiveFailures", // Docs = ??
                            Path = "/heartbeat",
                        },
                    },
                    Metadata = new Dictionary<string, string>()
                    {
                        { "ConsecutiveFailuresHealthPolicy.Threshold", "3" } // TODO: mapping to above?
                    },
                    LoadBalancingPolicy = "Random", // docs: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/load-balancing?view=aspnetcore-9.0
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            "destination1-primary", new DestinationConfig()
                            {
                                Address = "http://localhost:5000", 
                                // TODO: authentication via requesttransforms? https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/extensibility-transforms?view=aspnetcore-9.0
                            }
                        },
                        {
                            "destination1-secondary", new DestinationConfig()
                            {
                                Address = "http://127.0.0.1:5000",
                                // TODO: authentication via requesttransforms? https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/extensibility-transforms?view=aspnetcore-9.0
                            }
                        },
                    }
                }
            ];
        }
    }
}
