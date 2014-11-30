﻿using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Application;
using System.Net.Http;
using System.Collections.Generic;

namespace Swashbuckle.Application
{
    public static class HttpConfigurationExtensions
    {
        private static readonly string DefaultRouteTemplate = "swagger/docs/{apiVersion}";

        public static SwaggerEnabledConfiguration EnableSwagger(
            this HttpConfiguration httpConfig,
            Action<SwaggerDocsConfig> configure = null)
        {
            return EnableSwagger(httpConfig, DefaultRouteTemplate, null, configure);
        }

        public static SwaggerEnabledConfiguration EnableSwagger(
            this HttpConfiguration httpConfig,
            string routeTemplate,
            object defaults,
            Action<SwaggerDocsConfig> configure = null)
        {
            var config = new SwaggerDocsConfig();
            if (configure != null) configure(config);

            httpConfig.Routes.MapHttpRoute(
                "swagger_docs",
                routeTemplate,
                defaults,
                new { apiVersion = @".+" },
                new SwaggerDocsHandler(config)
            );

            return new SwaggerEnabledConfiguration(
                httpConfig,
                config.GetRootUrlResolver(),
                config.GetApiVersions().Select(version => routeTemplate.Replace("{apiVersion}", version)));
        }

        internal static IContractResolver GetJsonContractResolver(this HttpConfiguration configuration)
        {
            var formatter = configuration.Formatters.JsonFormatter;
            return (formatter != null)
                ? formatter.SerializerSettings.ContractResolver
                : new DefaultContractResolver();
        }
    }


    public class SwaggerEnabledConfiguration
    {
        private static readonly string DefaultRouteTemplate = "swagger/ui/{*assetPath}";

        private readonly HttpConfiguration _httpConfig;
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;
        private readonly IEnumerable<string> _discoveryPaths;

        public SwaggerEnabledConfiguration(
            HttpConfiguration httpConfig,
            Func<HttpRequestMessage, string> rootUrlResolver,
            IEnumerable<string> discoveryPaths)
        {
            _httpConfig = httpConfig;
            _rootUrlResolver = rootUrlResolver;
            _discoveryPaths = discoveryPaths;
        }

        public void EnableSwaggerUi(Action<SwaggerUiConfig> configure = null)
        {
            EnableSwaggerUi(DefaultRouteTemplate, null, configure);
        }

        public void EnableSwaggerUi(
            string routeTemplate,
            object defaults,
            Action<SwaggerUiConfig> configure = null)
        {
            var config = new SwaggerUiConfig(_rootUrlResolver, _discoveryPaths);
            if (configure != null) configure(config);

            _httpConfig.Routes.MapHttpRoute(
                "swagger_ui",
                routeTemplate,
                defaults,
                new { assetPath = @".+" },
                new SwaggerUiHandler(config)
            );

            if (routeTemplate == DefaultRouteTemplate)
            {
                _httpConfig.Routes.MapHttpRoute(
                    "swagger_ui_shortcut",
                    "swagger",
                    null,
                    null,
                    new RedirectHandler(_rootUrlResolver, "swagger/ui/index.html"));
            }
        }
    }
}