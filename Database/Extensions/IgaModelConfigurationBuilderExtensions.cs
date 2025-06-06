using Microsoft.EntityFrameworkCore;

namespace Database.Extensions;

public static class IgaModelConfigurationBuilderExtensions
{
    public static ModelConfigurationBuilder UseIgaModel(this ModelConfigurationBuilder builder) => builder;
}