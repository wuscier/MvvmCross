﻿using MvvmCross.Platform.Core;
using MvvmCross.Test.Mocks.Dispatchers;

namespace MvvmCross.Binding.Test.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Moq;

    using MvvmCross.Binding.Bindings;
    using MvvmCross.Binding.Bindings.Source;
    using MvvmCross.Binding.Bindings.Source.Construction;
    using MvvmCross.Binding.Bindings.SourceSteps;
    using MvvmCross.Binding.Bindings.Target;
    using MvvmCross.Binding.Bindings.Target.Construction;
    using MvvmCross.Platform.Converters;
    using MvvmCross.Platform.Exceptions;
    using MvvmCross.Test.Core;

    using NUnit.Framework;

    [TestFixture]
    public class MvxFullBindingValueConversionTest : MvxIoCSupportingTest
    {
        public class MockSourceBinding : IMvxSourceBinding
        {
            public MockSourceBinding()
            {
                this.SourceType = typeof(object);
            }

            public int DisposeCalled = 0;

            public void Dispose()
            {
                this.DisposeCalled++;
            }

            public Type SourceType { get; set; }

            public List<object> ValuesSet = new List<object>();

            public void SetValue(object value)
            {
                this.ValuesSet.Add(value);
            }

            public void FireSourceChanged()
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler Changed;

            public bool TryGetValueResult;
            public object TryGetValueValue;

            public object GetValue()
            {
                if (!this.TryGetValueResult)
                    return MvxBindingConstant.UnsetValue;

                return this.TryGetValueValue;
            }
        }

        public class MockTargetBinding : IMvxTargetBinding
        {
            public MockTargetBinding()
            {
                this.TargetType = typeof(object);
            }

            public int DisposeCalled = 0;

            public void Dispose()
            {
                this.DisposeCalled++;
            }

            public void SubscribeToEvents()
            { }

            public Type TargetType { get; set; }
            public MvxBindingMode DefaultMode { get; set; }

            public List<object> Values = new List<object>();

            public void SetValue(object value)
            {
                this.Values.Add(value);
            }

            public void FireValueChanged(MvxTargetChangedEventArgs args)
            {
                ValueChanged?.Invoke(this, args);
            }

            public event EventHandler<MvxTargetChangedEventArgs> ValueChanged;
        }

        public class MockValueConverter : IMvxValueConverter
        {
            public object ConversionResult;
            public bool ThrowOnConversion;

            public List<object> ConversionsRequested = new List<object>();
            public List<object> ConversionParameters = new List<object>();
            public List<Type> ConversionTypes = new List<Type>();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                this.ConversionsRequested.Add(value);
                this.ConversionParameters.Add(parameter);
                this.ConversionTypes.Add(targetType);
                if (this.ThrowOnConversion)
                    throw new MvxException("Conversion throw requested");
                return this.ConversionResult;
            }

            public object ConversionBackResult;
            public bool ThrowOnConversionBack;
            public List<object> ConversionsBackRequested = new List<object>();
            public List<object> ConversionBackParameters = new List<object>();
            public List<Type> ConversionBackTypes = new List<Type>();

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                this.ConversionsBackRequested.Add(value);
                this.ConversionBackParameters.Add(parameter);
                this.ConversionBackTypes.Add(targetType);
                if (this.ThrowOnConversionBack)
                    throw new MvxException("Conversion throw requested");
                return this.ConversionBackResult;
            }
        }

        [Test]
        public void TestConverterIsUsedForConvert()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "Test ConversionResult",
            };
            var parameter = new { Ignored = 12 };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("Test ConversionResult", mockTarget.Values[0]);
            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual("TryGetValueValue", mockValueConverter.ConversionsRequested[0]);
        }

        [Test]
        public void TestConverterParameterIsUsedForConvert()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "Test ConversionResult",
            };
            var parameter = new { Ignored = 12 };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("Test ConversionResult", mockTarget.Values[0]);
            Assert.AreEqual(1, mockValueConverter.ConversionParameters.Count);
            Assert.AreEqual(parameter, mockValueConverter.ConversionParameters[0]);
        }

        [Test]
        public void TestConverterIsUsedForConvertBack()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionBackResult = "Test ConversionBackResult",
            };
            var parameter = new { Ignored = 12 };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, typeof(object), out mockSource, out mockTarget);

            var valueChanged = new { Hello = 34 };
            mockTarget.FireValueChanged(new MvxTargetChangedEventArgs(valueChanged));

            Assert.AreEqual(1, mockSource.ValuesSet.Count);
            Assert.AreEqual("Test ConversionBackResult", mockSource.ValuesSet[0]);
            Assert.AreEqual(1, mockValueConverter.ConversionsBackRequested.Count);
            Assert.AreEqual(valueChanged, mockValueConverter.ConversionsBackRequested[0]);
        }

        [Test]
        public void TestConverterParameterIsUsedForConvertBack()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionBackResult = "Test ConversionBackResult",
            };
            var parameter = new { Ignored = 12 };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, typeof(object), out mockSource, out mockTarget);

            var valueChanged = new { Hello = 34 };
            mockTarget.FireValueChanged(new MvxTargetChangedEventArgs(valueChanged));

            Assert.AreEqual(1, mockSource.ValuesSet.Count);
            Assert.AreEqual("Test ConversionBackResult", mockSource.ValuesSet[0]);
            Assert.AreEqual(1, mockValueConverter.ConversionBackParameters.Count);
            Assert.AreEqual(parameter, mockValueConverter.ConversionBackParameters[0]);
        }

        [Test]
        public void TestTargetTypeIsUsedForConvert()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
            };
            var parameter = new { Ignored = 12 };
            var targetType = new { Foo = 23 }.GetType();
            var binding = this.TestSetupCommon(mockValueConverter, parameter, targetType, out mockSource, out mockTarget);

            mockSource.TryGetValueValue = "new value";
            mockSource.FireSourceChanged();

            Assert.AreEqual(2, mockValueConverter.ConversionTypes.Count);
            Assert.AreEqual(targetType, mockValueConverter.ConversionTypes[0]);
            Assert.AreEqual(targetType, mockValueConverter.ConversionTypes[1]);
        }

        [Test]
        public void TestSourceTypeIsUsedForConvertBack()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
            };
            var parameter = new { Ignored = 12 };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, typeof(object), out mockSource, out mockTarget);

            var aType = new { Hello = 34 };
            mockSource.SourceType = aType.GetType();
            mockTarget.FireValueChanged(new MvxTargetChangedEventArgs("Ignored"));

            Assert.AreEqual(1, mockValueConverter.ConversionBackTypes.Count);
            Assert.AreEqual(aType.GetType(), mockValueConverter.ConversionBackTypes[0]);
        }

        [Test]
        public void TestFallbackValueIsUsedIfConversionFails()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ThrowOnConversion = true
            };
            var parameter = new { Ignored = 12 };
            var fallback = new { Fred = "Not Barney" };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual(fallback, mockTarget.Values[0]);
        }

        [Test]
        public void TestFallbackValueIsUsedIfSourceResolutionFails()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "A test value"
            };
            var parameter = new { Ignored = 12 };
            var fallback = new { Fred = "Not Barney" };
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("A test value", mockTarget.Values[0]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(fallback, mockTarget.Values[1]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfConversionFails_Object()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ThrowOnConversion = true
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[0]);

            mockSource.TryGetValueValue = "Fred";
            mockSource.FireSourceChanged();

            Assert.AreEqual(2, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[1]);

            mockSource.TryGetValueValue = "Betty";
            mockSource.FireSourceChanged();

            Assert.AreEqual(3, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[2]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfConversionFails_Int()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ThrowOnConversion = true
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(int), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[0]);

            mockSource.TryGetValueValue = "Fred";
            mockSource.FireSourceChanged();

            Assert.AreEqual(2, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[1]);

            mockSource.TryGetValueValue = "Betty";
            mockSource.FireSourceChanged();

            Assert.AreEqual(3, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[2]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfConversionFails_NullableInt()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ThrowOnConversion = true
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(int?), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[0]);

            mockSource.TryGetValueValue = "Fred";
            mockSource.FireSourceChanged();

            Assert.AreEqual(2, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[1]);

            mockSource.TryGetValueValue = "Betty";
            mockSource.FireSourceChanged();

            Assert.AreEqual(3, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[2]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfSourceResolutionFails_Object()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "A test value"
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(object), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("A test value", mockTarget.Values[0]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[1]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[2]);

            mockSource.TryGetValueValue = "Fred";
            mockSource.TryGetValueResult = true;
            mockSource.FireSourceChanged();

            Assert.AreEqual(2, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(4, mockTarget.Values.Count);
            Assert.AreEqual("A test value", mockTarget.Values[3]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfSourceResolutionFails_Int()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "A test value"
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(int), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("A test value", mockTarget.Values[0]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[1]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[2]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(4, mockTarget.Values.Count);
            Assert.AreEqual(0, mockTarget.Values[3]);
        }

        [Test]
        public void TestDefaultValueIsUsedIfSourceResolutionFails_NullableInt()
        {
            MockSourceBinding mockSource;
            MockTargetBinding mockTarget;
            var mockValueConverter = new MockValueConverter()
            {
                ConversionResult = "A test value"
            };
            var parameter = new { Ignored = 12 };
            object fallback = null;
            var binding = this.TestSetupCommon(mockValueConverter, parameter, fallback, typeof(int?), out mockSource, out mockTarget);

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(1, mockTarget.Values.Count);
            Assert.AreEqual("A test value", mockTarget.Values[0]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(2, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[1]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(3, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[2]);

            mockSource.TryGetValueResult = false;
            mockSource.FireSourceChanged();

            Assert.AreEqual(1, mockValueConverter.ConversionsRequested.Count);
            Assert.AreEqual(0, mockValueConverter.ConversionsBackRequested.Count);

            Assert.AreEqual(4, mockTarget.Values.Count);
            Assert.AreEqual(null, mockTarget.Values[3]);
        }

        private MvxFullBinding TestSetupCommon(IMvxValueConverter valueConverter, object converterParameter,
                                               Type targetType, out MockSourceBinding mockSource, out MockTargetBinding mockTarget)
        {
            return this.TestSetupCommon(valueConverter, converterParameter, new { Value = 4 }, targetType, out mockSource, out mockTarget);
        }

        private MvxFullBinding TestSetupCommon(IMvxValueConverter valueConverter, object converterParameter, object fallbackValue,
            Type targetType, out MockSourceBinding mockSource, out MockTargetBinding mockTarget)
        {
            ClearAll();
            MvxBindingSingletonCache.Initialize();
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(new InlineMockMainThreadDispatcher());

            var mockSourceBindingFactory = new Mock<IMvxSourceBindingFactory>();
            Ioc.RegisterSingleton(mockSourceBindingFactory.Object);

            var mockTargetBindingFactory = new Mock<IMvxTargetBindingFactory>();
            Ioc.RegisterSingleton(mockTargetBindingFactory.Object);

            var realSourceStepFactory = new MvxSourceStepFactory();
            realSourceStepFactory.AddOrOverwrite(typeof(MvxPathSourceStepDescription), new MvxPathSourceStepFactory());
            Ioc.RegisterSingleton<IMvxSourceStepFactory>(realSourceStepFactory);

            var sourceText = "sourceText";
            var targetName = "targetName";
            var source = new { Value = 1 };
            var target = new { Value = 2 };
            var bindingDescription = new MvxBindingDescription
            {
                Source = new MvxPathSourceStepDescription()
                {
                    Converter = valueConverter,
                    ConverterParameter = converterParameter,
                    FallbackValue = fallbackValue,
                    SourcePropertyPath = sourceText,
                },
                Mode = MvxBindingMode.TwoWay,
                TargetName = targetName
            };

            mockSource = new MockSourceBinding();
            mockTarget = new MockTargetBinding() { TargetType = targetType };
            mockTarget.DefaultMode = MvxBindingMode.TwoWay;

            var localSource = mockSource;
            mockSourceBindingFactory
                .Setup(x => x.CreateBinding(It.IsAny<object>(), It.Is<string>(s => s == sourceText)))
                .Returns((object a, string b) => localSource);
            var localTarget = mockTarget;
            mockTargetBindingFactory
                .Setup(x => x.CreateBinding(It.IsAny<object>(), It.Is<string>(s => s == targetName)))
                .Returns((object a, string b) => localTarget);

            mockSource.TryGetValueResult = true;
            mockSource.TryGetValueValue = "TryGetValueValue";

            var request = new MvxBindingRequest(source, target, bindingDescription);
            var toTest = new MvxFullBinding(request);
            return toTest;
        }
    }
}