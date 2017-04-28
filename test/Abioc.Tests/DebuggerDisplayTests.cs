// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;

    public abstract class DebuggerDisplayTestsBase
    {
        private Type _sutType;
        private DebuggerDisplayAttribute _debuggerDisplay;
        private PropertyInfo _debuggerDisplayPropertyInfo;
        private MethodInfo _debuggerDisplayGetMethod;
        private object _debuggerDisplayValue;
        protected string DebuggerDisplayText;

        protected void GetDebuggerDisplay<TSut>(TSut sut)
        {
            _sutType = sut.GetType();

            _debuggerDisplay = _sutType.GetTypeInfo().GetCustomAttribute<DebuggerDisplayAttribute>(inherit: false);

            _debuggerDisplayPropertyInfo =
                _sutType.GetProperty("DebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);

            _debuggerDisplayGetMethod = _debuggerDisplayPropertyInfo.GetGetMethod(true);

            _debuggerDisplayValue = _debuggerDisplayGetMethod.Invoke(sut, new object[] { });

            DebuggerDisplayText = _debuggerDisplayValue.ToString();
        }

        [Fact]
        public void HaveTheDebuggerDisplayAttribute()
        {
            _debuggerDisplay.Should().NotBeNull();
        }

        [Fact]
        public void SpecifyTheDebuggerDisplayProperty()
        {
            _debuggerDisplay.Value.Should().BeEquivalentTo("{DebuggerDisplay,nq}");
        }

        [Fact]
        public void HaveTheDebuggerDisplayPrivateProperty()
        {
            _debuggerDisplayPropertyInfo.Should().NotBeNull();
        }

        [Fact]
        public void HaveAGetterOnTheDebuggerDisplayProperty()
        {
            _debuggerDisplayGetMethod.Should().NotBeNull();
        }

        [Fact]
        public void ProvideAStringDisplayProperty()
        {
            _debuggerDisplayValue.Should().BeOfType<string>();
        }

        [Fact]
        public void IncludeTheTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().StartWith($"{_sutType.Name}:");
        }
    }

    public class WhenRunningInTheDebuggerFactoryRegistrationShould : DebuggerDisplayTestsBase
    {
        private readonly Type _implementationType;

        public WhenRunningInTheDebuggerFactoryRegistrationShould()
        {
            _implementationType = GetType();
            var sut = new FactoryRegistration(_implementationType, () => null);
            GetDebuggerDisplay(sut);
        }

        [Fact]
        public void IncludeImplementationTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().Contain(_implementationType.Name);
        }
    }

    public class WhenRunningInTheDebuggerFactoryRegistrationExtraShould : DebuggerDisplayTestsBase
    {
        private readonly Type _implementationType;

        public WhenRunningInTheDebuggerFactoryRegistrationExtraShould()
        {
            _implementationType = GetType();
            var sut = new FactoryRegistration<int>(_implementationType, c => null);
            GetDebuggerDisplay(sut);
        }

        [Fact]
        public void IncludeImplementationTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().Contain(_implementationType.Name);
        }
    }

    public class WhenRunningInTheDebuggerSingleConstructorRegistrationShould : DebuggerDisplayTestsBase
    {
        private readonly Type _implementationType;

        public WhenRunningInTheDebuggerSingleConstructorRegistrationShould()
        {
            _implementationType = GetType();
            var sut = new SingleConstructorRegistration(_implementationType);
            GetDebuggerDisplay(sut);
        }

        [Fact]
        public void IncludeImplementationTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().Contain(_implementationType.Name);
        }
    }

    public class WhenRunningInTheDebuggerTypedFactoryRegistrationShould : DebuggerDisplayTestsBase
    {
        private readonly Type _implementationType;

        public WhenRunningInTheDebuggerTypedFactoryRegistrationShould()
        {
            var sut = new TypedFactoryRegistration<DebuggerDisplayTestsBase>(() => null);
            _implementationType = sut.ImplementationType;
            GetDebuggerDisplay(sut);
        }

        [Fact]
        public void IncludeImplementationTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().Contain(_implementationType.Name);
        }
    }

    public class WhenRunningInTheDebuggerTypedFactoryRegistrationExtraShould : DebuggerDisplayTestsBase
    {
        private readonly Type _implementationType;

        public WhenRunningInTheDebuggerTypedFactoryRegistrationExtraShould()
        {
            var sut = new TypedFactoryRegistration<int, DebuggerDisplayTestsBase>(c => null);
            _implementationType = sut.ImplementationType;
            GetDebuggerDisplay(sut);
        }

        [Fact]
        public void IncludeImplementationTypeInTheDebuggerDisplay()
        {
            DebuggerDisplayText.Should().Contain(_implementationType.Name);
        }
    }
}
