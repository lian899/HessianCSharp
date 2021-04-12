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
using System.IO;
using System.Web;
using HessianCSharp.io;
using System.Web.SessionState;
using System;

#endregion

namespace HessianCSharp.server
{
    /// <summary>
    /// HessianHandler for request and response
    /// </summary>
    public class CustomHandler : IHttpHandler, IRequiresSessionState
    {
        #region CLASS_FIELDS

        /// <summary>
        /// Proxy object
        /// </summary>
        private HttpContext context;

        #endregion

        protected HttpContext Context { get { return context; } }
        public bool IsReusable { get { return true; } }

        /// <summary>
        /// Execute a request.
        /// </summary>				
        public void ProcessRequest(HttpContext ctx)
        {
            try
            {
                context = ctx;
                Stream inStream = ctx.Request.InputStream;
                //MemoryStream outStream = new MemoryStream();

                ctx.Response.BufferOutput = true;
                ctx.Response.ContentType = "text/xml";

                //AbstractHessianInput inHessian = new CHessianInput(inStream);
                //AbstractHessianOutput outHessian = new CHessianOutput(ctx.Response.OutputStream);

                var service = ServiceFactory.SelectService(ctx.Request.Path);
                if (service == null)
                {
                    ctx.Response.StatusCode = 404;  // "Internal server error"
                    ctx.Response.StatusDescription = "Service Not Found.";
                    return;
                }
                //Vieleicht das Interface als API übergeben???
                var m_objectSkeleton = new CHessianSkeleton(service.GetType(), service);

                m_objectSkeleton.invoke(inStream, ctx.Response.OutputStream);
                //byte[] arrData = outStream.ToArray();
                //int intLength = arrData.Length;
                //Set length
                //ctx.Response.AppendHeader("Content-Length", intLength.ToString());
                //Write stream
                //ctx.Response.OutputStream.Write(arrData, 0, intLength);
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;  // "Internal server error"
                var encode = System.Web.HttpUtility.UrlEncode(ex.Message);
                ctx.Response.StatusDescription = encode.Length > 512 ? encode.Substring(0, 512) : encode;
            }
        }
    }

}
