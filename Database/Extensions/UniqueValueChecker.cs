using System.Linq.Expressions;
using System.Reflection;
using Core.Interfaces;
using Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Database.Extensions;

public static class UniqueValueChecker
{
    public static async Task<bool> IsStringValueUniqueAsync<TEntity>(this IgaDbContext context, string propertyName, string propertyValue, Guid? excludedEntityId, CancellationToken cancellationToken)
        where TEntity : class, IGuidEntity
    {
        IQueryable<TEntity> query = context.Set<TEntity>();

        if (excludedEntityId.HasValue)
        {
            Guid excludedId = excludedEntityId.Value;
            query = query.Where(entity => entity.Id != excludedId);
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "entity");
        MethodInfo propertyMethodDefinition = GetEfPropertyMethodDefinition();
        MethodInfo propertyMethodInfo = propertyMethodDefinition.MakeGenericMethod(typeof(string));
        ConstantExpression propertyNameExpression = Expression.Constant(propertyName);
        MethodCallExpression propertyCallExpression = Expression.Call(propertyMethodInfo, parameterExpression, propertyNameExpression);
        ConstantExpression valueExpression = Expression.Constant(propertyValue, typeof(string));
        BinaryExpression equalsExpression = Expression.Equal(propertyCallExpression, valueExpression);
        Expression<Func<TEntity, bool>> predicateExpression = Expression.Lambda<Func<TEntity, bool>>(equalsExpression, parameterExpression);

        IQueryable<TEntity> filteredQuery = query.Where(predicateExpression);
        bool exists = await filteredQuery.AnyAsync(cancellationToken).ConfigureAwait(false);
        bool isUnique = !exists;
        return isUnique;
    }

    private static MethodInfo GetEfPropertyMethodDefinition()
    {
        MethodInfo[] methodInfos = typeof(EF).GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (MethodInfo methodInfo in methodInfos)
        {
            if (methodInfo.Name != "Property")
            {
                continue;
            }

            if (!methodInfo.IsGenericMethodDefinition)
            {
                continue;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 2)
            {
                continue;
            }

            return methodInfo;
        }

        throw new InvalidOperationException("EF.Property method not found.");
    }
}