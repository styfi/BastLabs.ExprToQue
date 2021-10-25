using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BastLabs.ExprToQue
{
    public class ExprToQueService : ExpressionVisitor
    {
        #region "Fields and Properites"

        private StringBuilder sb;
        private string _orderBy = string.Empty;
        private int? _skip;
        private int? _take;
        private string _where = string.Empty;
        private readonly ExprToQueConfig ExprToQueConfig;

        #endregion

        public ExprToQueService(ExprToQueConfig config = null)
        {
            if (config is null)
            {
                config = new ExprToQueConfig();
            }

            ExprToQueConfig = config;
        }

        #region "Privates and internals"

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }


        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
            LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;
            lambdaExpression = (LambdaExpression)ExpressionEvaluator.PartialEval(lambdaExpression);

            if (lambdaExpression.Body is MemberExpression body)
            {
                if (string.IsNullOrEmpty(_orderBy))
                {
                    _orderBy = string.Format("{0} {1}", body.Member.Name, order);
                }
                else
                {
                    _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
                }

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            if (int.TryParse(sizeExpression.Value.ToString(), out int size))
            {
                _take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            if (int.TryParse(sizeExpression.Value.ToString(), out int size))
            {
                _skip = size;
                return true;
            }

            return false;
        }

        #endregion

        #region "Overrides"

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);

                return m;
            }
            else if (m.Method.Name == "Contains")
            {
                MemberExpression listExpr;
                Expression operandExpr;

                if (typeof(IEnumerable).IsAssignableFrom(m.Arguments[0].Type))
                {
                    listExpr = (MemberExpression)m.Arguments[0];
                    operandExpr = m.Arguments[1];
                }
                else
                {
                    operandExpr = m.Arguments[0];
                    listExpr = (MemberExpression)m.Object;
                }
               
                sb.Append("(");
                var resultExpr = Visit(operandExpr);
                sb.Append(" IN (");
                
                resultExpr = Visit(listExpr);  // Visit(((MemberExpression)m.Object).Expression);

                sb.Append("))");

                return resultExpr;
            }
            else if (m.Method.Name == "IsNullOrEmpty")
            {
                sb.Append("Coalesce(");
                var resultExpr = Visit(m.Arguments[0]);
                sb.Append(", '') = ''");

                return resultExpr;
            }
            else if (m.Method.Name == "Take")
            {
                if (ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (ParseOrderByExpression(m, "ASC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (ParseOrderByExpression(m, "DESC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    Visit(u.Operand);

                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);

                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.Coalesce:

                    sb.Append("Coalesce(");
                    Visit(b.Left);
                    sb.Append(", ");
                    Visit(b.Right);
                    sb.Append(")");

                    return b;
            }

            sb.Append("(");
            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }

                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }

                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            Visit(b.Right);
            sb.Append(")");

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.DateTime:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.Object:
                        var type = c.Value.GetType();
                        var fields = type.GetFields();
                        if (fields.Count() == 1
                            && fields[0].FieldType.IsGenericType
                            && fields[0].FieldType.GenericTypeArguments.Count() == 1)
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(fields[0].FieldType))
                            {
                                var itemType = fields[0].FieldType.GenericTypeArguments[0];
                                string list = string.Empty;
                                switch (itemType.Name)
                                {
                                    case "Int16":
                                    case "Int32":
                                    case "Int64":
                                        {
                                            IEnumerable<int> intList = fields[0].GetValue(c.Value) as IEnumerable<int>;
                                            list = string.Join(",", intList.Select(i => i.ToString()));

                                            break;
                                        }
                                    case "String":
                                        {
                                            IEnumerable<string> strList = fields[0].GetValue(c.Value) as IEnumerable<string>;
                                            list = string.Join(",", strList.Select(i => $"'{i}'"));

                                            break;
                                        }
                                }

                                if (string.IsNullOrEmpty(list))
                                {
                                    list = "SELECT 1 WHERE 1=0";
                                }

                                sb.Append(list);
                                return c;
                            }
                        }

                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                if (m.Type.Name != "String" && typeof(IEnumerable).IsAssignableFrom(m.Type))
                {
                    sb.Append("@");
                }

                sb.Append(m.Member.Name);
                return m;
            }

            if (ExprToQueConfig.TranslateEnumerablesAsParameters
                && m.Expression != null && m.Expression.NodeType == ExpressionType.Constant
                && typeof(IEnumerable).IsAssignableFrom(m.Type) 
                && m.Type.Name != "String")
            {
                sb.Append("Select Val from @");
                sb.Append(m.Member.Name);
                return m;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
            {
                return Visit(m.Expression);
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Body != null && node.Body.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCallExp = (MethodCallExpression)node.Body;
                return base.Visit(methodCallExp);

            }

            return base.VisitLambda(node);
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }

        #endregion

        public string Translate(Expression expression)
        {
            sb = new StringBuilder();
            Visit(expression);
            _where = sb.ToString();

            return _where;
        }
    }
}
