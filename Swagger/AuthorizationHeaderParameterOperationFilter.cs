using Microsoft.AspNetCore.Mvc.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRLCP.Swagger
{
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        //Not In used
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var _filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = _filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = _filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

            if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "Authorization",
                    In = "Header",
                    Description = "Enter Access token to use CLRCP API",
                    Required = true,
                    Type = "string"
                    
                   
                }) ;
            }
        }
    }
}
