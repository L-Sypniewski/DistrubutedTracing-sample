using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shared;
using WebApiSecond.Controllers;
using WebApiSecond.EfCore;

namespace WebApiSecond
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
            services.AddHttpClient<SecondController>();
            services.AddDistributedTracing("WebApiSecond");
            services.AddDaprClient();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiSecond", Version = "v1" });
            });

            services.AddDbContext<WebApiSecondDbContext>(options =>
              {
                  options.UseSqlServer(Configuration
                  .GetSection("connectionstrings")
                  .GetSection("sqlserverBlablaDaprName")
                  .GetValue<string>("webApiSecondDbOtherName")
                 );
              }
            );
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebApiSecondDbContext webApiSecondDbContext)
        {
            if (env.IsDevelopment())
            {
                ApplyDbMigration(webApiSecondDbContext);
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiSecond v1"));
            }
            else
            {
                app.UseHttpsRedirection();
            }


            // app.UseTraceProvider();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Needed the waiting logic so that Docker container with SQL server is ready to receive requests
        /// </summary>
        /// <param name="dbContext"></param>
        private static void ApplyDbMigration(DbContext dbContext)
        {
            const int attemptsCount = 10;
            const int delayInMs = 1500;

            for (int i = 0; i < attemptsCount; i++)
            {
                try
                {
                    dbContext.Database.Migrate();
                    Console.WriteLine("Migration succeeded");
                    break;
                }
                catch
                {
                    Console.WriteLine($"Migration failed, attempt {i + 1}/{attemptsCount}");

                    Thread.Sleep(delayInMs);
                }

            }
        }
    }
}
