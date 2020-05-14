using Hessian.Models;
using System;
using System.Collections.Generic;

namespace HessianService
{
    public class Service : IService
    {

        public string Hello()
        {
            return "Hello World!";
        }


        public TestClass[] Test2()
        {
            throw new Exception("test");
            List<TestClass> array = new List<TestClass>();
            //var d = "3147483647D";
            //var ind = (int.Parse(d));
            //Console.WriteLine(ind);
            var sd = new TestClass()
            {
                guid = Guid.NewGuid(),
                Integer = 3,
                ArrayList = new System.Collections.ArrayList { DBNull.Value, DBNull.Value },
                Decimal = -7888884.23243248M,
                Float = -122.1434F,
                Double = -2147483649.32334566D,
                Long = -234344234325L,
                Long2=3233,
                String = "你还好吗？你還好嗎？Are You Ok? Nǐ hái hǎo ma?大丈夫ですか？Você está bem?Вы ў парадку?",
                DateTime = DateTime.Now
            };
            //sd.m = 2343423.233432M;
            array.Add(sd);
            sd = new TestClass()
            {
                guid = Guid.NewGuid(),
                Integer = -1,
                ArrayList = new System.Collections.ArrayList { DBNull.Value, DBNull.Value },
                Decimal = 7888884.23243248M,
                Float = 122.1434F,
                Double = -3,
                Long = -1,
                String = "你还好吗？你還好嗎？Are You Ok? Nǐ hái hǎo ma?大丈夫ですか？Você está bem?Вы ў парадку?",
                DateTime = DateTime.Now
            };
            array.Add(sd);
            sd = new TestClass() { String = "r" };
            array.Add(sd);
            sd = new TestClass() { String = "f" };
            array.Add(sd);
            sd = new TestClass() { String = "d" };
            array.Add(sd);
            var first = array[0];
            Console.WriteLine(first);
            return array.ToArray();
        }
    }

}
