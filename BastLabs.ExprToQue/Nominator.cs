using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BastLabs.ExprToQue
{
    internal class Nominator : ExpressionVisitor
    {
        private readonly Func<Expression, bool> FnCanBeEvaluated;
        private HashSet<Expression> _candidates;
        private bool _cannotBeEvaluated;

        internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
        {
            FnCanBeEvaluated = fnCanBeEvaluated;
        }

        internal HashSet<Expression> Nominate(Expression expression)
        {
            _candidates = new HashSet<Expression>();
            Visit(expression);

            return _candidates;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = _cannotBeEvaluated;
                _cannotBeEvaluated = false;
                base.Visit(expression);

                if (!_cannotBeEvaluated)
                {
                    if (FnCanBeEvaluated(expression))
                    {
                        _candidates.Add(expression);
                    }
                    else
                    {
                        _cannotBeEvaluated = true;
                    }
                }

                _cannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }
    }
}
