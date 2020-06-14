using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using StackExchange.Redis;
using System;

namespace MoodyBudgeter.Utility.Cache.Redis
{
    public class RedisConnection
    {
        private static Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var environment = new EnvironmentRequester();

            string configString = environment.GetVariable("RedisName");
            var options = ConfigurationOptions.Parse(configString);
            options.ClientName = "MoodyBudgeter";
            options.AbortOnConnectFail = false;

            options.DefaultDatabase = int.Parse(environment.GetVariable("RedisDB"));

            // Leaving false until I run into something where I need to set it to true.
            options.AllowAdmin = false;

            return ConnectionMultiplexer.Connect(options);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }
    }
}
