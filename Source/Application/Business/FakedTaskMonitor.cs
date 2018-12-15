using System;
using System.Runtime.InteropServices;
using EPiServer.Async;

namespace Application.Business
{
	[ComVisible(false)]
	public class FakedTaskMonitor : TaskMonitor
	{
		#region Methods

		public override TaskInformation GetStatus(TrackingToken trackingToken)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}