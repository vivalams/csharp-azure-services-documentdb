//  -----------------------------------------------------------------------
//  
//  <copyright file="BaseController.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------

namespace {{ cookiecutter.assembly_name }}.Api.Controllers
{
    using System;
    using System.Security.Claims;
    using AspNetCore.Authorization;
    using AspNetCore.Cors;
    using AspNetCore.Mvc;
   
    /// <summary>
    /// Class BaseController.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Authorize]
    [EnableCors("AllowAllHeaders")]
    public class BaseController : Controller
    {}
}