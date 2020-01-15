using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace PB.ScheduleBot
{
    public static class FunctionUtils
    {
        public static IConfigurationRoot GetConfiguration(ExecutionContext context) => new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

        public static string GetBaseUrl(this HttpRequest req)
        {
            string encodedUrl = req.GetEncodedUrl();
            int indexOfLastSlash = encodedUrl.LastIndexOf('/');
            return indexOfLastSlash < 0 ? encodedUrl : encodedUrl.Substring(0, indexOfLastSlash + 1);
        }
    }
}