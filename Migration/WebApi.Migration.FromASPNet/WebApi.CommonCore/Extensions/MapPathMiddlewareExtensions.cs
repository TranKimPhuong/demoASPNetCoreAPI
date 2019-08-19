using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.CommonCore.Extensions
{
    public static class MapPathMiddlewareExtensions
    {
        public static IApplicationBuilder UseMapMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MapMiddleware>();
        }
    }
}
