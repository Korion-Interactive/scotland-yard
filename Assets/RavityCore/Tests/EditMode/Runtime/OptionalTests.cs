using System.Collections.Generic;
using NUnit.Framework;

namespace Ravity.OptionalTests
{
    public class DefaultInitialization : OptionalTestsBase
    {
        [TestCaseSource(nameof(OptionalTypes))]
        public void OptionalInstance_IsNotNull<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> defaultOptional = default;
            Assert.That(defaultOptional, Is.Not.Null, nameof(defaultOptional));
        }
            
        [TestCaseSource(nameof(OptionalTypes))]
        public void HasValue_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> defaultOptional = default;
            Assert.That(defaultOptional.HasValue, Is.False, nameof(defaultOptional.HasValue));
        }
            
        [TestCaseSource(nameof(OptionalTypes))]
        public void ValueOr_ReturnsFallback<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> defaultOptional = default;
            T valueOrResult = defaultOptional.ValueOr(type.FallbackValue);
            Assert.That(valueOrResult, Is.EqualTo(type.FallbackValue), nameof(valueOrResult));
        }
            
        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGet_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> defaultOptional = default;
            bool tryGetResult = defaultOptional.TryGet(out T _);
            Assert.That(tryGetResult, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToBaseType_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> defaultOptional = default;
            bool result = defaultOptional.TryGetCast(out object _);
            Assert.That(result, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToInheritedType_ReturnsFalse<TInherited>(TInherited inherited) where TInherited : IOptionalType<TInherited>
        {
            Optional<object> optional = default;
            bool result = optional.TryGetCast(out TInherited _);
            Assert.That(result, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToIndependentType_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> optional = default;
            bool result = optional.TryGetCast(out string _);
            Assert.That(result, Is.False);
        }
    }

    public class FromValueOfSameType : OptionalTestsBase
    {
        [TestCaseSource(nameof(OptionalTypes))]
        public void OptionalInstance_IsNotNull<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            Assert.That(valueOptional, Is.Not.Null, nameof(valueOptional));
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void HasValue_ReturnsTrue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            Assert.That(valueOptional.HasValue, Is.True, nameof(valueOptional.HasValue));
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void ValueOr_ReturnsValue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            T valueOrResult = valueOptional.ValueOr(type.FallbackValue);
            Assert.That(valueOrResult, Is.EqualTo(type.InitializationValue), nameof(valueOrResult));
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGet_ReturnsTrue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            bool tryGetResult = valueOptional.TryGet(out T _);
            Assert.That(tryGetResult, Is.True);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGet_OutParameterIsValue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            valueOptional.TryGet(out T value);
            Assert.That(value, Is.EqualTo(type.InitializationValue));
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToBaseType_ReturnsTrue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            bool result = valueOptional.TryGetCast(out object _);
            Assert.That(result, Is.True);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToBaseType_OutParameterIsValue<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> valueOptional = Optional.From(type.InitializationValue);
            valueOptional.TryGetCast(out object value);
            Assert.That(value, Is.EqualTo(type.InitializationValue));
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToInheritedType_ReturnsFalse<TInherited>(TInherited inherited) where TInherited : IOptionalType<TInherited>
        {
            Optional<object> optional = Optional.From(new object());
            bool result = optional.TryGetCast(out TInherited _);
            Assert.That(result, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToIndependentType_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> optional = Optional.From(type.InitializationValue);
            bool result = optional.TryGetCast(out string _);
            Assert.That(result, Is.False);
        }
    }

    public class FromValueOfInheritedType : OptionalTestsBase
    {
        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToInheritedType_ReturnsTrue<TInherited>(TInherited inherited) where TInherited : IOptionalType<TInherited>
        {
            Optional<object> optional = new Optional<object>(inherited.InitializationValue);
            bool result = optional.TryGetCast(out TInherited _);
            Assert.That(result, Is.True);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToInheritedType_OutParameterIsValue<TInherited>(TInherited inherited) where TInherited : IOptionalType<TInherited>
        {
            Optional<object> optional = new Optional<object>(inherited.InitializationValue);
            optional.TryGetCast(out TInherited value);
            Assert.That(value, Is.EqualTo(inherited.InitializationValue));
        }
    }
    
    public class FromNull : OptionalTestsBase
    {
        [Test]
        public void OptionalInstance_IsNotNull()
        {
            Optional<ReferenceType> nullOptional = new Optional<ReferenceType>(null);
            Assert.That(nullOptional, Is.Not.Null, nameof(nullOptional));
        }
            
        [Test]
        public void HasValue_ReturnsFalse()
        {
            Optional<ReferenceType> nullOptional = new Optional<ReferenceType>(null);
            Assert.That(nullOptional.HasValue, Is.False, nameof(nullOptional.HasValue));
        }
            
        [Test]
        public void ValueOr_ReturnsFallback()
        {
            Optional<ReferenceType> nullOptional = new Optional<ReferenceType>(null);
            ReferenceType fallbackValue = new ReferenceType().FallbackValue;
            ReferenceType valueOrResult = nullOptional.ValueOr(fallbackValue);
            Assert.That(valueOrResult, Is.EqualTo(fallbackValue), nameof(valueOrResult));
        }
            
        [Test]
        public void TryGet_ReturnsFalse()
        {
            Optional<ReferenceType> nullOptional = new Optional<ReferenceType>(null);
            bool tryGetResult = nullOptional.TryGet(out ReferenceType _);
            Assert.That(tryGetResult, Is.False);
        }

        [Test]
        public void TryGetCast_ToBaseType_ReturnsFalse()
        {
            Optional<ReferenceType> nullOptional = new Optional<ReferenceType>(null);
            bool result = nullOptional.TryGetCast(out object _);
            Assert.That(result, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToInheritedType_ReturnsFalse<TInherited>(TInherited inherited) where TInherited : IOptionalType<TInherited>
        {
            Optional<object> nullOptional = new Optional<object>(null);
            bool result = nullOptional.TryGetCast(out TInherited _);
            Assert.That(result, Is.False);
        }

        [TestCaseSource(nameof(OptionalTypes))]
        public void TryGetCast_ToIndependentType_ReturnsFalse<T>(T type) where T : IOptionalType<T>
        {
            Optional<T> optional = default;
            bool result = optional.TryGetCast(out string _);
            Assert.That(result, Is.False);
        }
    }

    public class OptionalTestsBase
    {
        protected static IEnumerable<TestCaseData> OptionalTypes
        {
            get
            {
                yield return new TestCaseData(new ValueType())
                    .SetName(nameof(ValueType));
                
                yield return new TestCaseData(new ReferenceType())
                    .SetName(nameof(ReferenceType));
            }
        }
            
        public interface IOptionalType<T>
        {
            T FallbackValue { get; }
            T InitializationValue { get; }
        }
        
        public class ReferenceType : IOptionalType<ReferenceType>
        {
            private static ReferenceType _fallbackValue = new ReferenceType();
            public ReferenceType FallbackValue => _fallbackValue;
            
            private static ReferenceType _initializationValue = new ReferenceType();
            public ReferenceType InitializationValue => _initializationValue;
        }
        
        public struct ValueType : IOptionalType<ValueType>
        {
#pragma warning disable 414
            private int _payload;
#pragma warning restore 414

            public ValueType FallbackValue => new ValueType { _payload = 37 };
            public ValueType InitializationValue => new ValueType { _payload = 1042 };
        }
    }
}
