using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace api_cloud_azure
{
    /// <summary>
    /// Class to manage Azure VMs
    /// </summary>
    public class AzureVM
    {
        public IAzure Azure { get; set; }
        public string? VmId { get; set; }
        public string? RgId { get; set; }
        public INetworkInterface? Nic { get; set; }
        public string RgName { get; set; }
        public string VmName { get; set; }

        private Region Location { get; set; }
        private string VNetName { get; set; }
        private string VNetAddress { get; set; }
        private string SubnetName { get; set; }
        private string SubnetAddress { get; set; }
        private string NicName { get; set; }
        private string AdminUser { get; set; }
        private string AdminPassword { get; set; }
        private string PublicIPName { get; set; }
        private string NsgName { get; set; }

        private List<string> LogMessages { get; set; }

        public AzureVM(string name)
        {
            RgName = $"{name}-rg";
            VmName = $"{name}-vm";

            LogMessages = new List<string>();

            Location = Region.FranceCentral;
            VNetName = "VNET-Fluent";
            VNetAddress = "172.16.0.0/16";
            SubnetName = "Subnet-Fluent";
            SubnetAddress = "172.16.0.0/24";
            NicName = "NIC-Fluent";
            AdminUser = "azureadminuser";
            AdminPassword = "Pas$m0rd$123";
            PublicIPName = "publicIP-Fluent";
            NsgName = "NSG-Fluent";

            // Authenticate with Azure
            var credentials = SdkContext.AzureCredentialsFactory.FromFile("./azure-configuration.json");
            this.Azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(credentials).WithDefaultSubscription();
        }

        /// <summary>
        /// method to create an Azure VM
        /// </summary>
        /// <param name="os"></param>
        public void CreateAzureVM(string os)
        {
            var resourceGroup = Azure.ResourceGroups.Define(RgName)
                .WithRegion(Location)
                .Create();

            RgId = resourceGroup.Id;

            LogMessage($"Creating virtual network {VNetName} ...");
            var network = Azure.Networks.Define(VNetName)
                .WithRegion(Location)
                .WithExistingResourceGroup(RgName)
                .WithAddressSpace(VNetAddress)
                .WithSubnet(SubnetName, SubnetAddress)
                .Create();

            LogMessage($"Creating public IP {PublicIPName} ...");
            var publicIP = Azure.PublicIPAddresses.Define(PublicIPName)
                .WithRegion(Location)
                .WithExistingResourceGroup(RgName)
                .Create();

            LogMessage($"Creating Network Security Group {NsgName} ...");
            var nsg = Azure.NetworkSecurityGroups.Define(NsgName)
                .WithRegion(Location)
                .WithExistingResourceGroup(RgName)
                .Create();

            LogMessage($"Creating a Security Rule for allowing the remote");
            nsg.Update()
                .DefineRule("Allow-RDP")
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(3389)
                .WithProtocol(SecurityRuleProtocol.Tcp)
                .WithPriority(100)
                .Attach()
                .Apply();

            LogMessage($"Creating a Security Rule for allowing SSH");
            nsg.Update()
                .DefineRule("Allow-SSH")
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(22)
                .WithProtocol(SecurityRuleProtocol.Tcp)
                .WithPriority(101)
                .Attach()
                .Apply();

            LogMessage($"Creating network interface {NicName} ...");
            Nic = Azure.NetworkInterfaces.Define(NicName)
                .WithRegion(Location)
                .WithExistingResourceGroup(RgName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(SubnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIP)
                .WithExistingNetworkSecurityGroup(nsg)
                .Create();

            switch (os)
            {
                case "windows":
                    LogMessage($"Creating virtual machine {VmName} ...");
                    Azure.VirtualMachines.Define(VmName)
                        .WithRegion(Location)
                        .WithExistingResourceGroup(RgName)
                        .WithExistingPrimaryNetworkInterface(Nic)
                        .WithLatestWindowsImage("MicrosoftWindowsDesktop", "Windows-10", "win10-22h2-pro-g2")
                        .WithAdminUsername(AdminUser)
                        .WithAdminPassword(AdminPassword)
                        .WithComputerName(VmName)
                        .WithSize(ExpandableStringEnum<VirtualMachineSizeTypes>.Parse("Standard_B1ls"))
                        .Create();

                    var windowsVM = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);
                    VmId = windowsVM.Id;
                    break;

                case "ubuntu":
                    LogMessage($"Creating virtual machine {VmName} ...");
                    Azure.VirtualMachines.Define(VmName)
                        .WithRegion(Location)
                        .WithExistingResourceGroup(RgName)
                        .WithExistingPrimaryNetworkInterface(Nic)
                        .WithLatestLinuxImage("Canonical", "UbuntuServer", "18.04-LTS")
                        .WithRootUsername(AdminUser)
                        .WithRootPassword(AdminPassword)
                        .WithComputerName(VmName)
                        .WithSize(ExpandableStringEnum<VirtualMachineSizeTypes>.Parse("Standard_B1ls"))
                        .Create();

                    var linuxVM = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);
                    VmId = linuxVM.Id;
                    break;

                default: // debian
                    LogMessage($"Creating virtual machine {VmName} ...");
                    Azure.VirtualMachines.Define(VmName)
                        .WithRegion(Location)
                        .WithExistingResourceGroup(RgName)
                        .WithExistingPrimaryNetworkInterface(Nic)
                        .WithLatestLinuxImage("Debian", "debian-10", "10")
                        .WithRootUsername(AdminUser)
                        .WithRootPassword(AdminPassword)
                        .WithComputerName(VmName)
                        .WithSize(ExpandableStringEnum<VirtualMachineSizeTypes>.Parse("Standard_B1ls"))
                        .Create();

                    var debianVM = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);
                    VmId = debianVM.Id;
                    break;
            }

        }

        /// <summary>
        /// method to get the public IP address of the VM
        /// </summary>
        /// <returns>IP address of the VM</returns>
        public string GetIP()
        {
            INetworkInterface ni = Azure.NetworkInterfaces.GetByResourceGroup(RgName, NicName);

            var ipConfiguration = ni.IPConfigurations.Values.FirstOrDefault();
            string? ipAddress = "0.0.0.0";

            if (ipConfiguration != null)
            {
                var publicIPAddress = ipConfiguration.GetPublicIPAddress();

                if (publicIPAddress != null)
                {
                    ipAddress = publicIPAddress.IPAddress;
                }
            }

            return ipAddress;
        }

        /// <summary>
        /// method to check the status of the VM
        /// </summary>
        /// <returns>The status of the VM</returns>
        /// <example>Stopped/Running</example>
        public string CheckVMStatus()
        {
            IVirtualMachine vm = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);

            return ($"{vm.PowerState}");
        }

        /// <summary>
        /// method to stopped the VM
        /// </summary>
        /// <returns>Message confirmation: Currently VM {Id} is stopped</returns>
        public string ShutDownVM()
        {
            IVirtualMachine vm = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);
            vm.PowerOff();

            return ($"Currently VM {vm.Id} is stopped");
        }

        /// <summary>
        /// method to start the VM
        /// </summary>
        /// <returns>Message confirmation: Currently VM {Id} is restart</returns>
        public string RestartVM()
        {
            IVirtualMachine vm = Azure.VirtualMachines.GetByResourceGroup(RgName, VmName);
            vm.Start();

            return ($"Currently VM {vm.Id} is restart");
        }

        /// <summary>
        /// method to delete the VM
        /// </summary>
        public void DeleteAzureVM()
        {
            Azure.VirtualMachines.DeleteByResourceGroup(RgName, VmName);

            LogMessage($"Deletion in progress...");
            LogMessage($"Virtual machine {VmName} in resource group {RgName} has been deleted successfully.");
        }

        /// <summary>
        /// method to delete the Resource Group
        /// </summary>
        public void DeleteAzureRG()
        {
            Azure.ResourceGroups.DeleteByName(RgName);

            LogMessage($"Deletion in progress...");
            LogMessage($"Resource group {RgName} has been deleted successfully.");
        }

        /// <summary>
        /// method to add log messages
        /// </summary>
        /// <param name="message"></param>
        private void LogMessage(string message)
        {
            LogMessages.Add(message);
        }

        /// <summary>
        /// method to get log messages
        /// </summary>
        /// <returns></returns>
        public string GetLogMessages()
        {
            return string.Join(Environment.NewLine, LogMessages);
        }

        /// <summary>
        /// method to create and delete Azure resources with delay
        /// </summary>
        /// <param name="os"></param>
        public async Task CreateAndDeleteAzureResourcesWithDelay(string os, int delay = 10)
        {
            CreateAzureVM(os);

            // Attente de 15 minutes avant d'appeler DeleteAzureRG()
            await Task.Delay(TimeSpan.FromMinutes(delay));

            DeleteAzureRG();
        }

    }
}
