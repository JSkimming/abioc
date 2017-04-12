// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    public interface ISimpleInterface
    {
    }

    public class SimpleClass1WithoutDependencies : ISimpleInterface
    {
    }

    public class SimpleClass2WithoutDependencies : ISimpleInterface
    {
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
