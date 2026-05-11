using System;
using System.Text;
using Fiscalapi.Abstractions;
using Fiscalapi.Common;
using Fiscalapi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FiscalApi
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 1) M�todo de extensi�n que permite configurar <see cref="FiscalapiSettings"/> 
        /// directamente mediante una expresi�n lambda.
        /// </summary>
        /// <param name="services">Contenedor de dependencias</param>
        /// <param name="configureSettings">Acci�n de configuraci�n para <see cref="FiscalapiSettings"/></param>
        /// <returns></returns>
        public static IServiceCollection AddFiscalApi(
            this IServiceCollection services,
            Action<FiscalapiSettings> configureSettings)
        {
            // Registra la configuraci�n (Action<FiscalApiOptions>)
            services.Configure(configureSettings);

            // Registra IFiscalApiClient con alcance 'Scoped'
            services.AddScoped<IFiscalApiClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<FiscalapiSettings>>().Value;
                return FiscalApiClient.Create(settings);
            });

            return services;
        }

        /// <summary>
        /// 2) M�todo de extensi�n que lee la configuraci�n directamente de la secci�n 
        /// "FiscalapiSettings" del archivo de configuraci�n (appsettings.json).
        /// Lanza una excepci�n si la secci�n no existe o est� vac�a.
        /// </summary>
        /// <param name="services">Contenedor de dependencias</param>
        /// <returns></returns>
        public static IServiceCollection AddFiscalApi(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                if (configuration == null)
                    throw new InvalidOperationException("No se pudo obtener IConfiguration del contenedor...");

                const string defaultSectionName = "FiscalapiSettings";
                var configSection = configuration.GetSection(defaultSectionName);

                if (!configSection.Exists())
                    throw new InvalidOperationException(
                        $"No se encontr� la secci�n '{defaultSectionName}' en la configuraci�n. " +
                        "Verifica que exista en tu appsettings.json.");


                services.Configure<FiscalapiSettings>(options => configSection.Bind(options));
            }

            // Registra IFiscalApiClient como Scoped
            services.AddScoped<IFiscalApiClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<FiscalapiSettings>>().Value;
                return FiscalApiClient.Create(settings);
            });

            return services;
        }


        /// <summary>
        /// Encode string to base64
        /// </summary>
        /// <param name="plainText">string to encode</param>
        /// <returns>base64 encoded string</returns>
        public static string EncodeToBase64(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decode string to plainText
        /// </summary>
        /// <param name="base64EncodedData">base64 encoded data to decode</param>
        /// <returns>plainText</returns>
        public static string DecodeFromBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
