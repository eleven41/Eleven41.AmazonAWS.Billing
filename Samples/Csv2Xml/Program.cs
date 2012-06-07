using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Csv2Xml
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Csv2Xml <input csv> <output xml>");
				return;
			}

			string inputCsv = args[0];
			string outputXml = args[1];
			
			var statement = Eleven41.AmazonAWS.Billing.BillingCsv.ReadFile(inputCsv);

			using (var fileStream = new FileStream(outputXml, FileMode.Create))
			{
				SerializeToStream(statement, fileStream);
				fileStream.Close();
			}
		}

		static void SerializeToStream<T>(T obj, Stream stream)
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
			serializer.Serialize(stream, obj);
		}
	}
}
