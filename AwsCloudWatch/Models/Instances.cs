using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.EC2.Model;

namespace AwsCloudWatch.Models
{
    public class VmView
    {
        public List<VirtualMachine> ListInsMachines;
        public OwnerInstance Owner;
    }

    public class VirtualMachine
    {
        public string AmiId { get; set; }
        public string RootDevice { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string InstanceId { get; set; }
        public string InstanceName { get; set; }
        public string InstanceType { get; set; }
        public string PrivateDns { get; set; }
        public string Platform { get; set; }
        public string Monitoring { get; set; }

        public Placement Placement { get; set; }

    }

    public class OwnerInstance
    {
        public string OwnerId { get; set; }
        public string OwnerDisplayName { get; set; }
    }

    public class PlacementInstance
    {
        public string Affinity { get; set; }
        public string AvailabilityZone { get; set; }
        public string GroupName { get; set; }
        public string Tenancy { get; set; }
    }
}