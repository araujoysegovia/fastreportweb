using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FastReport.Data;
using FastReport.Utils;

namespace FRWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de API web
            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            // Rutas de API web
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
             name: "Index",
             routeTemplate: "{id}.html",
             defaults: new { id = "index" }
             );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
