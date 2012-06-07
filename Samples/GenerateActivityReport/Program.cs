using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;

namespace GenerateActivityReport
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
			string outputHtml = args[1];

			var statement = Eleven41.AmazonAWS.Billing.BillingCsv.ReadFile(inputCsv);

			WriteHtml(statement, outputHtml);
		}

		static void SerializeToStream<T>(T obj, System.IO.Stream stream)
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
			serializer.Serialize(stream, obj);
		}

		static void WriteHtml(Eleven41.AmazonAWS.Billing.AwsStatement statement, string file)
		{
			string xsltFile = @"ActivityReport.xslt";

			// Serialize the statement to XML first
			System.IO.MemoryStream xmlStream = new System.IO.MemoryStream();
			SerializeToStream(statement, xmlStream);
			xmlStream.Seek(0, System.IO.SeekOrigin.Begin);

			// IF we already have a file, then delete it
			if (System.IO.File.Exists(file))
				System.IO.File.Delete(file);

			// Open the target file for writing
			using (var resultStream = System.IO.File.OpenWrite(file))
			{
				try
				{
					XPathDocument xPathDoc = new XPathDocument(xmlStream);

					XslTransform transform = new XslTransform();
					transform.Load(xsltFile);

					XmlTextWriter writer = new XmlTextWriter(resultStream, System.Text.Encoding.UTF8);
					transform.Transform(xPathDoc, null, writer);
				}
				finally
				{
					resultStream.Close();
				}
			}
		}
	}
}
