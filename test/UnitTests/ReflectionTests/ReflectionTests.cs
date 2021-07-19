using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.ReflectionTests
{

    class fx
    {
        int add(List<int> a, int b) => a.Sum() + b;
        public fx()
        {
        }
    }

    public class ReflectionTests
    {
        [Fact]
        public async void MethodInfoTest()
        {
            var method = typeof(fx).GetMethod("add", BindingFlags.NonPublic | BindingFlags.Instance);
            var a = method.GetParameters().First();
            var x = a.ToString();
        }
    }
}
