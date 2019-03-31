using Autofac;
using Autofac.Builder;
using AutoFixture.Xunit2;
using Moq;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ServForOracle.NetCore.Extensions.Autofac.Tests
{
    public class ServForOracleModuleTests
    {
        public enum TestEnum
        {
            A,
            B
        }

        [Fact]
        public void Constructor_NullArgument()
        {
            Assert.Throws<ArgumentNullException>("connectionString", () => new ServForOracleModule((string)null));
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule((Dictionary<string, string>)null));
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule((Dictionary<object, string>)null));
        }

        [Fact]
        public void Constructor_EmptyArgument()
        {
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule(new Dictionary<string, string>()));
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule(new Dictionary<object, string>()));
        }

        [Fact]
        public void Enum_Constructor_NullArgument()
        {
            Assert.Throws<ArgumentNullException>("connectionStrings", () => new ServForOracleModule<TestEnum>((string[])null));
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule<TestEnum>((Dictionary<TestEnum, string>)null));
        }

        [Fact]
        public void Enum_Constructor_EmptyArgument()
        {
            Assert.Throws<ArgumentNullException>("connectionStrings", () => new ServForOracleModule<TestEnum>(new string[] { }));
            Assert.Throws<ArgumentNullException>("keyNameConnectionStringsPairs", () => new ServForOracleModule<TestEnum>(new Dictionary<TestEnum, string>()));
        }

        [Theory, AutoData]
        public void ConnnectionString(string connectionString)
        {
            var builder = new ContainerBuilder();

            var module = new ServForOracleModule(connectionString);

            module.LoadInternal(builder);

            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

            Assert.True(container.IsRegistered<IServiceForOracle>());
            Assert.True(container.IsRegistered<IDbConnectionFactory>());

            var service = container.Resolve<IServiceForOracle>();

            Assert.NotNull(service);
        }

        [Theory, AutoData]
        public void ConnnectionStringDictionary(Dictionary<string, string> connectionStrings)
        {
            var builder = new ContainerBuilder();

            var module = new ServForOracleModule(connectionStrings);

            module.LoadInternal(builder);

            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

            foreach (var conStr in connectionStrings)
            {
                Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(conStr.Key));
                Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(conStr.Key));
                var service = container.ResolveKeyed<IServiceForOracle>(conStr.Key);

                Assert.NotNull(service);
            }
        }

        [Theory, AutoData]
        public void ConnnectionStringObjectDictionary(Dictionary<object, string> connectionStrings)
        {
            var builder = new ContainerBuilder();

            var module = new ServForOracleModule(connectionStrings);

            module.LoadInternal(builder);

            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

            foreach (var conStr in connectionStrings)
            {
                Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(conStr.Key));
                Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(conStr.Key));
                var service = container.ResolveKeyed<IServiceForOracle>(conStr.Key);

                Assert.NotNull(service);
            }
        }

        [Theory, AutoData]
        public void Enum_ConnnectionStringIEnumerable(string conStringA, string conStringB)
        {
            var builder = new ContainerBuilder();

            var module = new ServForOracleModule<TestEnum>(new[] { conStringA, conStringB });

            module.LoadInternal(builder);

            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

            Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(TestEnum.A));
            Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(TestEnum.A));
            var serviceA = container.ResolveKeyed<IServiceForOracle>(TestEnum.A);

            Assert.NotNull(serviceA);

            Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(TestEnum.B));
            Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(TestEnum.B));
            var serviceB = container.ResolveKeyed<IServiceForOracle>(TestEnum.B);

            Assert.NotNull(serviceB);
        }

        [Theory, AutoData]
        public void Enum_ConnnectionStringDictionary(string conStringA, string conStringB)
        {
            var builder = new ContainerBuilder();

            var module = new ServForOracleModule<TestEnum>(new Dictionary<TestEnum, string>
            {
                { TestEnum.A, conStringA },
                { TestEnum.B, conStringB }
            });

            module.LoadInternal(builder);

            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);

            Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(TestEnum.A));
            Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(TestEnum.A));
            var serviceA = container.ResolveKeyed<IServiceForOracle>(TestEnum.A);

            Assert.NotNull(serviceA);

            Assert.True(container.IsRegisteredWithKey<IServiceForOracle>(TestEnum.B));
            Assert.True(container.IsRegisteredWithKey<IDbConnectionFactory>(TestEnum.B));
            var serviceB = container.ResolveKeyed<IServiceForOracle>(TestEnum.B);

            Assert.NotNull(serviceB);
        }
    }
}
