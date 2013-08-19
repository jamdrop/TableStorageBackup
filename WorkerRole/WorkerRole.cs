using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ServiceModel;
using System.ServiceModel.Web;
using AntsCode.TableStorageBackupLib;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        /// ServiceHost object for backup endpoint.
        private ServiceHost backupServiceHost;

        public override void Run()
        {
            try
            {
                // Start the backup service
                RoleInstanceEndpoint externalEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["BackupService"];
                
                this.backupServiceHost = BackupService.GetServiceHost(
                    BindingType.Tcp,
                    externalEndPoint.IPEndpoint.ToString(),
                    "22ec43b717d14be7a4e618610738a25f",
                    StoreLocation.LocalMachine,
                    StoreName.My,
                    "localhost");

                this.backupServiceHost.Open();

                System.Diagnostics.Trace.TraceInformation("Backup Service started on endpoint: " + externalEndPoint.IPEndpoint.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Could not start Backup Service: " + e.Message);
            }

            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            DiagnosticMonitor.Start("DiagnosticsConnectionString");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}