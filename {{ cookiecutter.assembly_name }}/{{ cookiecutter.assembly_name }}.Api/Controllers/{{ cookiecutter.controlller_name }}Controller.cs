//  -----------------------------------------------------------------------
//  
//  <copyright file="{{ cookiecutter.controlller_name }}Controller.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api.Controllers
{ 
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents;

     /// <summary>
    /// Learner Record Controller
    /// </summary>
    /// <seealso cref="Microsoft.LearningServices.LearnerRecords.Api.Controllers.BaseController" />
    [Route("api/[controller]")]
    public class {{ cookiecutter.controlller_name }}Controller : BaseController
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="{{ cookiecutter.controlller_name }}Controller" /> class
        /// </summary>
         public {{ cookiecutter.controlller_name }}Controller()
         {
             // Constructor code 
         }

    }

}