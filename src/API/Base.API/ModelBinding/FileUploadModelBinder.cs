using Base.API.Adapters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Base.API.ModelBinding;

/// <summary>
/// CUSTOM MODEL BINDER THAT BINDS AN IFORMFILE FROM THE REQUEST TO AN IFILE UPLOAD PROPERTY.
/// </summary>
public class FileUploadModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var fieldName = bindingContext.FieldName;

        var file = bindingContext.HttpContext.Request.Form.Files.GetFile(fieldName);

        if (file is not null && file.Length > 0)
        {
            bindingContext.Result = ModelBindingResult.Success(new FormFileUploadAdapter(file));
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }

        return Task.CompletedTask;
    }
}
