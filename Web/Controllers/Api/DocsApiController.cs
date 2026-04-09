using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/docs")]
    public class DocsApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDocs()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var controllers = assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            var result = new List<object>();

            foreach (var controller in controllers)
            {
                var routeAttr = controller.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
                var controllerName = controller.Name.Replace("Controller", "");
                var controllerRoute = routeAttr?.Template ?? controllerName;

                var methods = controller.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m =>
                        m.GetCustomAttributes<HttpMethodAttribute>().Any() ||
                        m.GetCustomAttributes<HttpGetAttribute>().Any() ||
                        m.GetCustomAttributes<HttpPostAttribute>().Any() ||
                        m.GetCustomAttributes<HttpPutAttribute>().Any() ||
                        m.GetCustomAttributes<HttpDeleteAttribute>().Any());

                foreach (var method in methods)
                {
                    var httpMethod = "GET";
                    var methodRoute = "";

                    var httpGet = method.GetCustomAttributes<HttpGetAttribute>().FirstOrDefault();
                    var httpPost = method.GetCustomAttributes<HttpPostAttribute>().FirstOrDefault();
                    var httpPut = method.GetCustomAttributes<HttpPutAttribute>().FirstOrDefault();
                    var httpDelete = method.GetCustomAttributes<HttpDeleteAttribute>().FirstOrDefault();

                    if (httpGet != null) { httpMethod = "GET"; methodRoute = httpGet.Template ?? ""; }
                    else if (httpPost != null) { httpMethod = "POST"; methodRoute = httpPost.Template ?? ""; }
                    else if (httpPut != null) { httpMethod = "PUT"; methodRoute = httpPut.Template ?? ""; }
                    else if (httpDelete != null) { httpMethod = "DELETE"; methodRoute = httpDelete.Template ?? ""; }

                    
                    string fullRoute;
                    if (controllerRoute.StartsWith("api/"))
                    {
                        
                        fullRoute = methodRoute.Length > 0
                            ? $"/{controllerRoute}/{methodRoute}"
                            : $"/{controllerRoute}";
                    }
                    else
                    {
                        
                        fullRoute = $"/{controllerName}/{method.Name}";
                    }

                    var parameters = method.GetParameters()
                        .Where(p => p.ParameterType != typeof(CancellationToken))
                        .Select(p => new
                        {
                            name = p.Name,
                            type = GetFriendlyTypeName(p.ParameterType),
                            source = p.GetCustomAttribute<FromBodyAttribute>() != null ? "body" :
                                     p.GetCustomAttribute<FromQueryAttribute>() != null ? "query" : "route"
                        });

                    result.Add(new
                    {
                        controller = controllerName,
                        action = method.Name,
                        httpMethod,
                        route = fullRoute,
                        parameters,
                        requiresAuth = controller.BaseType == typeof(BaseApiController),
                        returns = GetFriendlyTypeName(method.ReturnType)
                    });
                }
            }

            return Ok(new
            {
                title = "BikeSharing API",
                version = "1.0",
                generatedAt = DateTime.UtcNow,
                endpoints = result.OrderBy(e => ((dynamic)e).route)
            });
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
                var baseName = type.Name.Split('`')[0];
                return $"{baseName}<{genericArgs}>";
            }
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(decimal)) return "decimal";
            return type.Name;
        }
    }
}
