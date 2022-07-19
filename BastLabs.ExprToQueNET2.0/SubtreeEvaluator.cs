using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BastLabs.ExprToQue
{
    internal class SubtreeEvaluator : ExpressionVisitor
    {
        private HashSet<Expression> _candidates;

        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            _candidates = candidates;
        }

        #region "Privates and Internals"

        private Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
            {
                return e;
            }

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();

            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
        }

        internal Expression Eval(Expression exp)
        {
            return Visit(exp);
        }

        #endregion

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            if (_candidates.Contains(exp))
            {
                return Evaluate(exp);
            }

            return base.Visit(exp);
        }
    }
}
