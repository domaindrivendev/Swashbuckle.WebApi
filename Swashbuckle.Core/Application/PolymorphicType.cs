using System;
using System.Linq.Expressions;

namespace Swashbuckle.Application
{
    public class PolymorphicType<T> : PolymorphicType
    {
        public PolymorphicType(bool isBase)
            : base(typeof(T), isBase)
        { }

        public PolymorphicType<T> SubType<TSub>(Action<PolymorphicType<TSub>> configure = null)
            where TSub : T
        {
            var subTypeInfo = new PolymorphicType<TSub>(false);
            RegisterSubType(subTypeInfo);

            if (configure != null) configure(subTypeInfo);

            return this;
        }
    }

    public class PolymorphicBaseType<T> : PolymorphicType<T>
    {
        public PolymorphicBaseType()
            : base(true)
        {
        }

        public PolymorphicType<T> DiscriminateBy(Expression<Func<T, object>> expression)
        {
            DiscriminateBy(InferDiscriminatorFrom(expression));
            return this;
        }

        private static string InferDiscriminatorFrom(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpression;

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
                memberExpression = unaryExpression.Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException(String.Format("Failed to infer discriminator from provided expression - {0}", expression));

            return memberExpression.Member.Name;
        }
    }
}