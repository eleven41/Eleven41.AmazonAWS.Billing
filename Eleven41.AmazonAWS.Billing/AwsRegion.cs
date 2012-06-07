using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eleven41.AmazonAWS.Billing
{
	public class AwsRegion
	{
		[XmlAttribute]
		public string RegionCode { get; set; }

		[XmlAttribute]
		public string RegionName { get; set; }

		public List<AwsLineItem> Items { get; set; }
	}
}
