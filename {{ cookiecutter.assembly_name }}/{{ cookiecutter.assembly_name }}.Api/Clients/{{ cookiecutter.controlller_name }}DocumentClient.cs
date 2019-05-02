//  -----------------------------------------------------------------------
//  
//  <copyright file="{{ cookiecutter.controlller_name }}DocumentClient.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api.Clients
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Web;
    using Azure.Documents;
    using Azure.Documents.Client;
    using Azure.Documents.Linq;

    /// <summary>
    /// The document db client class extension for {{ cookiecutter.controlller_name }}. Has some specific methods for querying
    /// {{ cookiecutter.controlller_name }} records and CRUD implementations leaning on partition keys
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class {{ cookiecutter.controlller_name }}DocumentClient : DocumentDbClient, I{{ cookiecutter.controlller_name }}DocumentClient
    {

        /// <summary>
        /// An azure cosmos document db document client.
        /// </summary>
        protected readonly DocumentClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="{{ cookiecutter.controlller_name }}DocumentClient"/> class.
        /// </summary>
        public {{ cookiecutter.controlller_name }}DocumentClient()
        {
            // Initialize Document DB client Here
        }


        /// <summary>
        /// Get document as an asynchronous operation, with the partition key being added as a
        /// request option.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="partitionKey">The partition key for {{ cookiecutter.controlller_name }} records.</param>
        /// <returns>Task&lt;{{ cookiecutter.controlller_name }} Record&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">document Id</exception>
        public async Task<{{ cookiecutter.controlller_name }}> GetDocumentAsync(string documentId, string partitionKey)
        {
             if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            Uri documentUri = this.CreateDocumentUri(documentId);

            var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
            var document = await this.client.ReadDocumentAsync(documentUri, requestOptions);

            return ({{ cookiecutter.controlller_name }})(document.Resource as dynamic);
        }

    }

}