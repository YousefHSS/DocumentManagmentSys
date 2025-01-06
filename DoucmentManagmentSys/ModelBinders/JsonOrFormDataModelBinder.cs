using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DoucmentManagmentSys.ModelBinders
{
    public class JsonOrFormDataModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            

            var request = bindingContext.HttpContext.Request;

            if (request.ContentType == "application/json")
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    var model = System.Text.Json.JsonSerializer.Deserialize(body, bindingContext.ModelType);
                    bindingContext.Result = ModelBindingResult.Success(model);
                }
            }
            else if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                var model = Activator.CreateInstance(bindingContext.ModelType);

                foreach (var property in bindingContext.ModelType.GetProperties())
                {
                    if (form.TryGetValue(property.Name, out var value))
                    {
                        property.SetValue(model, Convert.ChangeType(value.ToString(), property.PropertyType));
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(model);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
