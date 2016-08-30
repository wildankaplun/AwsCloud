using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Amazon.EC2;
using Amazon.Runtime;
using AwsCloudWatch.Models;
using Amazon;
using Amazon.EC2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Web.Configuration;

namespace AwsCloudWatch.Controllers
{
    public class HomeController : Controller
    {
        private static string AccessKeyId;
        private static string SecretAccessKey;
        private static string AwsRegion;

        private IAmazonEC2 _amazonEc2Client;
        private IAmazonS3 _amazonS3Client;   

        // GET: Home
        public ActionResult Index(string id)
        {
            ViewBag.Title = "List Virtual Machines / Intances";
            //Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
            //config.AppSettings.Settings["AWSRegion"].Value = RegionEndpoint.APSoutheast1.SystemName;
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");

            AccessKeyId = ConfigurationManager.AppSettings["AmazonAccessKeyId"];
            SecretAccessKey = ConfigurationManager.AppSettings["AmazonSecretAccessKey"];
            //AwsRegion = ConfigurationManager.AppSettings["AWSRegion"];
            if (string.IsNullOrEmpty(id)) id = ConfigurationManager.AppSettings["AWSRegion"];
            RegionEndpoint AwsRegion = RegionEndpoint.APSoutheast1.SystemName.Equals(id) ? RegionEndpoint.APSoutheast1 : RegionEndpoint.APNortheast1;

            _amazonEc2Client = new AmazonEC2Client(GetAmazonCredentials(AccessKeyId, SecretAccessKey), RegionEndpoint.APSoutheast1.SystemName.Equals(id) ? RegionEndpoint.APSoutheast1 : RegionEndpoint.APNortheast1);
            _amazonS3Client = new AmazonS3Client(GetAmazonCredentials(AccessKeyId, SecretAccessKey), RegionEndpoint.APSoutheast1.SystemName.Equals(id) ? RegionEndpoint.APSoutheast1 : RegionEndpoint.APNortheast1);

            ListBucketsResponse response = _amazonS3Client.ListBuckets();
            
            List<Region> amazonRegions = GetAmazonRegions(_amazonEc2Client);
            List<AvailabilityZone> amazonAvaRegions = GetAmazonAvailabilityRegions(_amazonEc2Client);
            //int usersChoice = GetSelectedRegionOfUser(amazonRegions);
            Region selectedRegion = amazonRegions.FirstOrDefault(c => c.RegionName.Equals(AwsRegion.SystemName));
            var imagesInRegion = GetSuitableImages(_amazonEc2Client, selectedRegion);
            Image selectedImage = imagesInRegion.FirstOrDefault(c => c.ImageId.Equals("ami-21d30f42"));
            List<VirtualMachine> listInstances = LaunchImage(_amazonEc2Client, selectedImage, amazonRegions.FirstOrDefault(c => c.RegionName.Equals(AwsRegion)));

            var Owner = new OwnerInstance()
            {
                OwnerId = response.Owner.Id,
                OwnerDisplayName = response.Owner.DisplayName
            };

            var Vm = new VmView()
            {
                ListInsMachines = listInstances,
                Owner = Owner
            };

            // Dispose interface objects
            Dispose(_amazonEc2Client, _amazonS3Client);

            return View(Vm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static AWSCredentials GetAmazonCredentials(string accessKeyId, string secretAccessKey)
        {
            AWSCredentials awsCredentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            
            return awsCredentials;
        }        

        private static List<Region> GetAmazonRegions(IAmazonEC2 amazonEc2Client)
        {
            try
            {
                //var response = amazonEc2Client.DescribeAvailabilityZones();
                DescribeRegionsRequest describeRegionsRequest = new DescribeRegionsRequest();
                DescribeRegionsResponse describeRegionsResponse = amazonEc2Client.DescribeRegions(describeRegionsRequest);
                List<Region> regions = describeRegionsResponse.Regions;
                return regions;
            }
            catch
            {
                throw;
            }
        }

        private static List<AvailabilityZone> GetAmazonAvailabilityRegions(IAmazonEC2 amazonEc2Client)
        {
            try
            {
                DescribeAvailabilityZonesRequest descAvaRegionRequest = new DescribeAvailabilityZonesRequest();
                DescribeAvailabilityZonesResponse descAvaRegionResponse = amazonEc2Client.DescribeAvailabilityZones(descAvaRegionRequest);
                var response = descAvaRegionResponse.AvailabilityZones;

                return response;
            }
            catch
            {
                throw;
            }
        }

        private static int GetSelectedRegionOfUser(List<Region> amazonRegions)
        {
            Console.Write("Select a region: ");
            string selection = Console.ReadLine();
            int selectableMin = 1;
            int selectableMax = amazonRegions.Count;
            int selectedMenuPoint;
            bool validFormat = int.TryParse(selection, out selectedMenuPoint);
            while (!validFormat || (selectedMenuPoint < selectableMin || selectedMenuPoint > selectableMax))
            {
                Console.WriteLine("Invalid input.");
                Console.Write("Select a region: ");
                selection = Console.ReadLine();
                validFormat = int.TryParse(selection, out selectedMenuPoint);
            }

            return selectedMenuPoint;
        }

        private static List<Amazon.EC2.Model.Image> GetSuitableImages(IAmazonEC2 amazonEc2Client, Region selectedRegion)
        {
            try
            {
                DescribeImagesRequest imagesRequest = new DescribeImagesRequest()
                {
                    ImageIds = selectedRegion.RegionName.Equals(RegionEndpoint.APSoutheast1.SystemName) ? new List<string>() { "ami-21d30f42", "ami-d6f32ab5" } : new List<string>() { "" }
                };
                DescribeImagesResponse imagesResponse = amazonEc2Client.DescribeImages(imagesRequest);

                return imagesResponse.Images;
            }
            catch
            {
                return new List<Amazon.EC2.Model.Image>();
                //throw;
            }
        }

        private static List<Amazon.EC2.Model.Image> GetSuitableImagesFilter(IAmazonEC2 amazonEc2Client, Region selectedRegion)
        {
            try
            {
                DescribeImagesRequest imagesRequest = new DescribeImagesRequest();

                List<String> owners = new List<string>();
                owners.Add(ConfigurationManager.AppSettings["AmiSavOwnerId"]);
                owners.Add(ConfigurationManager.AppSettings["AmiGclOwnerId"]);
                owners.Add(ConfigurationManager.AppSettings["NewAmiOwnerId"]);
                imagesRequest.Owners = owners;

                Amazon.EC2.Model.Filter availabilityFilter = new Amazon.EC2.Model.Filter {Name = "state"};
                List<String> filterValues = new List<string> {"available"};
                availabilityFilter.Values = filterValues;
                List<Amazon.EC2.Model.Filter> filters = new List<Amazon.EC2.Model.Filter> {availabilityFilter};
                imagesRequest.Filters = filters;

                DescribeImagesResponse imagesResponse = amazonEc2Client.DescribeImages(imagesRequest);
                List<Amazon.EC2.Model.Image> images = imagesResponse.Images;
                return images;
            }
            catch
            {
                return new List<Amazon.EC2.Model.Image>();
                //throw;
            }
        }

        private static List<VirtualMachine> LaunchImage(IAmazonEC2 amazonEc2Client, Image selectedImage, Region selectedRegion)
        {
            try
            {
                DescribeInstancesRequest descInstanceRequest = new DescribeInstancesRequest();
                DescribeInstancesResponse descInstancesResponse = amazonEc2Client.DescribeInstances(descInstanceRequest);
                var descInstancesResult = descInstancesResponse.Reservations;

                return (from item in descInstancesResult.ToList()
                    from itemChild in item.Instances
                        select new VirtualMachine()
                    {
                        AmiId = itemChild.ImageId, 
                        InstanceId = itemChild.InstanceId, 
                        InstanceName = string.Empty, 
                        InstanceType = itemChild.InstanceType,
                        PrivateDns = itemChild.PrivateDnsName,
                        Status = itemChild.State.Name,
                        Platform = selectedImage.ImageId.Equals(itemChild.ImageId) && string.IsNullOrEmpty(itemChild.Platform) ? selectedImage.Name : (string)itemChild.Platform,
                        Monitoring = itemChild.Monitoring.State.Value
                    }).ToList();

            }
            catch
            {
                return new List<VirtualMachine> ();
                //throw;
            }
        }

        protected void Dispose(IAmazonEC2 ec2, IAmazonS3 s3)
        {
            if (ec2 != null && s3 != null)
            {
                ((IDisposable)ec2).Dispose();
                ((IDisposable)s3).Dispose();

            }
        }
    }
}