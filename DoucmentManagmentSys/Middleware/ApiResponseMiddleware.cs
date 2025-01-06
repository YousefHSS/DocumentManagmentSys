using DoucmentManagmentSys.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using (var newBodyStream = new MemoryStream())
        {
            context.Response.Body = newBodyStream;

            await _next(context);

            context.Response.Body = originalBodyStream;
            newBodyStream.Seek(0, SeekOrigin.Begin);

            var responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();
            var statusCode = context.Response.StatusCode;

            JObject responseBodyObject = null;
            if (IsValidJson(responseBody))
            {
                responseBodyObject = JObject.Parse(responseBody);
            }
            ApiResponse<JObject> apiResponse=  new ApiResponse<JObject>();
            if (!string.IsNullOrEmpty(responseBody))
            {
                if (statusCode >= 200 && statusCode < 300)
                {
                    apiResponse = new ApiResponse<JObject>
                    {
                        Success = true,
                        Data = responseBodyObject,
                        Message = responseBody,
                        Errors = null,
                        StatusCode = statusCode
                    };
                }
                else
                {
                    var errorData= new JObject();
                    try
                    {
                            errorData = JsonConvert.DeserializeObject<JObject>(responseBody);
                    
                    }
                    catch (JsonException)
                    {
                        errorData = (JObject?)responseBody;
                    }

                    apiResponse = new ApiResponse<JObject>
                    {
                        Success = false,
                        Data = null,
                        Message = "Request failed.",
                        Errors = errorData.ContainsKey("errors") ? errorData["errors"] : new JArray { "An unknown error occurred." },
                        StatusCode = statusCode
                    };
                }
            }
            

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
        }
        
    }
    private bool IsValidJson(string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput)) return false;
        strInput = strInput.Trim();
        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || // For object
            (strInput.StartsWith("[") && strInput.EndsWith("]")))   // For array
        {
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception) // some other exception
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
