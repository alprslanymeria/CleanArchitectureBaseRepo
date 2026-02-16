using Base.Application.Contracts.Infrastructure.Files;

namespace Base.API.Adapters;

/// <summary>
/// ADAPTER CLASS THAT WRAPS IFormFile TO IMPLEMENT IFileUpload ABSTRACTION.
/// THIS ALLOWS THE APPLICATION LAYER TO WORK WITH FILES WITHOUT DEPENDING ON ASP.NET CORE.
/// </summary>
public class FormFileUploadAdapter(IFormFile formFile) : IFileUpload
{
    private readonly IFormFile _formFile = formFile;

    public string FileName => _formFile.FileName;

    public string ContentType => _formFile.ContentType;

    public long Length => _formFile.Length;

    public Stream OpenReadStream() => _formFile.OpenReadStream();
}
