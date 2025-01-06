using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;

namespace DoucmentManagmentSys.ModelBinders
{

    public class JsonOrFormDataModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var loggerFactory = (ILoggerFactory)context.Services.GetService(typeof(ILoggerFactory));
            return new BinderTypeModelBinder(typeof(JsonOrFormDataModelBinder));
        }
    }

}
