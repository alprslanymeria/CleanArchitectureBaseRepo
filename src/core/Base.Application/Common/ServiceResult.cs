using System.Net;
using System.Text.Json.Serialization;

namespace App.Application.Common;

public class ServiceResult<T>
{
    public T? Data { get; init; }
    public List<string>? ErrorMessage { get; init; }

    // WE'LL USE THIS TO QUICKLY CHECK WHETHER THE OPERATION WAS SUCCESSFUL OR NOT IN OUR OWN INTERNAL IMPLEMENTATION.
    [JsonIgnore] public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
    [JsonIgnore] public bool IsFail => !IsSuccess;
    [JsonIgnore] public HttpStatusCode Status { get; private set; }
    [JsonIgnore] public string? UrlAsCreated { get; set; }

    // SUCCESS AND DATA TO RETURN
    public static ServiceResult<T> Success(T data, HttpStatusCode status = HttpStatusCode.OK) => new() { Data = data, Status = status };

    // SUCCESS FOR CREATE CRUD OPERATION
    public static ServiceResult<T> SuccessAsCreated(T data, string urlAsCreated) => new() { Data = data, Status = HttpStatusCode.Created, UrlAsCreated = urlAsCreated };

    // FAIL
    public static ServiceResult<T> Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest) => new() { ErrorMessage = errorMessage, Status = status };

    // FAIL BUT ONLY ONE ERROR MESSAGE
    public static ServiceResult<T> Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest) => new() { ErrorMessage = [errorMessage], Status = status };
}


public class ServiceResult
{
    public List<string>? ErrorMessage { get; init; }

    // WE'LL USE THIS TO QUICKLY CHECK WHETHER THE OPERATION WAS SUCCESSFUL OR NOT IN OUR OWN INTERNAL IMPLEMENTATION.
    [JsonIgnore] public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
    [JsonIgnore] public bool IsFail => !IsSuccess;
    [JsonIgnore] public HttpStatusCode Status { get; private set; }


    // SUCCESS AND NO DATA TO RETURN
    public static ServiceResult Success(HttpStatusCode status = HttpStatusCode.OK) => new() { Status = status };

    // FAIL
    public static ServiceResult Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest) => new() { ErrorMessage = errorMessage, Status = status };

    // FAIL BUT ONLY ONE ERROR MESSAGE
    public static ServiceResult Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest) => new() { ErrorMessage = [errorMessage], Status = status };

}
