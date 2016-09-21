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
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.ServiceRuntime;

#endregion

namespace Microsoft.AzureCat.Samples.Helpers
{
    [EventSource(Name = "TraceEventSource")]
    public sealed class TraceEventSource : EventSource
    {
        #region Internal Enums
        public class Keywords
        {
            public const EventKeywords EventHub = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Performance = (EventKeywords)8;
        }
        #endregion

        #region Public Static Properties
        public static readonly TraceEventSource Log = new TraceEventSource(); 
        #endregion

        #region Private Methods
        [Event(1,
       Message = "TraceIn",
       Keywords = Keywords.Diagnostic,
       Level = EventLevel.Verbose)]
        private void TraceIn(string application,
                             string instance,
                             Guid activityId,
                             string description,
                             string source,
                             string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance))
            {
                return;
            }
            WriteEvent(1, application, instance, activityId, description, source, method);
        }

        [Event(2,
               Message = "TraceOut",
               Keywords = Keywords.Diagnostic,
               Level = EventLevel.Verbose)]
        private void TraceOut(string application,
                              string instance,
                              Guid activityId,
                              string description,
                              string source,
                              string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance))
            {
                return;
            }
            WriteEvent(2, application, instance, activityId, description, source, method);
        }

        [Event(3,
               Message = "TraceApi",
               Keywords = Keywords.Diagnostic,
               Level = EventLevel.Informational)]
        private void TraceExec(string application,
                               string instance,
                               Guid activityId,
                               double elapsed,
                               string description,
                               string source,
                               string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(description))
            {
                return;
            }
            WriteEvent(3, application, instance, activityId, elapsed, description, source, method);
        }

        [Event(4,
               Message = "TraceInfo",
               Keywords = Keywords.Diagnostic,
               Level = EventLevel.Informational)]
        private void TraceInfo(string application,
                               string instance,
                               string description,
                               string source,
                               string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(description))
            {
                return;
            }
            WriteEvent(4, application, instance, description, source, method);
        }

        [Event(5,
               Message = "TraceError",
               Keywords = Keywords.Diagnostic,
               Level = EventLevel.Error)]
        private void TraceError(string application,
                                string instance,
                                Guid activityId,
                                string exception,
                                string innerException,
                                string source,
                                string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(exception))
            {
                return;
            }
            WriteEvent(5, application, instance, activityId, exception, string.IsNullOrWhiteSpace(innerException) ? string.Empty : innerException, source, method);
        }

        [Event(6,
               Message = "OpenPartition",
               Keywords = Keywords.EventHub,
               Level = EventLevel.Informational)]
        private void OpenPartition(string application, 
                                   string instance, 
                                   string eventHub, 
                                   string consumerGroup, 
                                   string partitionId, 
                                   string source, 
                                   string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            WriteEvent(6, application, instance, eventHub, consumerGroup, partitionId, source, method);
        }

        [Event(7,
               Message = "ClosePartition",
               Keywords = Keywords.EventHub,
               Level = EventLevel.Informational)]
        private void ClosePartition(string application, 
                                    string instance, 
                                    string eventHub, 
                                    string consumerGroup, 
                                    string partitionId, 
                                    string reason, 
                                    string source, 
                                    string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            WriteEvent(7, application, instance, eventHub, consumerGroup, partitionId, reason, source, method);
        }

        [Event(8,
               Message = "ProcessEvents",
               Keywords = Keywords.EventHub,
               Level = EventLevel.Informational)]
        private void ProcessEvents(string application, 
                                   string instance, 
                                   string eventHub, 
                                   string consumerGroup, 
                                   string partitionId, 
                                   int messageCount, 
                                   string source, 
                                   string method)
        {
            if (string.IsNullOrWhiteSpace(application) ||
                string.IsNullOrWhiteSpace(instance) ||
                string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            if (IsEnabled())
            {
                WriteEvent(8, application, instance, eventHub, consumerGroup, partitionId, messageCount, source, method);
            }
        } 
        #endregion

        #region Public Methods
        [NonEvent]
        public void TraceApi(TimeSpan elapsed,
                             string description,
                             [CallerFilePath] string source = "",
                             [CallerMemberName] string method = "")
        {
            if (IsEnabled())
            {
                TraceExec(RoleEnvironment.CurrentRoleInstance.Role.Name,
                          RoleEnvironment.CurrentRoleInstance.Id,
                          Guid.NewGuid(),
                          elapsed.TotalMilliseconds,
                          description,
                          source,
                          method);
            }
        }

        [NonEvent]
        public void TraceApi(Guid activityId,
                             TimeSpan elapsed,
                             string description,
                             [CallerFilePath] string source = "",
                             [CallerMemberName] string method = "")
        {
            if (IsEnabled())
            {
                TraceExec(RoleEnvironment.CurrentRoleInstance.Role.Name,
                          RoleEnvironment.CurrentRoleInstance.Id,
                          activityId,
                          elapsed.TotalMilliseconds,
                          description,
                          source,
                          method);
            }
        }

        [NonEvent]
        public void TraceIn([CallerFilePath] string source = "",
                            [CallerMemberName] string method = "")
        {
            if (IsEnabled())
            {
                TraceIn(RoleEnvironment.CurrentRoleInstance.Role.Name,
                        RoleEnvironment.CurrentRoleInstance.Id,
                        Guid.NewGuid(),
                        string.Empty,
                        source,
                        method);
            }
        }

        [NonEvent]
        public void TraceOut([CallerFilePath] string source = "",
                             [CallerMemberName] string method = "")
        {
            if (IsEnabled())
            {
                TraceOut(RoleEnvironment.CurrentRoleInstance.Role.Name,
                         RoleEnvironment.CurrentRoleInstance.Id,
                         Guid.NewGuid(),
                         string.Empty,
                         source,
                         method);
            }
        }

        [NonEvent]
        public void TraceInfo(string description, 
                              [CallerFilePath] string source = "", 
                              [CallerMemberName] string method = "")
        {
            if (IsEnabled())
            {
                TraceInfo(RoleEnvironment.CurrentRoleInstance.Role.Name,
                          RoleEnvironment.CurrentRoleInstance.Id,
                          description,
                          source,
                          method);
            }
        }

        [NonEvent]
        public void TraceError(string exception, 
                               string innerException,
                               [CallerFilePath] string source = "", 
                               [CallerMemberName] string method = "")
        {
            if (string.IsNullOrWhiteSpace(exception))
            {
                return;
            }
            if (IsEnabled())
            {
                TraceError(RoleEnvironment.CurrentRoleInstance.Role.Name,
                           RoleEnvironment.CurrentRoleInstance.Id,
                           Guid.NewGuid(),
                           exception, 
                           string.IsNullOrWhiteSpace(innerException) ? string.Empty : innerException,
                           source,
                           method);
            }
        }

       [NonEvent]
        public void OpenPartition(string eventHub, 
                                  string consumerGroup, 
                                  string partitionId,
                                  [CallerFilePath] string source = "",
                                  [CallerMemberName] string method = "")
        {
            if (string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            if (IsEnabled())
            {
                OpenPartition(RoleEnvironment.CurrentRoleInstance.Role.Name,
                              RoleEnvironment.CurrentRoleInstance.Id,
                              eventHub,
                              consumerGroup,
                              partitionId,
                              source,
                              method);
            }
        }

        [NonEvent]
       public void ClosePartition(string eventHub, 
                                  string consumerGroup, 
                                  string partitionId,
                                  string reason,
                                  [CallerFilePath] string source = "",
                                  [CallerMemberName] string method = "")
        {
            if (string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            if (IsEnabled())
            {
                ClosePartition(RoleEnvironment.CurrentRoleInstance.Role.Name,
                               RoleEnvironment.CurrentRoleInstance.Id,
                               eventHub,
                               consumerGroup,
                               partitionId,
                               reason,
                               source,
                               method);
            }
        }

        [NonEvent]
        public void ProcessEvents(string eventHub, 
                                  string consumerGroup, 
                                  string partitionId, 
                                  int messageCount,
                                  [CallerFilePath] string source = "",
                                  [CallerMemberName] string method = "")
        {
            if (string.IsNullOrWhiteSpace(eventHub) ||
                string.IsNullOrWhiteSpace(consumerGroup) ||
                string.IsNullOrWhiteSpace(partitionId))
            {
                return;
            }
            if (IsEnabled())
            {
                ProcessEvents(RoleEnvironment.CurrentRoleInstance.Role.Name,
                              RoleEnvironment.CurrentRoleInstance.Id,
                              eventHub,
                              consumerGroup,
                              partitionId,
                              messageCount,
                              source,
                              method);
            }
        }

        #endregion
    }
}