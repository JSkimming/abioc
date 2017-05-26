// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.ConstructionContextTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace ConstructionContextTests
    {
        public interface IClassCreatedWithAContext
        {
            ConstructionContext<Guid> Context { get; }
        }

        public class ClassCreatedWithAContext : IClassCreatedWithAContext
        {
            private ClassCreatedWithAContext(ConstructionContext<Guid> context)
            {
                Context = context;
            }

            public ConstructionContext<Guid> Context { get; }

            internal static ClassCreatedWithAContext StrongCreate(ConstructionContext<Guid> context)
            {
                var service = new ClassCreatedWithAContext(context);
                return service;
            }

            internal static object WeakCreate(ConstructionContext<Guid> context)
            {
                return StrongCreate(context);
            }
        }
    }

    public abstract class FactoringClassWithAContextBase
    {
        protected AbiocContainer<Guid> Container;

        [Fact]
        public void ItShouldSpecifyTheImplementationTypeAsTheConcreteClassIfRequestingTheConcreteClass()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            ClassCreatedWithAContext actual = Container.GetService<ClassCreatedWithAContext>(data);

            // Assert
            actual.Should().NotBeNull();
            actual.Context.Extra.Should().Be(data);
            actual.Context.ImplementationType.Should().Be(typeof(ClassCreatedWithAContext));
            actual.Context.RecipientType.Should().Be(typeof(void));
        }

        [Fact]
        public void ItShouldSpecifyTheServiceTypeAsTheConcreteClassIfRequestingTheConcreteClass()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            ClassCreatedWithAContext actual = Container.GetService<ClassCreatedWithAContext>(data);

            // Assert
            actual.Should().NotBeNull();
            actual.Context.Extra.Should().Be(data);
            actual.Context.ServiceType.Should().Be(typeof(ClassCreatedWithAContext));
            actual.Context.RecipientType.Should().Be(typeof(void));
        }

        [Fact]
        public void ItShouldSpecifyTheImplementationTypeAsTheConcreteClassIfRequestingTheInterface()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            IClassCreatedWithAContext actual = Container.GetService<IClassCreatedWithAContext>(data);

            // Assert
            actual.Should().NotBeNull().And.BeOfType<ClassCreatedWithAContext>();
            actual.Context.Extra.Should().Be(data);
            actual.Context.ImplementationType.Should().Be(typeof(ClassCreatedWithAContext));
            actual.Context.RecipientType.Should().Be(typeof(void));
        }

        [Fact]
        public void ItShouldSpecifyTheServiceTypeTheAsInterfaceIfRequestingTheInterface()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            IClassCreatedWithAContext actual = Container.GetService<IClassCreatedWithAContext>(data);

            // Assert
            actual.Should().NotBeNull().And.BeOfType<ClassCreatedWithAContext>();
            actual.Context.Extra.Should().Be(data);
            actual.Context.ServiceType.Should().Be(typeof(IClassCreatedWithAContext));
            actual.Context.RecipientType.Should().Be(typeof(void));
        }
    }

    public class WhenFactoringAStronglyTypedClassWithAContext : FactoringClassWithAContextBase
    {
        public WhenFactoringAStronglyTypedClassWithAContext(ITestOutputHelper output)
        {
            Container =
                new RegistrationSetup<Guid>()
                    .RegisterFactory(ClassCreatedWithAContext.StrongCreate)
                    .Register<IClassCreatedWithAContext, ClassCreatedWithAContext>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }
    }

    public class WhenFactoringAWeaklyTypedClassWithAContext : FactoringClassWithAContextBase
    {
        public WhenFactoringAWeaklyTypedClassWithAContext(ITestOutputHelper output)
        {
            Container =
                new RegistrationSetup<Guid>()
                    .RegisterFactory(typeof(ClassCreatedWithAContext), ClassCreatedWithAContext.WeakCreate)
                    .Register<IClassCreatedWithAContext, ClassCreatedWithAContext>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }
    }
}
