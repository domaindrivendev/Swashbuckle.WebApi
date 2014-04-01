using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Swashbuckle.Core
{
    public class PolymorphicType
    {
        private readonly bool _isBase;
        private readonly List<PolymorphicType> _subTypes;

        public PolymorphicType(Type type, bool isBase)
        {
            Type = type;
            _isBase = isBase;
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
            if (!_isBase)
                throw new InvalidOperationException("Invalid Swashbuckle configuration. Discriminator can only be applied to the base class");

            Discriminator = discriminator;
        }

        internal void RegisterSubType(PolymorphicType subTypeInfo)
        {
            if (_isBase && Discriminator == null)
                throw new InvalidOperationException("Invalid Swashbuckle configuration. Discriminator must be set on the base class ");

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
        public PolymorphicType(bool isBase)
            : base(typeof(T), isBase)
        {}

        public PolymorphicType<T> SubType<TSub>(Action<PolymorphicType<TSub>> configure = null)
        {
            var subTypeInfo = new PolymorphicType<TSub>(false);
            RegisterSubType(subTypeInfo);

            if (configure != null) configure(subTypeInfo);
            
            return this;
        }

    }

    public class BasePolymorphicType<T> : PolymorphicType<T>
    {
        public BasePolymorphicType()
            : base(true)
        {
        }

        public PolymorphicType<T> DiscriminateBy(Expression<Func<T, object>> expression)
        {
            DiscriminatorBy(InferDiscriminatorFrom(expression));
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