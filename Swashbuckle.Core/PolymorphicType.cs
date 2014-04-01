using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Swashbuckle.Core
{
    public class PolymorphicType
    {
        private readonly List<PolymorphicType> _subTypes;
        public PolymorphicType(Type type)
        {
            Type = type;
            _subTypes = new List<PolymorphicType>();
        }

        internal Type Type { get; private set; }

        internal string Discriminator { get; private set; }

        internal IEnumerable<PolymorphicType> SubTypes
        {
            get { return _subTypes; }           
        }

        internal void DiscriminatorBy(string discriminator)
        {
            Discriminator = discriminator;
        }

        internal void RegisterSubType(PolymorphicType subTypeInfo)
        {
            _subTypes.Add(subTypeInfo);
        }

        internal PolymorphicType SearchHierarchyFor(Type type)
        {
            foreach (var directSubType in _subTypes)
            {
                if (directSubType.Type == type) return directSubType;

                var nestedSubType = directSubType.SearchHierarchyFor(type);
                if (nestedSubType != null) return nestedSubType;
            }

            return null;
        }
    }

    public class PolymorphicType<T> : PolymorphicType
    {
        public PolymorphicType()
            : base(typeof(T))
        {
        }

        public PolymorphicType<T> DiscriminateBy(Expression<Func<T, object>> expression)
        {
            DiscriminatorBy(InferDiscriminatorFrom(expression));
            return this;
        }

        public PolymorphicType<T> SubType<TSub>(Action<PolymorphicType<TSub>> configure = null)
        {
            var subTypeInfo = new PolymorphicType<TSub>();
            RegisterSubType(subTypeInfo);

            if (configure != null) configure(subTypeInfo);
            
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