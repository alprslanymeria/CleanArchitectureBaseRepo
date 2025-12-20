using System.Net;
using System.Text.Json.Serialization;

namespace Base.Application;

public class ServiceResult<T> where T : class
{
    public T? Data { get; private set; }
    public List<string>? ErrorMessage { get; private set; }

    // BUNU KENDİ İÇ YAPIMIZDA KISA YOLDAN İŞLEMİN BAŞARILI OLUP OLMADIĞINI KONTROL ETMEK İÇİN KULLANACAĞIZ.
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
    public List<string>? ErrorMessage { get; private set; }

    // BUNU KENDİ İÇ YAPIMIZDA KISA YOLDAN İŞLEMİN BAŞARILI OLUP OLMADIĞINI KONTROL ETMEK İÇİN KULLANACAĞIZ.
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