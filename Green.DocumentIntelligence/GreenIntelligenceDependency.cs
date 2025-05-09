using Azure;
using Green.DocumentIntelligence.Models;
using Green.DocumentIntelligence.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.DocumentIntelligence
{
    public static class Extensions
    {
        public static IServiceCollection AddGreenIntelligence(this IServiceCollection services, Action<GreenIntelligenceOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new GreenIntelligenceOptions();
            configureOptions(options);

            services.AddAzureClients(o =>
            {
                o.AddDocumentAnalysisClient(new Uri(options.Endpoint),
                    new AzureKeyCredential(options.Token));
            });
            services.AddSingleton(options);
            services.AddSingleton<IGreenIntelligenceService, GreenIntelligenceService>();

            return services;
        }
    }
}
