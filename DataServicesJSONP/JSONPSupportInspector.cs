using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace DataServicesJSONP
{
	internal class JSONPSupportInspector : IDispatchMessageInspector
	{
		private class Writer : BodyWriter
		{
			private string content;

			public Writer(string content) : base(false)
			{
				this.content = content;
			}

			protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
			{
				if (writer == null)
					throw new ArgumentNullException("writer");

				writer.WriteStartElement("Binary");
				byte[] bytes = JSONPSupportInspector.encoding.GetBytes(this.content);
				writer.WriteBase64(bytes, 0, bytes.Length);
				writer.WriteEndElement();
			}
		}

		private static Encoding encoding = Encoding.UTF8;
		private Message message = null;

		public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			object result;
			if (request == null)
				throw new ArgumentNullException("request");

			if (request.Properties.ContainsKey("UriTemplateMatchResults"))
			{
				HttpRequestMessageProperty httpRequestMessageProperty = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
				UriTemplateMatch uriTemplateMatch = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];
				string value = uriTemplateMatch.QueryParameters["$format"];
				if ("json".Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					uriTemplateMatch.QueryParameters.Remove("$format");
					httpRequestMessageProperty.Headers["Accept"] = "application/json";
					string text = uriTemplateMatch.QueryParameters["$callback"];
					if (!string.IsNullOrEmpty(text))
					{
						uriTemplateMatch.QueryParameters.Remove("$callback");
						result = text;
						return result;
					}
				}
			}
			result = null;
			return result;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
			if (reply == null)
				throw new ArgumentNullException("reply");
			
			String str = correlationState as String;

			if (str != null)
			{
				XmlDictionaryReader readerAtBodyContents = reply.GetReaderAtBodyContents();
				readerAtBodyContents.ReadStartElement();
				string text = JSONPSupportInspector.encoding.GetString(readerAtBodyContents.ReadContentAsBase64());
				text = str + "(" + text + ")";
				this.message = Message.CreateMessage(MessageVersion.None, "", new JSONPSupportInspector.Writer(text));
				this.message.Properties.CopyProperties(reply.Properties);
				reply = this.message;
			}
		}
	}
}
