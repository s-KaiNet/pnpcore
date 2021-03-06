﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Reflection;

namespace PnP.Core.Services
{
    internal class TelemetryManager
    {
        private readonly TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();

        internal TelemetryManager(PnPGlobalSettingsOptions globalOptions)
        {
            telemetryConfiguration.InstrumentationKey = "ffe6116a-bda0-4f0a-b0cf-d26f1b0d84eb";
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
            GlobalOptions = globalOptions;

            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            Version = ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
        }

        /// <summary>
        /// Azure AppInsights Telemetry client
        /// </summary>
        internal TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        /// Settings client
        /// </summary>
        internal PnPGlobalSettingsOptions GlobalOptions { get; private set; }

        /// <summary>
        /// File version of the PnP Core SDK
        /// </summary>
        internal string Version { get; private set; }

        internal void LogServiceRequest(Batch batch, BatchRequest request, PnPContext context)
        {
            TelemetryClient.TrackEvent("PnPCoreRequest", PopulateProperties(request, batch, context));
        }

        private Dictionary<string, string> PopulateProperties(BatchRequest request, Batch batch, PnPContext context)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>(10)
            {
                { "PnPCoreSDKVersion", Version },
                { "AADTenantId", GlobalOptions.AADTenantId.ToString() },
                { "Model", request.Model.GetType().FullName },
                { "ApiType", request.ApiCall.Type.ToString() },
                { "ApiMethod", request.Method.ToString() },
                { "PnPContextId", context.Id.ToString() },
                { "BatchId", batch.Id.ToString() },
                { "BatchRequestId", request.Id.ToString() }

            };

            if (request.Method == HttpMethod.Get)
            {
                var selectProperties = GetSelectedProperties(request.ApiCall);
                if (selectProperties != null)
                {
                    properties.Add("SelectedProperties", selectProperties);
                }
            }

            return properties;
        }

        private static string GetSelectedProperties(ApiCall apiCall)
        {
            if (apiCall.Request.Contains("$select=", StringComparison.InvariantCultureIgnoreCase))
            {
                string queryStringToParse;
                if (apiCall.Type == ApiType.SPORest)
                {
                    // REST API calls are fully qualified
                    queryStringToParse = new Uri(apiCall.Request).Query;
                }
                else
                {
                    // Graph API calls are delivered without the graph endpoint suffix
                    queryStringToParse = new Uri($"{PnPConstants.MicrosoftGraphBaseUrl}{apiCall.Request}").Query;
                }

                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(queryStringToParse);
                return queryString["$select"];
            }

            return null;
        }
    }
}
