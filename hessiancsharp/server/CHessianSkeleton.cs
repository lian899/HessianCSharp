/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.HessianCSharp.org
*
* This library is free software; you can redistribute it and/or
* modify it under the terms of the GNU Lesser General Public
* License as published by the Free Software Foundation; either
* version 2.1 of the License, or (at your option) any later version.
*
* This library is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
* Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public
* License along with this library; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
* 
* You can find the GNU Lesser General Public here
* http://www.gnu.org/licenses/lgpl.html
* or in the license.txt file in your source directory.
******************************************************************************************************  
* You can find all contact information on http://www.HessianCSharp.com	
******************************************************************************************************
*
*
******************************************************************************************************
* Last change: 2005-12-16
* By Dimitri Minich	
* Exception handling
******************************************************************************************************
*/
#region NAMESPACES
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HessianCSharp.io;

#endregion

namespace HessianCSharp.server
{
    /// <summary>
    /// Proxy class for Hessian services.
    /// </summary>
    public class CHessianSkeleton
    {
        #region CLASS_FIELDS
        //Api Classe
        private Type m_typApi;
        private Object m_Service;
        private IDictionary m_dictMethod = new Hashtable();
        private HessianInputFactory _inputFactory = new HessianInputFactory();
        private HessianFactory _hessianFactory = new HessianFactory();
        #endregion

        /// <summary>
        /// 调用异常回调，用来作日志记录
        /// </summary>
        public Action<object, Exception> InvokeErrorCallBack;


        /// <summary> 
        /// Create a new hessian skeleton.
        ///
        ///param service the underlying service object.
        ///param apiClass the API interface
        /// </summary>
        public CHessianSkeleton(Type api, Object service)
        {

            m_typApi = api;
            m_Service = service;

            foreach (MethodInfo metInfo in m_typApi.GetMethods())
            {
                ParameterInfo[] parInfo = metInfo.GetParameters();


                if (!m_dictMethod.Contains(metInfo.Name))
                {
                    m_dictMethod.Add(metInfo.Name, metInfo);
                }

                String mangledName = metInfo.Name + "__" + parInfo.Length;
                if (!m_dictMethod.Contains(mangledName))
                {
                    m_dictMethod.Add(mangledName, metInfo);
                }

                String mangledName2 = MangleName(metInfo, false);
                if (!m_dictMethod.Contains(mangledName2))
                {
                    m_dictMethod.Add(mangledName2, metInfo);
                }

            }


        }


        /// <summary>
        /// Invoke the object with the request from the input stream.
        /// </summary>
        /// <param name="inputStream">in the Hessian input stream</param>
        /// <param name="outputStream">@param out the Hessian output stream</param>
        public void invoke(Stream inputStream, Stream outputStream)
        {


            HeaderType header = _inputFactory.ReadHeader(inputStream);

            AbstractHessianInput input;
            AbstractHessianOutput output;

            switch (header)
            {
                case HeaderType.CALL_1_REPLY_1:
                    input = _hessianFactory.CreateHessianInput(inputStream);
                    output = _hessianFactory.CreateHessianOutput(outputStream);
                    break;

                case HeaderType.CALL_1_REPLY_2:
                    input = _hessianFactory.CreateHessianInput(inputStream);
                    output = _hessianFactory.CreateHessian2Output(outputStream);
                    break;

                case HeaderType.HESSIAN_2:
                    input = _hessianFactory.CreateHessian2Input(inputStream);
                    input.ReadCall();
                    output = _hessianFactory.CreateHessian2Output(outputStream);
                    break;

                default:
                    throw new InvalidOperationException(header + " is an unknown Hessian call");
            }

            try
            {
                invoke(input, output);
            }
            finally
            {
                output.Flush();
            }
        }


