using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace DataServicesJSONP
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class JSONPSupportBehaviorAttribute : Attribute, IServiceBehavior
	{
		void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			if (serviceHostBase == null)
				throw new ArgumentNullException("serviceHostBase");

			using (IEnumerator<ChannelDispatcherBase> enumerator = serviceHostBase.ChannelDispatchers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ChannelDispatcher channelDispatcher = (ChannelDispatcher)enumerator.Current;
					foreach (EndpointDispatcher current in channelDispatcher.Endpoints)
					{
						current.DispatchRuntime.MessageInspectors.Add(new JSONPSupportInspector());
					}
				}
			}
		}

		void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
