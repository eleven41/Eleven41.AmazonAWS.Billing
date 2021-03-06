﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eleven41.AmazonAWS.Billing
{
	public class AwsLineItem
	{
		[XmlAttribute]
		public string Description { get; set; }

		[XmlAttribute]
		public double UsageQuantity { get; set; }

		[XmlAttribute]
		public string FormattedUsageQuantity { get; set; }

		[XmlAttribute]
		public string UsageType { get; set; }

		[XmlAttribute]
		public double CostBeforeTaxes { get; set; }

		[XmlAttribute]
		public string Region { get; set; }

		[XmlAttribute]
		public string RegionName { get; set; }

		[XmlAttribute]
		public string Category { get; set; }

		[XmlAttribute]
		public string CategoryName { get; set; }
	}
}
