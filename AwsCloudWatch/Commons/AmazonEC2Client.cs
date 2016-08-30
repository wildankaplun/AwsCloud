using System;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;
using Amazon.EC2.Model;
using Amazon.EC2.Util;
using Amazon.EC2;

namespace AwsCloudWatch.Commons
{
    public class AmazonEC2Client
    {
        #region constructor
        private String awsAccessKeyId = null;
        private String awsSecretAccessKey = null;
        private AmazonEC2Config config = null;

        /// <summary>
        /// Constructs AmazonEC2Client with AWS Access Key ID and AWS Secret Key
        /// </summary>
        /// <param name="awsAccessKeyId">AWS Access Key ID</param>
        /// <param name="awsSecretAccessKey">AWS Secret Access Key</param>
        public AmazonEC2Client(String awsAccessKeyId, String awsSecretAccessKey)
            : this(awsAccessKeyId, awsSecretAccessKey, new AmazonEC2Config())
        {
        }


        /// <summary>
        /// Constructs AmazonEC2Client with AWS Access Key ID and AWS Secret Key
        /// </summary>
        /// <param name="awsAccessKeyId">AWS Access Key ID</param>
        /// <param name="awsSecretAccessKey">AWS Secret Access Key</param>
        /// <param name="config">configuration</param>
        public AmazonEC2Client(String awsAccessKeyId, String awsSecretAccessKey, AmazonEC2Config config)
        {
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            this.config = config;
        }

        // Public API ------------------------------------------------------------//


        /// <summary>
        /// Allocate Address 
        /// </summary>
        /// <param name="request">Allocate Address  request</param>
        /// <returns>Allocate Address  Response from the service</returns>
        /// <remarks>
        /// The AllocateAddress operation acquires an elastic IP address for use with your
        /// account.
        /// 
        /// </remarks>
        //public AllocateAddressResponse AllocateAddress(AllocateAddressRequest request)
        //{

        //    return (ConvertAllocateAddress(request));
        //}

        ///**
        // * Convert AllocateAddressRequest to name value pairs
        // */
        //private IDictionary<String, String> ConvertAllocateAddress(AllocateAddressRequest request)
        //{

        //    IDictionary<String, String> parameters = new Dictionary<String, String>();
        //    parameters.Add("Action", "AllocateAddress");

        //    return parameters;
        //}

        #endregion
    }
}