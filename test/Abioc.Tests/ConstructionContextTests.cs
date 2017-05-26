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

        public class ConstructorDependentClass
        {
            public ConstructorDependentClass(
                ClassCreatedWithAContext concreteDependency,
                IClassCreatedWithAContext abstractDependency)
            {
                ConcreteDependency =
                    concreteDependency ?? throw new ArgumentNullException(nameof(concreteDependency));
                AbstractDependency =
                    abstractDependency ?? throw new ArgumentNullException(nameof(abstractDependency));
            }

            public ClassCreatedWithAContext ConcreteDependency { get; }
            public IClassCreatedWithAContext AbstractDependency { get; }
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

    public abstract class FactoringClassWithAContextAsAConstructorDependencyBase
    {
        protected AbiocContainer<Guid> Container;

        [Fact]
        public void ItShouldSpecifyRecipientTypeAsTheDependentClassForTheConcreteDependency()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            ConstructorDependentClass actual = Container.GetService<ConstructorDependentClass>(data);

            // Assert
            actual.Should().NotBeNull();
            actual.ConcreteDependency.Should().NotBeNull();
            actual.ConcreteDependency.Context.Extra.Should().Be(data);
            actual.ConcreteDependency.Context.RecipientType.Should().Be(typeof(ConstructorDependentClass));
            actual.ConcreteDependency.Context.ImplementationType.Should().Be(typeof(ClassCreatedWithAContext));
            actual.ConcreteDependency.Context.ServiceType.Should().Be(typeof(ClassCreatedWithAContext));
        }

        [Fact]
        public void ItShouldSpecifyRecipientTypeAsTheDependentClassForTheAbstractDependency()
        {
            // Arrange
            Guid data = Guid.NewGuid();

            // Act
            ConstructorDependentClass actual = Container.GetService<ConstructorDependentClass>(data);

            // Assert
            actual.Should().NotBeNull();
            actual.AbstractDependency.Should().NotBeNull();
            actual.AbstractDependency.Context.Extra.Should().Be(data);
            actual.AbstractDependency.Context.RecipientType.Should().Be(typeof(ConstructorDependentClass));
            actual.AbstractDependency.Context.ImplementationType.Should().Be(typeof(ClassCreatedWithAContext));
            actual.AbstractDependency.Context.ServiceType.Should().Be(typeof(ClassCreatedWithAContext));
        }
    }

    public class WhenFactoringAStronglyTypedClassWithAContextAsAConstructorDependency
        : FactoringClassWithAContextAsAConstructorDependencyBase
    {
        public WhenFactoringAStronglyTypedClassWithAContextAsAConstructorDependency(ITestOutputHelper output)
        {
            Container =
                new RegistrationSetup<Guid>()
                    .RegisterFactory<IClassCreatedWithAContext, ClassCreatedWithAContext>(
                        ClassCreatedWithAContext.StrongCreate,
                        compose => compose.Internal())
                    .Register<ConstructorDependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }
    }

    public class WhenFactoringAWeaklyTypedClassWithAContextAsAConstructorDependency
        : FactoringClassWithAContextAsAConstructorDependencyBase
    {
        public WhenFactoringAWeaklyTypedClassWithAContextAsAConstructorDependency(ITestOutputHelper output)
        {
            Container =
                new RegistrationSetup<Guid>()
                    .RegisterFactory(
                        typeof(IClassCreatedWithAContext),
                        typeof(ClassCreatedWithAContext),
                        ClassCreatedWithAContext.WeakCreate, compose => compose.Internal())
                    .Register<ConstructorDependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }
    }
}
