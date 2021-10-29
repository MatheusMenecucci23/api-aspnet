
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alura.WebAPI.WebApp.Formatters;
using System;
using Alura.ListaLeitura.HttpClients;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Alura.ListaLeitura.WebApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //configurando o serviço do http accessor
            services.AddHttpContextAccessor();

            //adicionando autenticação por cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Usuario/Login";
            });

            services.AddHttpClient<AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/api/");
            });


            services.AddMvc(options => {
                options.OutputFormatters.Add(new LivroCsvFormatter());
            }).AddXmlSerializerFormatters();
            
            services.AddHttpClient<LivroApiClient>(client  => {
                client.BaseAddress = new Uri("http://localhost:6000/api/v1.0/");
            });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
