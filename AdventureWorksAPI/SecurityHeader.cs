using Microsoft.Extensions.Primitives;

namespace AdventureWorksAPI
{
    public class SecurityHeader
    {
        private readonly RequestDelegate next;

        public SecurityHeader(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("super-secure", new StringValues("enable"));
            return next(context);
        }

    }
}
