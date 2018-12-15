using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ApplicationModules.Security;
using EPiServer.Framework.Security;

namespace Application.Business
{
	public class FakedSiteSecretManager : ISiteSecretManager
	{
		#region Methods

		protected internal virtual ISiteSecret CreateSiteSecret(string siteId)
		{
			return new SiteSecret
			{
				Id = new Guid("e44c3c36-2492-488d-8c87-07f469b280b3"),
				Secret = new byte[] {1, 2, 3, 4},
				SiteId = siteId
			};
		}

		public virtual ISiteSecret GetSecret(string siteId)
		{
			var secret = this.CreateSiteSecret(siteId);

			return secret;
		}

		public virtual IEnumerable<ISiteSecret> List()
		{
			return Enumerable.Empty<ISiteSecret>();
		}

		#endregion
	}
}