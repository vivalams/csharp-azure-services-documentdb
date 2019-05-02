//  -----------------------------------------------------------------------
//  
//  <copyright file="ILearnerRecordDocumentClient.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace Microsoft.LearningServices.LearnerRecords.Api.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;

     /// <summary>
    /// Interface I{{ cookiecutter.controlller_name }}DocumentClient
    /// </summary>
    public interface I{{ cookiecutter.controlller_name }}DocumentClient : IDocumentDbClient
    {
        /// <summary>
        /// Test Get Method
        /// </summary>
        /// <param name="testValue">Test value please change it later  </param>
        /// <returns>The {{ cookiecutter.controlller_name }}</returns>
        Task<{{ cookiecutter.controlller_name }}> GetTestLearnerRecords(string testValue);
    }

}