/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.HessianCSharp.com
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
* Last change: 2005-08-14
* By Andre Voltmann	
* Licence added.
******************************************************************************************************
*/

#region NAMESPACES
using HessianCSharp.io;
using HessianCSharp.server;
using System;
using System.IO;
using System.Linq;
using System.Net;
#endregion

namespace HessianCSharp.client
{
    /// <summary>
    /// Factory for Proxy - creation.
    /// </summary>
    public class CHessianProxyFactory
    {
        #region CLASS_FIELDS
        /// <summary>
        /// flag, that allows or not the overloaded methods (using mangling)
        /// </summary>
        private bool m_blnIsOverloadEnabled = false;

        private string m_password;
        private string m_username;
        private string _urlSuffix = ".do";

        private Uri base_address;
        public Uri BaseAddress
        {
            get { return base_address; }
            set { base_address = value; }
        }

        private WebHeaderCollection headers;
        public WebHeaderCollection Headers
        {
            get
            {
                return headers ?? (headers = new WebHeaderCollection());
            }
        }

        private TimeSpan timeout = TimeSpan.FromSeconds(100);
        public TimeSpan Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                if (value != TimeSpan.FromMilliseconds(System.Threading.Timeout.Infinite) && value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException();

                timeout = value;
            }
        }

        private bool _isHessian2Reply = true;
        private bool _isHessian2Request = true;


        #endregion

        #region Contructors
        public CHessianProxyFactory()
        {
            m_username = null;
            m_password = null;
        }
        public CHessianProxyFactory(string username, string password)
        {
            m_username = username;
            m_password = password;

        }

        #endregion

        #region PROPERTIES
        /// <summary>
        /// Returns of Sets flag, that allows or not the overloaded methods (using mangling)
        /// </summary>
        public bool IsOverloadEnabled
        {
            get { return m_blnIsOverloadEnabled; }
            set { m_blnIsOverloadEnabled = value; }
        }

        public bool IsHessian2Request
        {
            get { return _isHessian2Request; }

            set { _isHessian2Request = value; }
        }

        public bool IsHessian2Reply
        {
            get { return _isHessian2Reply; }

            set { _isHessian2Reply = value; }
        }

        public string UrlSuffix
        {
            get { return _urlSuffix; }
            set { _urlSuffix = value; }
        }

        #endregion

        #region PUBLIC_METHODS


        /// <summary>
        /// Creates a new proxy with the specified URL.  The returned object
        /// is a proxy with the interface specified by api.
        /// <code>
        /// string url = "http://localhost:8080/ejb/hello");
        /// HelloHome hello = (HelloHome) factory.create(HelloHome.class, url);
        /// </code>
        /// </summary>
        /// <param name="type">the interface the proxy class needs to implement</param>
        /// <param name="strUrl">the URL where the client object is located</param>
        /// <returns>a proxy to the object with the specified interface</returns>
        public Object Create(Type type, string strUrl)
        {
            return CreateHessianStandardProxy(strUrl, type);
        }

        public T Create<T>()
        {
            string url;
            var attrRoute = (HessianRouteAttribute)typeof(T).GetCustomAttributes(typeof(HessianRouteAttribute), false).FirstOrDefault();
            if (attrRoute != null)
            {
                url = attrRoute.Uri?.Trim();
                if (url?.EndsWith(UrlSuffix) ?? false)
                    url = url.Substring(0, url.Length - UrlSuffix.Length);
            }
            else
            {
                var type = typeof(T);
                url = "/" + (type.Namespace + "." + type.Name).Replace(".", "/");
            }
            return (T)CreateHessianStandardProxy(url, typeof(T));
        }

        /// <summary>
        /// Creates proxy object using .NET - Remote proxy framework
        /// </summary>
        /// <param name="type">the interface the proxy class needs to implement</param>
        /// <param name="strUrl">the URL where the client object is located</param>
        /// <returns>a proxy to the object with the specified interface</returns>
        private object CreateHessianStandardProxy(string strUrl, Type type)
        {


#if COMPACT_FRAMEWORK
			// do CF stuff
			throw new CHessianException("not supported in compact version");
			
#else
            if ((m_username == null) && (m_password == null))
            {




                return new CHessianProxyStandardImpl(type, this, new Uri(strUrl, System.UriKind.RelativeOrAbsolute)).GetTransparentProxy();
            }
            else
            {
                return new CHessianProxyStandardImpl(type, this, new Uri(strUrl, System.UriKind.RelativeOrAbsolute), m_username, m_password).GetTransparentProxy();
            }
#endif

        }

        internal AbstractHessianInput GetHessianInput(Stream inputStream)
        {
            return GetHessian2Input(inputStream);
        }

        internal AbstractHessianInput GetHessian1Input(Stream inputStream)
        {
            AbstractHessianInput abstractHessianInput = new CHessianInput(inputStream);

            return abstractHessianInput;
        }

        internal AbstractHessianInput GetHessian2Input(Stream inputStream)
        {
            AbstractHessianInput abstractHessianInput = new CHessian2Input(inputStream);

            return abstractHessianInput;
        }

        internal AbstractHessianOutput GetHessianOutput(Stream os)
        {
            AbstractHessianOutput abstractHessianOutput;

            if (_isHessian2Request)
                abstractHessianOutput = new CHessian2Output(os);
            else
            {
                CHessianOutput out1 = new CHessianOutput(os);
                abstractHessianOutput = out1;

                if (_isHessian2Reply)
                    out1.SetVersion(2);
            }
            return abstractHessianOutput;
        }

        #endregion
    }
}