using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AssetFlow.Data.Extensions
{
    public static class ModelBuilderExtension
    {
        internal static void ApplyGlobalFilters<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> expression)
        {
            var entities = modelBuilder.Model
                .GetEntityTypes()
                .Where(e => e.ClrType.GetInterface(typeof(TInterface).Name) != null)
                .Select(e => e.ClrType);
            foreach (var entity in entities)
                modelBuilder.Entity(entity).AddQueryFilter(expression);
        }
       
        internal static void AddQueryFilter<T>(this EntityTypeBuilder entityTypeBuilder,
            Expression<Func<T, bool>> expression)
        {
            var parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
            var expressionFilter = ReplacingExpressionVisitor.Replace(
                expression.Parameters.Single(), parameterType, expression.Body);

            var currentQueryFilter = entityTypeBuilder.Metadata.GetQueryFilter();
            if (currentQueryFilter != null)
            {
                var currentExpressionFilter = ReplacingExpressionVisitor.Replace(
                    currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
                expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
            }

            var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
            entityTypeBuilder.HasQueryFilter(lambdaExpression);
        }

    }
}
