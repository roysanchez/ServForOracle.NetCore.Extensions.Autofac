using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServForOracle.NetCore.Extensions.Autofac.Tests")]
namespace ServForOracle.NetCore.Extensions.Autofac
{
    public class ServForOracleModule<T> : ServForOracleModule where T : Enum
    {
        private Dictionary<object, string> MapToDictionary(IEnumerable<string> connectionStrings)
        {
            var dictionary =
                Enum.GetValues(typeof(T)).Cast<T>()
                    .Zip(connectionStrings, (T key, string value) => new KeyValuePair<object, string>(key, value))
                    .ToDictionary(c => c.Key, c => c.Value);

            return dictionary;
        }

        public ServForOracleModule(IEnumerable<string> connectionStrings)
        {
            if (connectionStrings is null || !connectionStrings.Any())
            {
                throw new ArgumentNullException(nameof(connectionStrings));
            }

            KeyNameConnectionsTringsPairs = MapToDictionary(connectionStrings);
        }

        public ServForOracleModule(Dictionary<T, string> keyNameConnectionStringsPairs)
        {
            if(keyNameConnectionStringsPairs is null || !keyNameConnectionStringsPairs.Any())
            {
                throw new ArgumentNullException(nameof(keyNameConnectionStringsPairs));
            }

            KeyNameConnectionsTringsPairs = keyNameConnectionStringsPairs.ToDictionary(c => (object)c.Key, c => c.Value);
        }
    }

    public class ServForOracleModule : Module
    {
        public virtual Dictionary<object, string> KeyNameConnectionsTringsPairs { get; protected set; }
        protected virtual string SingleConnectionString { get; set; }

        protected ServForOracleModule()
        {
        }

        public ServForOracleModule(string connectionString)
        {
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            SingleConnectionString = connectionString;
        }

        public ServForOracleModule(Dictionary<string, string> keyNameConnectionStringsPairs)
        {
            if(keyNameConnectionStringsPairs is null || !keyNameConnectionStringsPairs.Any())
            {
                throw new ArgumentNullException(nameof(keyNameConnectionStringsPairs));
            }

            KeyNameConnectionsTringsPairs = keyNameConnectionStringsPairs.ToDictionary(c => (object)c.Key, c => c.Value);
        }

        public ServForOracleModule(Dictionary<object, string> keyNameConnectionStringsPairs)
        {
            if(keyNameConnectionStringsPairs is null || !keyNameConnectionStringsPairs.Any())
            {
                throw new ArgumentNullException(nameof(keyNameConnectionStringsPairs));
            }

            KeyNameConnectionsTringsPairs = keyNameConnectionStringsPairs;
        }

        protected override void Load(ContainerBuilder builder)
        {
            LoadInternal(builder);
        }

        internal void LoadInternal(ContainerBuilder builder)
        {
            builder.Register<MemoryCache>(ctx => new MemoryCache(new MemoryCacheOptions()))
                .As<IMemoryCache>()
                .SingleInstance()
                .IfNotRegistered(typeof(IMemoryCache));

            builder.Register<ServForOracleCache>(ctx =>
            {
                var cache = ctx.Resolve<IMemoryCache>();
                return ServForOracleCache.Create(cache);
            })
            .SingleInstance()
            .IfNotRegistered(typeof(ServForOracleCache));

            if (!string.IsNullOrWhiteSpace(SingleConnectionString))
            {
                builder.Register<IDbConnectionFactory>(ctx => new OracleDbConnectionFactory(SingleConnectionString));

                builder.Register(ctx =>
                {
                    var cache = ctx.Resolve<ServForOracleCache>();
                    var logger = ctx.ResolveOptional<ILogger<ServiceForOracle>>();
                    var db = ctx.Resolve<IDbConnectionFactory>();

                    return new ServiceForOracle(logger, cache, db);
                })
                .As<IServiceForOracle>();
            }
            else
            {
                foreach (var keypair in KeyNameConnectionsTringsPairs)
                {
                    builder
                        .Register<OracleDbConnectionFactory>(ctx => new OracleDbConnectionFactory(keypair.Value))
                        .Keyed<IDbConnectionFactory>(keypair.Key);

                    builder.Register(ctx =>
                    {
                        var cache = ctx.Resolve<ServForOracleCache>();
                        var logger = ctx.ResolveOptional<ILogger<ServiceForOracle>>();
                        var db = ctx.ResolveKeyed<IDbConnectionFactory>(keypair.Key);

                        return new ServiceForOracle(logger, cache, db);
                    })
                    .Keyed<IServiceForOracle>(keypair.Key);
                }
            }
        }
    }
}
