using App.Services.Services;
using App.Core.Validators;
using App.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.ServiceExtentions
{
    public static class ServiceExtention
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure WKT validation settings
            services.Configure<WktValidationConfig>(options => 
                configuration.GetSection("WktValidation").Bind(options));
            
            // Register validators
            services.AddScoped<GeometryValidator>();
            
            // Register services
            services.AddScoped<IGeometryService, GeometryService>();
            return services;
        }
    }
}
