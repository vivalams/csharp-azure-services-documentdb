//  -----------------------------------------------------------------------
//  
//  <copyright file="KeyVaultConfig.cs" company="Microsoft">
//  
//  Copyright (c) Microsoft. All rights reserved.
// 
//  </copyright>
// 
//  -----------------------------------------------------------------------
namespace {{ cookiecutter.assembly_name }}.Api.Settings
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class KeyVaultConfig.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class KeyVaultConfig
    {
        /// <summary>
        /// Gets or sets the vault base URI.
        /// </summary>
        /// <value>The vault base URI.</value>
        public string VaultBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether remote keyvault values should be used.
        /// </summary>
        /// <value><c>true</c> if remote keyvault configuration values should be used;
        /// otherwise, <c>false</c>.</value>
        public bool EnableSecureConfig { get; set; }
    }
}