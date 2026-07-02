using System.Linq.Expressions;

namespace Zlearn.V2.Application.Common.Utils
{
    public class FilterBuilder<TEntity>
    {
        private readonly ParameterExpression _parameter;
        private Expression<Func<TEntity, bool>>? _combined;
        public FilterBuilder()
        {
            _parameter = Expression.Parameter(typeof(TEntity), "e");
        }

        public void AndCondition(Expression<Func<TEntity, bool>> condition)
        {
            // Convert parameter of condition same as _parameter
            var visitor = new ParameterReplaceVisitor(condition.Parameters[0], _parameter);
            var modifiedConditionBody = visitor.Visit(condition.Body);

            // Using AND
            if (_combined == null)
            {
                _combined = Expression.Lambda<Func<TEntity, bool>>(modifiedConditionBody, _parameter);
            }
            else
            {
                var combinedBody = Expression.AndAlso(_combined.Body, modifiedConditionBody);
                _combined = Expression.Lambda<Func<TEntity, bool>>(combinedBody, _parameter);
            }
        }

        public void OrCondition(Expression<Func<TEntity, bool>> condition)
        {
            // Convert parameter of condition same as _parameter
            var visitor = new ParameterReplaceVisitor(condition.Parameters[0], _parameter);
            var modifiedConditionBody = visitor.Visit(condition.Body);

            // Using AND
            if (_combined == null)
            {
                _combined = Expression.Lambda<Func<TEntity, bool>>(modifiedConditionBody, _parameter);
            }
            else
            {
                var combinedBody = Expression.OrElse(_combined.Body, modifiedConditionBody);
                _combined = Expression.Lambda<Func<TEntity, bool>>(combinedBody, _parameter);
            }
        }

        public Expression<Func<TEntity, bool>>? GetPredicate()
        {
            return _combined;
        }

        public Expression<Func<TEntity, bool>> GetPredicateOrDefault()
        {
            return _combined ?? Expression.Lambda<Func<TEntity, bool>>(
                Expression.Constant(true),
                _parameter
            );
        }
    }

    public class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
}

