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
using Microsoft.ServiceBus.Messaging;
#endregion

namespace Microsoft.AzureCat.Samples.EventProcessorHostWorkerRole
{
    public class EventProcessorFactory<T> : IEventProcessorFactory where T : class, IEventProcessor
    {
        #region Private Fields
        private readonly T instance;
        private readonly string sqlDatabaseConnectionString;
        private readonly string insertStoredProcedure;
        #endregion

        #region Public Constructors
        public EventProcessorFactory()
        {
            sqlDatabaseConnectionString = null;
        }

        public EventProcessorFactory(string sqlDatabaseConnectionString, string insertStoredProcedure)
        {
            this.sqlDatabaseConnectionString = sqlDatabaseConnectionString;
            this.insertStoredProcedure = insertStoredProcedure;
        }

        public EventProcessorFactory(T instance)
        {
            this.instance = instance;
        }
        #endregion

        #region IEventProcessorFactory Methods
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return instance ?? Activator.CreateInstance(typeof(T), sqlDatabaseConnectionString, insertStoredProcedure) as T;
        }
        #endregion
    }
}
