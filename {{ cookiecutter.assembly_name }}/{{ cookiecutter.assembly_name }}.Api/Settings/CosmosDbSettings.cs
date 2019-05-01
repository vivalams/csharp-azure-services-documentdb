//  -----------------------------------------------------------------------
//  
//  <copyright file="CosmosDbSettings.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api.Settings
{
    using System;
    using System.Diagnostics.CodeAnalysis;
	
    /// <summary>
    /// The settings for cosmos DB
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CosmosDbSettings 
    {
        /// <summary>
        /// Gets or sets the Collection type
        /// </summary>
        public string CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the database id (name of the cosmos DB)
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the collection id (name of the cosmos collection)
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets a value indicating whether to build collection automatically.
        /// </summary>
        /// <value><c>true</c> if build collection automatically; otherwise, <c>false</c>.</value>
        public bool BuildCollectionAutomatically => true;

        /// <summary>
        /// Gets or sets the partition key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the throughput levels
        /// </summary>
        public int? Throughput { get; set; }

        /// <summary>
        /// Gets or sets the authorization key for the cosmos account
        /// </summary>
        public string DocumentDbKey { get; set; }

        /// <summary>
        /// Gets or sets the uri for the document db instance
        /// </summary>
        public Uri DocumentDbUri { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of stored procedures names.
        /// </summary>
        public string StoredProcedureNames { get; set; }
    }
}
