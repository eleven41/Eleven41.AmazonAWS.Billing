using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eleven41.AmazonAWS.Billing
{
	public class AwsProduct
	{
		[XmlAttribute]
		public string ProductCode { get; set; }

		[XmlAttribute]
		public string ProductName { get; set; }

		public List<AwsRegion> Regions { get; set; }

		public List<AwsLineItem> NonRegionItems { get; set; }
	}
}
