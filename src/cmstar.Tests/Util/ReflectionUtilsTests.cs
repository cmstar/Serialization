using System;
using System.Collections.Generic;
using NUnit.Framework;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedTypeParameter
namespace cmstar.Util
{
    [TestFixture]
    public class ReflectionUtilsTests
    {
        private struct InnerStruct
        {
        }

        private class InnerClass
        {
        }

        [Test]
        public void TestIsNullable()
        {
            Assert.IsFalse(ReflectionUtils.IsNullable(typeof(int)));
            Assert.IsFalse(ReflectionUtils.IsNullable(typeof(DateTime)));
            Assert.IsFalse(ReflectionUtils.IsNullable(typeof(InnerStruct)));

            Assert.IsTrue(ReflectionUtils.IsNullable(typeof(object)));
            Assert.IsTrue(ReflectionUtils.IsNullable(typeof(InnerClass)));
            Assert.IsTrue(ReflectionUtils.IsNullable(typeof(DateTime?)));
            Assert.IsTrue(ReflectionUtils.IsNullable(typeof(InnerStruct?)));
        }

        [Test]
        public void TestIsNullableType()
        {
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(int)));
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(object)));
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(DateTime)));
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(InnerStruct)));
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(InnerClass)));

            Assert.IsTrue(ReflectionUtils.IsNullableType(typeof(int?)));
            Assert.IsTrue(ReflectionUtils.IsNullableType(typeof(DateTime?)));
            Assert.IsTrue(ReflectionUtils.IsNullableType(typeof(InnerStruct?)));
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(object), ReflectionUtils.GetUnderlyingType(typeof(object)));
            Assert.AreEqual(typeof(int), ReflectionUtils.GetUnderlyingType(typeof(int)));
            Assert.AreEqual(typeof(DateTime), ReflectionUtils.GetUnderlyingType(typeof(DateTime)));
            Assert.AreEqual(typeof(InnerStruct), ReflectionUtils.GetUnderlyingType(typeof(InnerStruct)));
            Assert.AreEqual(typeof(InnerClass), ReflectionUtils.GetUnderlyingType(typeof(InnerClass)));

            Assert.AreEqual(typeof(int), ReflectionUtils.GetUnderlyingType(typeof(int?)));
            Assert.AreEqual(typeof(DateTime), ReflectionUtils.GetUnderlyingType(typeof(DateTime?)));
            Assert.AreEqual(typeof(InnerStruct), ReflectionUtils.GetUnderlyingType(typeof(InnerStruct?)));
        }

        private class GenericClass<T> : Dictionary<string, float>, IGenericInterface<T, int> { }
        private interface IGenericInterface<T, K> { }


        [Test]
        public void GetGenericArguments()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetGenericArguments(null, typeof(int)));
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetGenericArguments(typeof(int), null));
            Assert.Throws<ArgumentException>(() => ReflectionUtils.GetGenericArguments(typeof(int), typeof(int)));

            Assert.IsNull(ReflectionUtils.GetGenericArguments(typeof(int), typeof(List<>)));

            var args = ReflectionUtils.GetGenericArguments(typeof(List<>), typeof(List<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual("T", args[0].Name);

            args = ReflectionUtils.GetGenericArguments(typeof(List<>), typeof(ICollection<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual("T", args[0].Name);

            args = ReflectionUtils.GetGenericArguments(typeof(GenericClass<double>), typeof(GenericClass<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual(typeof(double), args[0]);

            args = ReflectionUtils.GetGenericArguments(typeof(GenericClass<double>), typeof(IGenericInterface<,>));
            Assert.NotNull(args);
            Assert.AreEqual(2, args.Length);
            Assert.AreEqual(typeof(double), args[0]);
            Assert.AreEqual(typeof(int), args[1]);

            args = ReflectionUtils.GetGenericArguments(typeof(GenericClass<double>), typeof(Dictionary<,>));
            Assert.NotNull(args);
            Assert.AreEqual(2, args.Length);
            Assert.AreEqual(typeof(string), args[0]);
            Assert.AreEqual(typeof(float), args[1]);

            args = ReflectionUtils.GetGenericArguments(typeof(GenericClass<double>), typeof(IEnumerable<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual(typeof(KeyValuePair<string, float>), args[0]);

            args = ReflectionUtils.GetGenericArguments(typeof(IList<int>), typeof(IEnumerable<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual(typeof(int), args[0]);

            args = ReflectionUtils.GetGenericArguments(typeof(IList<int>), typeof(IList<>));
            Assert.NotNull(args);
            Assert.AreEqual(1, args.Length);
            Assert.AreEqual(typeof(int), args[0]);
        }
    }
}