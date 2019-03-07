using HessianCSharp.server;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hessian.Models
{
    [HessianRoute("/Service.do")]
    public interface IService
    {
        string Hello();
        TestClass[] Test2();
    }

    public class TestClass
    {
        public Guid guid { get; set; }
        public string String { get; set; }
        public int? Integer { get; set; }
        public Status Enum { get; set; }
        public ArrayList ArrayList { get; set; }
        public decimal? Decimal { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public long Long { get; set; }
        public DateTime DateTime { get; set; }
    }

    public enum Status
    {
        未发送,
        已发,
        签收
    }
}
