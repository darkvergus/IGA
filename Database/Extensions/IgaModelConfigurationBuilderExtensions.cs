using Core.Domain.Dynamic;
using Database.Converter;
using Microsoft.EntityFrameworkCore;

namespace Database.Extensions;

public static class IgaModelConfigurationBuilderExtensions
{
    public static ModelConfigurationBuilder UseIgaModel(this ModelConfigurationBuilder builder)
    {
        builder.Properties<IDictionary<string, DynamicAttributeValue>>().HaveConversion<DictionaryJsonConverter>().HaveColumnType("nvarchar(max)");
        return builder;
    }
}