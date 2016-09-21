#region Copyright
//=======================================================================================
// Microsoft Azure Customer Advisory Team  
//
// This sample is supplemental to the technical guidance published on the community
// blog at http://blogs.msdn.com/b/paolos/. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright © 2016 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure;
#endregion

namespace Microsoft.AzureCat.Samples.Helpers
{
    /// <summary>
    /// This class contains some extensions to the RoleEnvironment class
    /// </summary>
    public class CloudConfigurationHelper
    {
        /// <summary>
        /// Same as RoleEnvironmentHelper.GetConfigurationSettingValue()
        /// but returns String.Empty instead of throwing an exception
        /// </summary>
        /// <param name="setting">The name of the configuration setting.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A string containing the value of the configuration setting, or a null reference if the specified setting was not found.</returns>
        public static string GetSetting(string setting, string defaultValue = null)
        {
            try
            {
                return CloudConfigurationManager.GetSetting(setting);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            { }
            return defaultValue;
        }
    }
}