        /// <summary>
        /// Invoke the object with the request from the input stream.
        /// </summary>
        /// <param name="inHessian">the Hessian input stream</param>
        /// <param name="outHessian">the Hessian output stream</param>		
        public void invoke(AbstractHessianInput inHessian, AbstractHessianOutput outHessian)
        {
            inHessian.SkipOptionalCall();

            // Hessian 1.0 backward compatibility
            String header;
            while ((header = inHessian.ReadHeader()) != null)
            {
                Object value = inHessian.ReadObject();
                //context.addHeader(header, value);
            }

            String methodName = inHessian.ReadMethod();
            int argLength = inHessian.ReadMethodArgLength();

            MethodInfo methodInf = getMethodInfo(methodName + "__" + argLength);

            if (methodInf == null)
                methodInf = getMethodInfo(methodName);

            //If the method doesn't exist
            if (methodInf == null)
            {
                outHessian.WriteFault("NoSuchMethodException",
                    EscapeMessage("The service has no method named: " + methodName),
                    null);
                outHessian.CompleteReply();
                return;
            }

            ParameterInfo[] paramInfo = methodInf.GetParameters();
            Object[] valuesParam = new Object[paramInfo.Length];

            if (argLength != paramInfo.Length && argLength >= 0)
            {
                outHessian.WriteFault("NoSuchMethod",
                        EscapeMessage("method " + methodInf + " argument length mismatch, received length=" + argLength),
                        null);
                outHessian.Flush();
                return;
            }

            for (int i = 0; i < paramInfo.Length; i++)
            {
                valuesParam[i] = inHessian.ReadObject(paramInfo[i].ParameterType);
            }
            inHessian.CompleteCall();

            var interceptor = methodInf.GetCustomAttributes(typeof(HessianInterceptorAttribute), false)
                .Cast<HessianInterceptorAttribute>().FirstOrDefault();

            MethodExecutedContext methodContext = new MethodExecutedContext();
            methodContext.ServiceType = m_Service.GetType();
            methodContext.Method = methodInf;
            methodContext.ParamInfos = paramInfo;
            methodContext.ParamValues = valuesParam;

            Object result = null;

            try
            {
                result = methodInf.Invoke(m_Service, valuesParam);
            }
            catch (Exception e)
            {
                //TODO: Exception besser behandeln

                //if (e.GetType() == typeof(System.Reflection.TargetInvocationException))
                //{
                //    if (e.InnerException != null)
                //    {
                //        e = e.InnerException;
                //    }
                //}

                InvokeErrorCallBack?.Invoke(this, e);

                methodContext.Exception = e;
                interceptor?.OnMethodExecuted(methodContext);

                //多层InnerException使用GetBaseException()更好。
                e = e.GetBaseException();
                //outHessian.StartReply();
                outHessian.WriteFault("ServiceException", e.Message, e.ToString());
                outHessian.Flush();
                //outHessian.CompleteReply();
                return;
            }

            methodContext.ReturnValue = result;
            interceptor?.OnMethodExecuted(methodContext);

            outHessian.StartReply();

            outHessian.WriteObject(result);

            outHessian.CompleteReply();
        }


        /// <summary>
        /// Returns the method by the mangled name.
        /// </summary>
        /// <param name="mangledName">the name passed by the protocol</param>
        /// <returns>MethodInfo of the method</returns>		
        protected MethodInfo getMethodInfo(String mangledName)
        {
            return (MethodInfo)m_dictMethod[mangledName];
        }

        /// <summary>
        /// Creates a unique mangled method name based on the method name and
        /// the method parameters.
        /// </summary>
        /// <param name="methodInfo">he method to mangle</param>
        /// <param name="isFull">if true, mangle the full classname</param>
        /// <returns>return a mangled string.</returns>	
        private String MangleName(MethodInfo methodInfo, bool isFull)
        {
            StringBuilder sbTemp = new StringBuilder();

            sbTemp.Append(methodInfo.Name);
            ParameterInfo[] paramsInf = methodInfo.GetParameters();
            foreach (ParameterInfo p in paramsInf)
            {
                sbTemp.Append('_');
                MangleClass(sbTemp, p.ParameterType, isFull);
            }

            return sbTemp.ToString();
        }


        /// <summary>
        /// Mangles a classname.
        /// </summary>
        /// <param name="sb">StringBuilder for writing in</param>
        /// <param name="paramType">Type of methodparameter</param>
        ///  <param name="isFull">if true, mangle the full classname</param>		
        private void MangleClass(StringBuilder sb, Type paramType, bool isFull)
        {

            String nameTemp = paramType.ToString();

            if (nameTemp.Equals("bool") || nameTemp.Equals("System.Boolean"))
            {
                sb.Append("boolean");
            }
            else if (nameTemp.Equals("int") || nameTemp.Equals("System.Int32") ||
                nameTemp.Equals("short") || nameTemp.Equals("System.Int16") ||
                nameTemp.Equals("byte") || nameTemp.Equals("System.Byte"))
            {
                sb.Append("int");
            }
            else if (nameTemp.Equals("long") || nameTemp.Equals("System.Int64"))
            {
                sb.Append("long");
            }
            else if (nameTemp.Equals("float") || nameTemp.Equals("System.Single") ||
                nameTemp.Equals("double") || nameTemp.Equals("System.Double"))
            {
                sb.Append("double");
            }
            else if (nameTemp.Equals("System.String") ||
                nameTemp.Equals("char") || nameTemp.Equals("System.Char"))
            {
                sb.Append("string");
            }
            else if (nameTemp.Equals("System.DateTime"))
            {
                sb.Append("date");
            }
            else if (paramType.IsAssignableFrom(typeof(Stream)) || nameTemp.Equals("[B"))
            {
                sb.Append("binary");
            }
            else if (paramType.IsArray)
            {
                sb.Append("[");
                MangleClass(sb, paramType.GetElementType(), isFull); ;
            }
            else if (isFull)
            {
                sb.Append(nameTemp);
            }
            else
            {
                int p = nameTemp.LastIndexOf('.');
                if (p > 0)
                    sb.Append(nameTemp.Substring(p + 1));
                else
                    sb.Append(nameTemp);
            }
            //TODO:XML

        }

        private String EscapeMessage(String msg)
        {
            if (msg == null)
                return null;

            StringBuilder sb = new StringBuilder();

            int length = msg.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = msg[i];

                switch (ch)
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case (char)0x0:
                        sb.Append("&#00;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }

    }
}
