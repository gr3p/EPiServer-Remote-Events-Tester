using System;
using System.Reflection;
using EPiServer.Async;
using EPiServer.Events;
using EPiServer.Events.Internal;
using EPiServer.Events.Providers;
using EPiServer.Framework;
using EPiServer.Framework.Configuration;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Security;
using EPiServer.Logging;
using EPiServer.Logging.Log4Net;
using EPiServer.Security;
using EPiServer.Security.Internal;
using EPiServer.ServiceLocation;

namespace Application.Business
{
	[InitializableModule]
	[ModuleDependency(typeof(ServiceContainerInitialization))]
	public class Initialization : IConfigurableModule
	{
		#region Fields

		private static Action<Type, IServiceConfigurationProvider> _serviceConfigurationScannerProcessAction;

		#endregion

		#region Properties

		protected internal virtual EventsInitialization EventsInitialization { get; } = new EventsInitialization();

		protected internal virtual Action<Type, IServiceConfigurationProvider> ServiceConfigurationScannerProcessAction
		{
			get
			{
				// ReSharper disable All
				if(_serviceConfigurationScannerProcessAction == null)
				{
					var serviceConfigurationScannerType = typeof(ServiceContainerInitialization).GetNestedType("ServiceConfigurationScanner", BindingFlags.NonPublic | BindingFlags.Static);

					var method = serviceConfigurationScannerType.GetMethod("Process");

					_serviceConfigurationScannerProcessAction = (Action<Type, IServiceConfigurationProvider>) Delegate.CreateDelegate(typeof(Action<Type, IServiceConfigurationProvider>), method);
				}
				// ReSharper restore All

				return _serviceConfigurationScannerProcessAction;
			}
		}

		#endregion

		#region Methods

		public void ConfigureContainer(ServiceConfigurationContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			foreach(var type in typeof(EventsInitialization).Assembly.GetTypes())
			{
				this.ServiceConfigurationScannerProcessAction.Invoke(type, context.Services);
			}

			context.Services.AddSingleton(new EventOptions
			{
				DefaultProviderFactory = serviceLocator =>
				{
					var frameworkConfiguration = serviceLocator.GetInstance<EPiServerFrameworkSection>();

					var eventProviderSettings = frameworkConfiguration.Event.Providers[frameworkConfiguration.Event.DefaultProvider];

					var eventProvider = (EventProvider) Activator.CreateInstance(Type.GetType(eventProviderSettings.Type, true));
					eventProvider.Initialize(eventProviderSettings.Name, eventProviderSettings.Parameters);

					return eventProvider;
				}
			});

			context.Services.AddSingleton(EPiServerFrameworkSection.Instance);
			context.Services.AddSingleton<ILoggerFactory, Log4NetLoggerFactory>();
			context.Services.AddSingleton<ISiteSecretManager, FakedSiteSecretManager>();

			// ReSharper disable All
			var noVirtualRoleReplicationType = Type.GetType(typeof(VirtualRoleAuthorizationSession).AssemblyQualifiedName.Replace("VirtualRoleAuthorizationSession", "NoVirtualRoleReplication"));
			context.Services.AddSingleton((IVirtualRoleReplication) Activator.CreateInstance(noVirtualRoleReplicationType));
			// ReSharper restore All

			context.Services.AddSingleton<TaskMonitor, FakedTaskMonitor>();

			this.EventsInitialization.ConfigureContainer(context);
		}

		public virtual void Initialize(InitializationEngine context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			LogManager.LoggerFactory = () => context.Locate.Advanced.GetInstance<ILoggerFactory>();

			this.EventsInitialization.Initialize(context);
		}

		public virtual void Uninitialize(InitializationEngine context)
		{
			this.EventsInitialization.Uninitialize(context);
		}

		#endregion
	}
}