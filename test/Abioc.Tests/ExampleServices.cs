// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;

    public interface ISimpleInterface
    {
    }

    public class SimpleClass1WithoutDependencies : ISimpleInterface
    {
    }

    public class SimpleClass2WithoutDependencies : ISimpleInterface
    {
    }

    public class ClassWithoutAPublicConstructor
    {
        private ClassWithoutAPublicConstructor()
        {
        }

        public static ClassWithoutAPublicConstructor Create()
        {
            return new ClassWithoutAPublicConstructor();
        }
    }

    public class ClassWithMultiplePublicConstructors
    {
        public ClassWithMultiplePublicConstructors()
            : this(ClassWithoutAPublicConstructor.Create())
        {
        }

        public ClassWithMultiplePublicConstructors(ClassWithoutAPublicConstructor other)
        {
            Other = other;
        }

        public ClassWithoutAPublicConstructor Other { get; }
    }

    public class ClassWithAPrivateAndPublicConstructor
    {
        private ClassWithAPrivateAndPublicConstructor()
            : this(new SimpleClass1WithoutDependencies())
        {
        }

        public ClassWithAPrivateAndPublicConstructor(SimpleClass1WithoutDependencies other)
        {
            Other = other;
        }

        public SimpleClass1WithoutDependencies Other { get; }
    }

    public class ClassWithAnInternalAndPublicConstructor
    {
        internal ClassWithAnInternalAndPublicConstructor()
            : this(new SimpleClass1WithoutDependencies())
        {
        }

        public ClassWithAnInternalAndPublicConstructor(SimpleClass1WithoutDependencies other)
        {
            Other = other;
        }

        public SimpleClass1WithoutDependencies Other { get; }
    }
}

namespace Example.Ns1
{
    using System;

    public class MyClass1
    {
    }

    public class MyClass2
    {
        public MyClass1 MyClass1 { get; }

        public MyClass2(MyClass1 myClass1)
        {
            MyClass1 = myClass1 ?? throw new ArgumentNullException(nameof(myClass1));
        }
    }

    public class MyClass3
    {
        public MyClass1 MyClass1 { get; }
        public MyClass2 MyClass2 { get; }
        public Ns2.MyClass1 MyOtherClass1 { get; }
        public Ns2.MyClass2 MyOtherClass2 { get; }

        public MyClass3(MyClass1 myClass1, MyClass2 myClass2, Ns2.MyClass1 myOtherClass1, Ns2.MyClass2 myOtherClass2)
        {
            MyClass1 = myClass1 ?? throw new ArgumentNullException(nameof(myClass1));
            MyClass2 = myClass2 ?? throw new ArgumentNullException(nameof(myClass2));
            MyOtherClass1 = myOtherClass1 ?? throw new ArgumentNullException(nameof(myOtherClass1));
            MyOtherClass2 = myOtherClass2 ?? throw new ArgumentNullException(nameof(myOtherClass2));
        }
    }
}

namespace Example.Ns2
{
    using System;

    public class MyClass1
    {
    }

    public class MyClass2
    {
        public MyClass1 MyClass1 { get; }
        public Ns1.MyClass1 MyOtherClass1 { get; }

        public MyClass2(MyClass1 myClass1, Ns1.MyClass1 myOtherClass1)
        {
            MyClass1 = myClass1 ?? throw new ArgumentNullException(nameof(myClass1));
            MyOtherClass1 = myOtherClass1 ?? throw new ArgumentNullException(nameof(myOtherClass1));
        }
    }
}
