﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


namespace yieldtome.API.Controllers
{
	public class PingController : ApiController
	{
		/// <summary>
		/// Returns an HTTP response OK
		/// </summary>
		/// <returns></returns>
		public HttpResponseMessage Index()
		{
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}