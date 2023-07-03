using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers.CouchDB.Client;
using Orleans.Providers.CouchDB.Configuration;
using Orleans.Providers.CouchDB.Membership;
using Orleans.Providers.CouchDB.Serialization;
using Orleans.Providers.CouchDB.Storage;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Providers.CouchDB
{
	public static class CouchDbSiloExtensions
	{
        /*
        /// <summary>
        /// Configure silo to use CouchDb with a passed in connection string.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="settingsFactory">The settings factory.</param>
        /// <returns></returns>
        public static ISiloBuilder UseCouchDbClient(this ISiloBuilder builder, Func<IServiceProvider, CouchDbGrainStorageOptions> settingsFactory)
        {
            return builder.ConfigureServices(services =>
            {
                if (settingsFactory == null)
                    throw new ArgumentNullException(nameof(settingsFactory));

                services.TryAddSingleton<ICouchDbClient>(provider =>
                {
                    var settings = settingsFactory(provider);
                    var httpFactory = provider.GetRequiredService<IHttpClientFactory>();
                    return new CouchDbClient(httpFactory, settings); ;
                });
            });
        }

        /*

        /// <summary>
        /// Configure ISiloBuilder to use MongoReminderTable.
        /// </summary>
        ///

        public static ISiloBuilder UseCouchDbReminders(this ISiloBuilder builder,
                Action<CouchDbRemindersOptions> configurator = null)
        {
            return builder.ConfigureServices(services => services.AddCouchDbReminders(configurator));
        }

        /// <summary>
        /// Configure ISiloBuilder to use MongoReminderTable
        /// </summary>
        public static ISiloBuilder UseCouchDbReminders(this ISiloBuilder builder,
            IConfiguration configuration)
        {
            return builder.ConfigureServices(services => services.AddCouchDbReminders(configuration));
        }

        /// <summary>
        /// Configure ISiloBuilder to use MongoBasedMembership
        /// </summary>
        public static ISiloBuilder UseCouchDbClustering(this ISiloBuilder builder,
            Action<CouchDbMembershipTableOptions> configurator = null)
        {
            return builder.ConfigureServices(services => services.AddCouchDbMembershipTable(configurator));
        }

        /// <summary>
        /// Configure ISiloBuilder to use MongoMembershipTable
        /// </summary>
        public static ISiloBuilder UseCouchDbMembershipTable(this ISiloBuilder builder,
            IConfiguration configuration)
        {
            return builder.ConfigureServices(services => services.AddCouchDbMembershipTable(configuration));
        }

        /// <summary>
        /// Configure silo to use MongoReminderTable.
        /// </summary>
        public static IServiceCollection AddCouchDbReminders(this IServiceCollection services,
            Action<CouchDbRemindersOptions> configurator = null)
        {
            services.AddReminders();
            services.Configure(configurator ?? (x => { }));
            services.AddSingleton<IReminderTable, MongoReminderTable>();
            services.AddSingleton<IConfigurationValidator, CouchDbGrainStorageOptionsValidator<CouchDbRemindersOptions>>();

            return services;
        }

        /// <summary>
        /// Configure silo to use MongoReminderTable.
        /// </summary>
        public static IServiceCollection AddCouchDbReminders(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddReminders();
            services.Configure<CouchDbRemindersOptions>(configuration);
            services.AddSingleton<IReminderTable, MongoReminderTable>();
            services.AddSingleton<IConfigurationValidator, CouchDbGrainStorageOptionsValidator<CouchDbRemindersOptions>>();

            return services;
        }
        */
        /// <summary>
        /// Configure silo to use MongoMembershipTable.
        /// </summary>
        ///

        public static ISiloBuilder UseCouchDbClustering(this ISiloBuilder builder,
            Action<CouchDbMembershipOptions> configurator)
        {
            return builder.ConfigureServices(services => services.AddCouchDbMembershipTable(configurator));
        }

        public static IServiceCollection AddCouchDbMembershipTable(this IServiceCollection services,
            Action<CouchDbMembershipOptions> configurator)
        {
            services.Configure(configurator);
            //services.AddSingleton<IConfigurationValidator, CouchDbOptionsValidator<CouchDbMembershipOptions>>();
            services.AddSingleton<IMembershipTable, CouchDbMembershipTable>();
          

            return services;
        }

        public static ISiloBuilder UseCouchDbMembershipTable(this ISiloBuilder builder, IConfiguration configuration)
        {
            return builder.ConfigureServices(services => services.AddCouchDbMembershipTable(configuration));
        }

       

        public static IServiceCollection AddCouchDbMembershipTable(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CouchDbMembershipOptions>(configuration);
            services.AddSingleton<IMembershipTable, CouchDbMembershipTable>();
            services.AddSingleton<IConfigurationValidator, CouchDbOptionsValidator<CouchDbMembershipOptions>>();

            return services;
        }

        /// <summary>
        /// Configure silo to use CouchDb for grain storage.
        /// </summary>
        public static ISiloBuilder AddCouchDbGrainStorage(this ISiloBuilder builder, string name,
            Action<CouchDbGrainStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddCouchDbGrainStorage(name, configureOptions));
        }

        /// <summary>
        /// Configure silo to use CouchDb as the default grain storage.
        /// </summary>
        public static ISiloBuilder AddCouchDbGrainStorageAsDefault(this ISiloBuilder builder,
            Action<CouchDbGrainStorageOptions> configureOptions)
        {
            return builder.AddCouchDbGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
        }

        /// <summary>
        /// Configure silo to use CouchDb as the default grain storage.
        /// </summary>
        public static ISiloBuilder AddCouchDbGrainStorageAsDefault(this ISiloBuilder builder,
            Action<OptionsBuilder<CouchDbGrainStorageOptions>> configureOptions)
        {
            return builder.AddCouchDbGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
        }

        /// <summary>
        /// Configure silo to use CouchDb for grain storage.
        /// </summary>
        public static ISiloBuilder AddCouchDbGrainStorage(this ISiloBuilder builder, string name,
            Action<OptionsBuilder<CouchDbGrainStorageOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddCouchDbGrainStorage(name, configureOptions));
        }

        /// <summary>
        /// Configure silo to use CouchDb as the default grain storage.
        /// </summary>
        public static IServiceCollection AddCouchDbGrainStorageAsDefault(this IServiceCollection services,
            Action<CouchDbGrainStorageOptions> configureOptions)
        {
            return services.AddCouchDbGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, ob => ob.Configure(configureOptions));
        }

        /// <summary>
        /// Configure silo to use CouchDb for grain storage.
        /// </summary>
        public static IServiceCollection AddCouchDbGrainStorage(this IServiceCollection services, string name,
            Action<CouchDbGrainStorageOptions> configureOptions)
        {
            return services.AddCouchDbGrainStorage(name, ob => ob.Configure(configureOptions));
        }

        /// <summary>
        /// Configure silo to use CouchDb as the default grain storage.
        /// </summary>
        public static IServiceCollection AddCouchDbGrainStorageAsDefault(this IServiceCollection services,
            Action<OptionsBuilder<CouchDbGrainStorageOptions>> configureOptions)
        {
            return services.AddCouchDbGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
        }

        /// <summary>
        /// Configure silo to use CouchDb for grain storage.
        /// </summary>
        public static IServiceCollection AddCouchDbGrainStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<CouchDbGrainStorageOptions>> configureOptions)
        {
            configureOptions?.Invoke(services.AddOptions<CouchDbGrainStorageOptions>(name));
            services.AddTransient<IPostConfigureOptions<CouchDbGrainStorageOptions>, CouchDbOptionsConfigurator>();

            if (string.Equals(name, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
            {
                services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            }

            //services.TryAddSingleton<IGrainStateSerializer, JsonGrainStateSerializer>();

            services.ConfigureNamedOptionForLogging<CouchDbGrainStorageOptions>(name);

            services.AddTransient<IConfigurationValidator>(sp => new CouchDbOptionsValidator<CouchDbGrainStorageOptions>(sp.GetRequiredService<IOptionsMonitor<CouchDbGrainStorageOptions>>().Get(name), name));
            services.AddSingletonNamedService(name, CouchDbGrainStorageFactory.Create);
            //services.AddSingletonNamedService(name, (s, n) => s.GetRequiredServiceByName<IGrainStorage>(n));

            return services;
        }
    }
}

