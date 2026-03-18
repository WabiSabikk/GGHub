//using AutoMapper;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using System.Reflection;

//namespace GGHubShared.Extensions
//{
//    public static class AutoMapperExtensions
//    {
//        /// <summary>
//        /// Adds AutoMapper services to the DI container by scanning assemblies for Profile classes
//        /// </summary>
//        /// <param name="services">The service collection</param>
//        /// <param name="assemblyMarkerTypes">Types from assemblies to scan for profiles</param>
//        /// <returns>The service collection for chaining</returns>
//        public static IServiceCollection AddCustomAutoMapper(
//            this IServiceCollection services,
//            params Type[] assemblyMarkerTypes)
//        {
//            if (assemblyMarkerTypes == null || assemblyMarkerTypes.Length == 0)
//            {
//                throw new ArgumentException("At least one assembly marker type must be provided", nameof(assemblyMarkerTypes));
//            }

//            // Register services with factory that creates MapperConfiguration
//            services.AddSingleton<MapperConfiguration>(provider =>
//            {
//                var loggerFactory = provider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
//                return CreateMapperConfiguration(assemblyMarkerTypes, loggerFactory);
//            });

//            services.AddSingleton<IMapper>(provider =>
//            {
//                var config = provider.GetRequiredService<MapperConfiguration>();
//                return config.CreateMapper();
//            });

//            return services;
//        }

//        /// <summary>
//        /// Adds AutoMapper services by scanning specific assemblies
//        /// </summary>
//        /// <param name="services">The service collection</param>
//        /// <param name="assemblies">Assemblies to scan for profiles</param>
//        /// <returns>The service collection for chaining</returns>
//        public static IServiceCollection AddCustomAutoMapper(
//            this IServiceCollection services,
//            params Assembly[] assemblies)
//        {
//            if (assemblies == null || assemblies.Length == 0)
//            {
//                throw new ArgumentException("At least one assembly must be provided", nameof(assemblies));
//            }

//            // Register services with factory that creates MapperConfiguration
//            services.AddSingleton<MapperConfiguration>(provider =>
//            {
//                var loggerFactory = provider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
//                return CreateMapperConfiguration(assemblies, loggerFactory);
//            });

//            services.AddSingleton<IMapper>(provider =>
//            {
//                var config = provider.GetRequiredService<MapperConfiguration>();
//                return config.CreateMapper();
//            });

//            return services;
//        }

//        /// <summary>
//        /// Adds AutoMapper services with custom configuration action
//        /// </summary>
//        /// <param name="services">The service collection</param>
//        /// <param name="configAction">Configuration action</param>
//        /// <returns>The service collection for chaining</returns>
//        public static IServiceCollection AddCustomAutoMapper(
//            this IServiceCollection services,
//            Action<IMapperConfigurationExpression> configAction)
//        {
//            if (configAction == null)
//            {
//                throw new ArgumentNullException(nameof(configAction));
//            }

//            // Register services with factory that creates MapperConfiguration
//            services.AddSingleton<MapperConfiguration>(provider =>
//            {
//                var loggerFactory = provider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
//                var mapperConfig = new MapperConfiguration(configAction, loggerFactory);

//                // Validate configuration
//                mapperConfig.AssertConfigurationIsValid();

//                return mapperConfig;
//            });

//            services.AddSingleton<IMapper>(provider =>
//            {
//                var config = provider.GetRequiredService<MapperConfiguration>();
//                return config.CreateMapper();
//            });

//            return services;
//        }

//        /// <summary>
//        /// Adds AutoMapper services with profiles and custom configuration
//        /// </summary>
//        /// <param name="services">The service collection</param>
//        /// <param name="configAction">Additional configuration action</param>
//        /// <param name="assemblyMarkerTypes">Types from assemblies to scan for profiles</param>
//        /// <returns>The service collection for chaining</returns>
//        public static IServiceCollection AddCustomAutoMapper(
//            this IServiceCollection services,
//            Action<IMapperConfigurationExpression> configAction,
//            params Type[] assemblyMarkerTypes)
//        {
//            if (assemblyMarkerTypes == null || assemblyMarkerTypes.Length == 0)
//            {
//                throw new ArgumentException("At least one assembly marker type must be provided", nameof(assemblyMarkerTypes));
//            }

//            // Register services with factory that creates MapperConfiguration
//            services.AddSingleton<MapperConfiguration>(provider =>
//            {
//                var loggerFactory = provider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();

//                // Create mapper configuration with both profiles and custom action
//                var mapperConfig = new MapperConfiguration(cfg =>
//                {
//                    // Add profiles from assemblies
//                    AddProfilesFromAssemblies(cfg, assemblyMarkerTypes.Select(t => t.Assembly).ToArray());

//                    // Apply custom configuration
//                    configAction?.Invoke(cfg);
//                }, loggerFactory);

//                // Validate configuration
//                mapperConfig.AssertConfigurationIsValid();

//                return mapperConfig;
//            });

//            services.AddSingleton<IMapper>(provider =>
//            {
//                var config = provider.GetRequiredService<MapperConfiguration>();
//                return config.CreateMapper();
//            });

//            return services;
//        }

//        #region Private Helper Methods

//        /// <summary>
//        /// Creates mapper configuration from assembly marker types
//        /// </summary>
//        private static MapperConfiguration CreateMapperConfiguration(Type[] assemblyMarkerTypes, ILoggerFactory loggerFactory)
//        {
//            var assemblies = assemblyMarkerTypes.Select(t => t.Assembly).ToArray();
//            return CreateMapperConfiguration(assemblies, loggerFactory);
//        }

//        /// <summary>
//        /// Creates mapper configuration from assemblies
//        /// </summary>
//        private static MapperConfiguration CreateMapperConfiguration(Assembly[] assemblies, ILoggerFactory loggerFactory)
//        {
//            var mapperConfig = new MapperConfiguration(cfg =>
//            {
//                AddProfilesFromAssemblies(cfg, assemblies);
//            }, loggerFactory);

//            // Validate configuration
//            mapperConfig.AssertConfigurationIsValid();

//            return mapperConfig;
//        }

//        /// <summary>
//        /// Adds all Profile classes from specified assemblies to configuration
//        /// </summary>
//        private static void AddProfilesFromAssemblies(IMapperConfigurationExpression cfg, Assembly[] assemblies)
//        {
//            var profileTypes = new List<Type>();

//            foreach (var assembly in assemblies)
//            {
//                var assemblyProfileTypes = assembly.GetTypes()
//                    .Where(t => typeof(Profile).IsAssignableFrom(t) &&
//                               !t.IsAbstract &&
//                               !t.IsInterface)
//                    .ToList();

//                profileTypes.AddRange(assemblyProfileTypes);
//            }

//            foreach (var profileType in profileTypes)
//            {
//                try
//                {
//                    var profile = Activator.CreateInstance(profileType) as Profile;
//                    if (profile != null)
//                    {
//                        cfg.AddProfile(profile);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw new InvalidOperationException(
//                        $"Failed to create instance of profile type '{profileType.FullName}'. " +
//                        "Make sure the profile has a parameterless constructor.", ex);
//                }
//            }
//        }

//        #endregion
//    }
//}