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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.AzureCat.Samples.Helpers;
#endregion

namespace Microsoft.AzureCat.Samples.EventProcessorHostWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        #region Private Constants
        //*******************************
        // Messages & Formats
        //*******************************
        private const string RoleEnvironmentSettingFormat = "Configuration Setting [{0}] = [{1}].";
        private const string RoleEnvironmentConfigurationSettingChangedFormat = "The setting [{0}] is changed: new value = [{1}].";
        private const string RoleEnvironmentConfigurationSettingChangingFormat = "The setting [{0}] is changing: old value = [{1}].";
        private const string RoleEnvironmentTopologyChangedFormat = "The  topology for the [{0}] role is changed.";
        private const string RoleEnvironmentTopologyChangingFormat = "The  topology for the [{0}] role is changing.";
        private const string RoleInstanceCountFormat = "[Role {0}] instance count = [{1}].";
        private const string RoleInstanceEndpointCountFormat = "[Role {0}] instance endpoints count = [{1}].";
        private const string RoleInstanceEndpointFormat = "[Role {0}] instance endpoint [{1}]: protocol = [{2}] address = [{3}] port = [{4}].";
        private const string RoleInstanceIdFormat = "[Role {0}] instance Id = [{1}].";
        private const string RoleInstanceStatusFormat = "[Role {0}] instance Id = [{1}] Status = [{2}].";
        private const string Unknown = "Unknown";
        private const string RegisteringEventProcessor = "Registering Event Processor [EventProcessor]... ";
        private const string EventProcessorRegistered = "Event Processor [EventProcessor] successfully registered. ";

        //*******************************
        // Settings
        //*******************************
        private const string SqlDatabaseConnectionStringSetting = "SqlDatabaseConnectionString";
        private const string InsertStoredProcedureSetting = "InsertStoredProcedure";
        private const string StorageAccountConnectionStringSetting = "StorageAccountConnectionString";
        private const string ServiceBusConnectionStringSetting = "ServiceBusConnectionString";
        private const string EventHubNameSetting = "EventHubName";
        private const string ConsumerGroupNameSetting = "ConsumerGroupName";
        #endregion

        #region Private Fields
        private string eventHubName;
        private string consumerGroupName;
        private string sqlDatabaseConnectionString;
        private string insertStoredProcedure;
        private string storageAccountConnectionString;
        private string serviceBusConnectionString;
        private EventProcessorHost eventProcessorHost;
        #endregion

        #region Public Methods
        public override void Run()
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();
                
                while (true)
                {
                    Thread.Sleep(10000);
                }
                // ReSharper disable once FunctionNeverReturns
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

        public override bool OnStart()
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Set Default values for the ServicePointManager
                ServicePointManager.UseNagleAlgorithm = false;
                ServicePointManager.Expect100Continue = false;

                // Setting RoleEnvironment event handlers
                RoleEnvironment.Changed += RoleEnvironment_Changed;
                RoleEnvironment.Changing += RoleEnvironment_Changing;
                RoleEnvironment.StatusCheck += RoleEnvironment_StatusCheck;
                RoleEnvironment.Stopping += RoleEnvironment_Stopping;

                // Read Configuration Settings
                ReadConfigurationSettings();

                // Start Event Processor
                StartEventProcessorAsync().Wait();

                // Run base.OnStart method
                return base.OnStart();
            }
            catch (Exception ex)
            {
                // Trace Exception
                TraceEventSource.Log.TraceError(ex.Message, ex.InnerException?.Message ?? string.Empty);
                return false;
            }
            finally
            {
                // TraceOut
                TraceEventSource.Log.TraceOut();
            }
        }
        #endregion

        #region Private Methods

        private void ReadConfigurationSettings()
        {
            // Read sql database connectionstring setting  
            sqlDatabaseConnectionString = CloudConfigurationHelper.GetSetting(SqlDatabaseConnectionStringSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        SqlDatabaseConnectionStringSetting,
                                                        sqlDatabaseConnectionString));

            // Read insert stored procedure setting  
            insertStoredProcedure = CloudConfigurationHelper.GetSetting(InsertStoredProcedureSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        InsertStoredProcedureSetting,
                                                        insertStoredProcedure));

            // Read storage account connectionstring setting  
            storageAccountConnectionString = CloudConfigurationHelper.GetSetting(StorageAccountConnectionStringSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        StorageAccountConnectionStringSetting,
                                                        storageAccountConnectionString));

            // Read service bus connectionstring setting  
            serviceBusConnectionString = CloudConfigurationHelper.GetSetting(ServiceBusConnectionStringSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        ServiceBusConnectionStringSetting,
                                                        serviceBusConnectionString));
            
            // Read event hub name setting
            eventHubName = CloudConfigurationHelper.GetSetting(EventHubNameSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        ServiceBusConnectionStringSetting,
                                                        serviceBusConnectionString));

            // Read event consumer group name setting
            consumerGroupName = CloudConfigurationHelper.GetSetting(ConsumerGroupNameSetting);
            TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentSettingFormat,
                                                        ServiceBusConnectionStringSetting,
                                                        serviceBusConnectionString));
        }

        private async Task StartEventProcessorAsync()
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();
                var eventHubClient = EventHubClient.CreateFromConnectionString(serviceBusConnectionString, eventHubName);

                // Get the default Consumer Group
                eventProcessorHost = new EventProcessorHost(RoleEnvironment.CurrentRoleInstance.Id,
                                                            eventHubClient.Path.ToLower(),
                                                            consumerGroupName.ToLower(),
                                                            serviceBusConnectionString,
                                                            storageAccountConnectionString)
                {
                    PartitionManagerOptions = new PartitionManagerOptions
                    {
                        AcquireInterval = TimeSpan.FromSeconds(10), // Default is 10 seconds
                        RenewInterval = TimeSpan.FromSeconds(10), // Default is 10 seconds
                        LeaseInterval = TimeSpan.FromSeconds(30) // Default value is 30 seconds
                    }
                };
                TraceEventSource.Log.TraceInfo(RegisteringEventProcessor);
                var eventProcessorOptions = new EventProcessorOptions
                {
                    InvokeProcessorAfterReceiveTimeout = true,
                    MaxBatchSize = 100,
                    PrefetchCount = 100,
                    ReceiveTimeOut = TimeSpan.FromSeconds(30),
                };
                eventProcessorOptions.ExceptionReceived += eventProcessorOptions_ExceptionReceived;
                await eventProcessorHost.RegisterEventProcessorFactoryAsync(new EventProcessorFactory<EventProcessor>(sqlDatabaseConnectionString, insertStoredProcedure),
                                                                            eventProcessorOptions);
                TraceEventSource.Log.TraceInfo(EventProcessorRegistered);
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

        void eventProcessorOptions_ExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            if (e?.Exception == null)
            {
                return;
            }

            // Trace Exception
            TraceEventSource.Log.TraceError(e.Exception.Message, e.Exception.InnerException?.Message ?? string.Empty);
        }

        /// <summary>
        /// Occurs after a change to the service configuration is applied to the running instances of a role.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Represents the arguments for the Changed event, which occurs after a configuration change has been applied to a role instance.</param>
        private static void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Get the list of configuration setting changes
                var settingChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();

                foreach (var settingChange in settingChanges)
                {
                    var value = CloudConfigurationHelper.GetSetting(settingChange.ConfigurationSettingName);
                    TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentConfigurationSettingChangedFormat,
                                                                            settingChange.ConfigurationSettingName ?? string.Empty,
                                                                            string.IsNullOrEmpty(value) ? string.Empty : value));
                }

                // Get the list of configuration changes
                var topologyChanges = e.Changes.OfType<RoleEnvironmentTopologyChange>();

                foreach (var roleName in topologyChanges.Select(topologyChange => topologyChange.RoleName))
                {
                    TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentTopologyChangedFormat,string.IsNullOrEmpty(roleName) ? Unknown : roleName));
                    if (string.IsNullOrEmpty(roleName))
                    {
                        continue;
                    }
                    var role = RoleEnvironment.Roles[roleName];
                    if (role == null)
                    {
                        continue;
                    }
                    TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceCountFormat, roleName, role.Instances.Count));
                    foreach (var roleInstance in role.Instances)
                    {
                        TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceIdFormat, roleName, roleInstance.Id));
                        TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceEndpointCountFormat, roleName, roleInstance.InstanceEndpoints.Count));
                        foreach (var instanceEndpoint in roleInstance.InstanceEndpoints)
                        {
                            TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceEndpointFormat,
                                                                            roleName,
                                                                            instanceEndpoint.Key,
                                                                            instanceEndpoint.Value.Protocol,
                                                                            instanceEndpoint.Value.IPEndpoint.Address,
                                                                            instanceEndpoint.Value.IPEndpoint.Port));
                        }
                    }
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

        /// <summary>
        /// Occurs before a change to the service configuration is applied to the running instances of a role. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">presents the arguments for the Changing event, which occurs before a configuration change is applied to a role instance. </param>
        private static void RoleEnvironment_Changing(object sender, RoleEnvironmentChangingEventArgs e)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Get the list of configuration setting changes
                var settingChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();

                foreach (var settingChange in settingChanges)
                {
                    var value = CloudConfigurationHelper.GetSetting(settingChange.ConfigurationSettingName);
                    TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentConfigurationSettingChangingFormat,
                                                                    settingChange.ConfigurationSettingName,
                                                                    string.IsNullOrEmpty(value) ? string.Empty : value));
                }

                // Get the list of configuration changes
                var topologyChanges = e.Changes.OfType<RoleEnvironmentTopologyChange>();

                foreach (var roleName in topologyChanges.Select(topologyChange => topologyChange.RoleName))
                {
                    TraceEventSource.Log.TraceInfo(string.Format(RoleEnvironmentTopologyChangingFormat,
                                                                    string.IsNullOrEmpty(roleName) ? Unknown : roleName));
                    if (string.IsNullOrEmpty(roleName))
                    {
                        continue;
                    }
                    var role = RoleEnvironment.Roles[roleName];
                    if (role == null)
                    {
                        continue;
                    }
                    TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceCountFormat, roleName, role.Instances.Count));
                    foreach (var roleInstance in role.Instances)
                    {
                        TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceIdFormat, roleName, roleInstance.Id));
                        TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceEndpointCountFormat, roleName, roleInstance.InstanceEndpoints.Count));
                        foreach (var instanceEndpoint in roleInstance.InstanceEndpoints)
                        {
                            TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceEndpointFormat,
                                                                            roleName,
                                                                            instanceEndpoint.Key,
                                                                            instanceEndpoint.Value.Protocol,
                                                                            instanceEndpoint.Value.IPEndpoint.Address,
                                                                            instanceEndpoint.Value.IPEndpoint.Port));
                        }
                    }
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

        /// <summary>
        /// Occurs at a regular interval to indicate the status of a role instance. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Represents the arguments for the StatusCheck event, which occurs at a regular interval to indicate the status of a role instance.</param>
        private static void RoleEnvironment_StatusCheck(object sender, RoleInstanceStatusCheckEventArgs e)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();

                // Write Role Instance Status
                TraceEventSource.Log.TraceInfo(string.Format(RoleInstanceStatusFormat,
                                                                RoleEnvironment.CurrentRoleInstance.Role.Name,
                                                                RoleEnvironment.CurrentRoleInstance.Id,
                                                                e.Status));
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

        /// <summary>
        /// Occurs when a role instance is about to be stopped. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Represents the arguments for the Stopping event, which occurs when a role instance is being stopped. </param>
        private static void RoleEnvironment_Stopping(object sender, RoleEnvironmentStoppingEventArgs e)
        {
            try
            {
                // TraceIn
                TraceEventSource.Log.TraceIn();
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
    }
}
