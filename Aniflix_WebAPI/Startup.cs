using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SeleniumBrowserStdLib;
using Microsoft.EntityFrameworkCore;
using Aniflix_WebAPI.Models;
using Microsoft.Extensions.Configuration;

namespace Aniflix_WebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //services.AddDbContext<EpisodeContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services.AddMvc()
            //  .AddXmlDataContractSerializerFormatters() ;

            services.AddMvc()
                // Necessary to avoid issues with infinite loops of json serialization
                // see https://docs.microsoft.com/en-us/ef/core/querying/related-data
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    );

            //services.AddDbContext<AniContext>(options => options.UseSqlite("DataSource=:memory:"));
            services.AddDbContext<AniContext>(options => options.UseInMemoryDatabase("Ani"));
            //services.AddDbContext<EpisodeContext>(options => options.UseInMemoryDatabase("Episodes"));
            //services.AddDbContext<AnimeContext>(options => options.UseInMemoryDatabase("Animes"));
            
            // CORS are necessary for cross domain web sites communications
            // see https://docs.microsoft.com/en-us/aspnet/core/security/cors
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseMvc();
            app.Run(async (context) =>
            {
                //await context.Response.WriteAsync(BrowserManager.Browse());

                await context.Response.WriteAsync("MVC did not find anything!");
            });
        }
    }
}
