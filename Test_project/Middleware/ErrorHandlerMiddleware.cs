using FluentValidation.TestHelper;
using System.Net;
using System.Text.Json;
using Test_project.Context;
using Test_project.ViewModel;

namespace Test_project.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,TestDbContext _sqlDB,SqliteDbContext _sqDB)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                await HandleExceptionAsync(context,error); 
            }
        }

        private async Task HandleExceptionAsync(HttpContext context,Exception error)
        {
            HttpResponse? response=context.Response;
            response.ContentType = "application/json";

            ResponseViewModel? responseViewModel = new ResponseViewModel
            {
                Instance = context.Request.Path
            };
            switch (error)
            {
                case UnauthorizedAccessException unauthorizedAccessEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    responseViewModel.StatusCode = (int)HttpStatusCode.Unauthorized;
                    responseViewModel.Message= error.Message;
                    responseViewModel.Detail = unauthorizedAccessEx.Message;
                    await response.WriteAsync(JsonSerializer.Serialize(responseViewModel));
                    break;

                case InvalidOperationException invalidOperationEx:
                    response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    responseViewModel.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    responseViewModel.Message= error.Message;
                    responseViewModel.Detail=invalidOperationEx.Message; 
                    await response.WriteAsync(JsonSerializer.Serialize(responseViewModel));
                    break;

                case Exception ex:
                    response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    responseViewModel.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    responseViewModel.Message = error.Message;
                    responseViewModel.Detail = ex.Message;
                    await response.WriteAsync(JsonSerializer.Serialize(responseViewModel));
                    break;

            }
        }
    }
    public static class ErrorHandlerMiddlewareExtension
    {
        public static IApplicationBuilder UseErrorhandler(this IApplicationBuilder builder) 
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
