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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.AzureCat.Samples.Helpers;
#endregion

namespace Microsoft.AzureCat.Samples.EventProcessorHostWorkerRole
{
    public class EventProcessor : IEventProcessor
    {
        #region Private Fields
        private readonly string sqlDatabaseConnectionString;
        private readonly string insertStoredProcedure;
        #endregion

        #region Public Constructors
        public EventProcessor(string sqlDatabaseConnectionString, string insertStoredProcedure)
        {
            this.sqlDatabaseConnectionString = sqlDatabaseConnectionString;
            this.insertStoredProcedure = insertStoredProcedure;
        }
        #endregion

        #region IEventProcessor Methods
        public Task OpenAsync(PartitionContext context)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Trace Open Partition
                TraceEventSource.Log.OpenPartition(context.EventHubPath,
                                                   context.ConsumerGroupName,
                                                   context.Lease.PartitionId);
            }
            catch (Exception ex)
            {
                // Trace Exception
                TraceEventSource.Log.TraceError(ex.Message, ex.InnerException?.Message ?? string.Empty);
            }
            finally
            {
                // TraceOut
                TraceEventSource.Log.TraceOut();
            }
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                if (events == null)
                {
                    return;
                }

                var eventList = events.Select(e => Encoding.UTF8.GetString(e.GetBytes())).ToList();

                if (!eventList.Any())
                {
                    return;
                }

                // Trace Process Events
                TraceEventSource.Log.ProcessEvents(context.EventHubPath,
                                                   context.ConsumerGroupName,
                                                   context.Lease.PartitionId,
                                                   eventList.Count);

                using (var sqlConnection = new SqlConnection(sqlDatabaseConnectionString))
                {
                    await sqlConnection.OpenAsync();

                    // Create command
                    var sqlCommand = new SqlCommand(insertStoredProcedure, sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add table-valued parameter
                    sqlCommand.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@Events",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = -1,
                        Value = GetJsonArray(eventList)
                    });

                    // Execute the query
                    await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                // Trace Exception
                TraceEventSource.Log.TraceError(ex.Message, ex.InnerException?.Message ?? string.Empty);
            }
            finally
            {
                // TraceOut
                TraceEventSource.Log.TraceOut();
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Trace Open Partition
                TraceEventSource.Log.ClosePartition(context.EventHubPath,
                                                    context.ConsumerGroupName,
                                                    context.Lease.PartitionId,
                                                    reason.ToString());

                if (reason == CloseReason.Shutdown)
                {
                    await context.CheckpointAsync();
                }
            }
            catch (Exception ex)
            {
                // Trace Exception
                TraceEventSource.Log.TraceError(ex.Message, ex.InnerException?.Message ?? string.Empty);
            }
            finally
            {
                // TraceOut
                TraceEventSource.Log.TraceOut();
            }
        }
        #endregion

        #region Private Methods
        private string GetJsonArray(IReadOnlyList<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return null;
            }
            var builder = new StringBuilder("[");
            for (var i = 0; i < list.Count; i++)
            {
                builder.Append(i == 0 ? list[0] : $",{list[i]}");
            }
            builder.Append("]");
            return builder.ToString();
        }
        #endregion
    }
}