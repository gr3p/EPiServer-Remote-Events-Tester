using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using EPiServer.Events.Providers;
using EPiServer.Events.Providers.Internal;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Application
{
	internal class Program
	{
		#region Methods

		private static bool IsPublishMode(string[] arguments)
		{
			var publish = arguments != null && arguments.Contains("publish", StringComparer.OrdinalIgnoreCase);

			// ReSharper disable InvertIf
			if(!publish)
			{
				if(bool.TryParse(ConfigurationManager.AppSettings["Publish"], out var parsedPublish))
					publish = parsedPublish;
			}
			// ReSharper restore InvertIf

			return publish;
		}

		protected internal static void Main(string[] arguments)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

			try
			{
				InitializationModule.FrameworkInitialization(HostType.WebApplication);

				var eventBroker = ServiceLocator.Current.GetInstance<IEventBroker>();

				if(IsPublishMode(arguments))
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Publishing (press \"Enter\", without typing a message, to exit)");
					Console.WriteLine("*************************************************************");
					Console.ResetColor();

					string parameter;

					while(!string.IsNullOrEmpty(parameter = Console.ReadLine()))
					{
						try
						{
							eventBroker.RaiseEventAsync(Guid.NewGuid(), parameter);
							WriteConfirmation(string.Format(CultureInfo.InvariantCulture, "Event with parameter \"{0}\" was raised. Check the log file if you have problems.", parameter));
						}
						catch(Exception exception)
						{
							WriteException(exception);
						}
					}
				}
				else
				{
					eventBroker.EventMissed += (sender, e) => { WriteEventMissed(e); };
					eventBroker.EventReceived += (sender, e) => { WriteEventReceived(e); };

					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Subscribing (press any key to exit)");
					Console.WriteLine("***********************************");
					Console.ResetColor();

					Console.ReadKey();
				}
			}
			catch(Exception exception)
			{
				WriteException(exception);
				PressAnyKeyToExit();
			}
		}

		private static void PressAnyKeyToExit()
		{
			Console.WriteLine();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		private static void WriteConfirmation(string message)
		{
			WriteValue(ConsoleColor.Green, message);
		}

		private static void WriteEventMessage(string applicationName, Guid? eventId, object parameter, Guid? raiserId, DateTime? sent, int? sequenceNumber, string serverName, string siteId, IEnumerable<byte> verificationData)
		{
			Console.WriteLine();
			Console.WriteLine("****************** Event Start ********************");

			if(!string.IsNullOrEmpty(applicationName))
				Console.WriteLine(" Application-name : " + applicationName);

			if(eventId != null)
				Console.WriteLine(" Event-id         : " + eventId);

			if(parameter != null)
				Console.WriteLine(" Parameter        : " + parameter);

			if(raiserId != null)
				Console.WriteLine(" Raiser-id        : " + raiserId);

			if(sent != null)
				Console.WriteLine(" Sent             : " + sent);

			if(sequenceNumber != null)
				Console.WriteLine(" Sequence-number  : " + sequenceNumber);

			if(!string.IsNullOrEmpty(serverName))
				Console.WriteLine(" Server-name      : " + serverName);

			if(!string.IsNullOrEmpty(siteId))
				Console.WriteLine(" Site-id          : " + siteId);

			if(verificationData != null)
				Console.WriteLine(" Verification-data: " + string.Join(", ", verificationData));

			Console.WriteLine("****************** Event End *********************");
		}

		private static void WriteEventMissed(EventMissedEventArgs e)
		{
			if(e == null)
				throw new ArgumentNullException(nameof(e));

			Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine();
			Console.WriteLine("****************** Event Missed Start ********************");
			Console.WriteLine(" Event-id         : " + e.EventId);
			Console.WriteLine("****************** Event Missed End *********************");

			Console.ResetColor();
		}

		private static void WriteEventReceived(EventReceivedEventArgs e)
		{
			if(e == null)
				throw new ArgumentNullException(nameof(e));

			WriteEventMessage(e.ApplicationName, e.EventId, e.Param, e.RaiserId, e.Sent, null, e.ServerName, null, null);
		}

		private static void WriteException(Exception exception)
		{
			Console.WriteLine();
			WriteValue(ConsoleColor.Red, exception);
		}

		private static void WriteValue(ConsoleColor color, object value)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(value);
			Console.ResetColor();
		}

		#endregion
	}
}