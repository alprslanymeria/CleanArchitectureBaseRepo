using Base.Application.Contracts.Infrastructure.Files;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.API.ModelBinding;

/// <summary>
/// PROVIDES THE FILE UPLOAD MODEL BINDER FOR IFILE UPLOAD TYPED PROPERTIES.
/// </summary>
public class FileUploadModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(IFileUpload))
        {
            return new FileUploadModelBinder();
        }

        return null;
    }
}