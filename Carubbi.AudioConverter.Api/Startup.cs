using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Carubbi.AudioConverter.Api.Converters;
using Carubbi.AudioConverter.Api.Utilities;
using Carubbi.AudioConverter.Api.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Carubbi.AudioConverter.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carubbi Audio Converter API", Version = "v1" });

            });

            services.AddSingleton(new EnvironmentVariablesConfig());

            services.AddTransient<IConverterSelector, ConverterSelector>();


            services.AddTransient<IConverter, WavToMp3Converter>();
            services.AddTransient<IConverter, Mp3ToWavConverter>();

            services.AddTransient<IConverter, WavToOggConverter>();
            services.AddTransient<IConverter, OggToWavConverter>();

            services.AddTransient<IConverter, OggToMp3Converter>();
            services.AddTransient<IConverter, Mp3ToOggConverter>();
            services.AddTransient<IFileValidator, FileValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carubbi Audio Converter API V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
