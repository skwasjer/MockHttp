using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockHttp.Http;

namespace MockHttp.Server;

internal static class ExceptionHandler
{
    internal static IApplicationBuilder UseMockHttpExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = HandleExceptionAsync });
    }

    private static Task HandleExceptionAsync(HttpContext ctx)
    {
        if (ctx.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        ILogger logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ExceptionHandler).FullName!);
        logger.LogRequestMessage(ctx, Resources.Error_VerifyMockSetup);

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

        IHttpResponseFeature? responseFeature = ctx.Features.Get<IHttpResponseFeature>();
        if (responseFeature is not null)
        {
            responseFeature.ReasonPhrase = Resources.Error_VerifyMockSetup;
        }

        ctx.Response.ContentType = $"{MediaTypes.PlainText}; charset={MockHttpHandler.DefaultEncoding.WebName}";
        byte[] body = MockHttpHandler.DefaultEncoding.GetBytes(Resources.Error_VerifyMockSetup + Environment.NewLine + ex);
        return ctx.Response.BodyWriter.WriteAsync(body, ctx.RequestAborted).AsTask();
    }
}
