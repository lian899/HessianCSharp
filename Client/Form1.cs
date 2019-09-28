using Hessian.Models;
using HessianCSharp.client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var dob = -2147483649.32334566D;
                Console.Write(dob);

                List<int> ls = new List<int>();
                bool ype = typeof(IList<int>).IsAssignableFrom(typeof(List<int>));
                Console.Write(ype);

                Stopwatch st = new Stopwatch();
                st.Start();
                CHessianProxyFactory factory = new CHessianProxyFactory();
                //factory.IsHessian2Request = false;
                //factory.IsHessian2Reply = false;
                factory.BaseAddress = new Uri("http://localhost.fiddler:2010/");
                var service = factory.Create<IService>();
                var result = service.Test2(new List<int>() { 1, 3, 44 });
                richTextBox1.Text = $"数据类型：{result.ToString()}";

                var type = result.GetType();
                if (type.IsGenericType || type.IsArray || result is DataTable)
                {
                    dataGridView1.DataSource = result;
                    richTextBox1.Text += $"，行数：{dataGridView1.Rows.Count}，列数：{dataGridView1.ColumnCount}";
                }

                st.Stop();
                var elapsed = st.ElapsedMilliseconds;
                richTextBox1.Text += $"，总消耗时间：{elapsed}毫秒";



                //var fs = File.Create(@"bin.bin");
                //BinaryWriter bw = new BinaryWriter(fs);
                //foreach (DataRow dr in result.Rows)
                //{
                //    foreach (object obj in dr.ItemArray)
                //    {
                //        var bytes = BitConverter.GetBytes((int)obj);
                //        bw.Write(bytes);
                //    }
                //}
                //bw.Close();
            }
            catch (Exception ex)
            {
                richTextBox1.Text = ex.ToString();
            }
        }
    }
}
