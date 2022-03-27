# EPiServer-Remote-Events-Tester


This is a project, application, for testing EPiServer remote events.

To run the application you may have to run it as an administrator (run as administrator). Otherwise you may get the following exception:

    System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.AggregateException: One or more errors occurred. ---> System.AggregateException: One or more errors occurred. ---> System.ServiceModel.CommunicationException: The service endpoint failed to listen on the URI 'net.tcp://127.0.0.1/RemoteEventService' because access was denied.  Verify that the current user is granted access in the appropriate allowAccounts section of SMSvcHost.exe.config.

Running it from Visual Studio requiere you to run Visual Studio as an administrator.

You can also change the settings in the following file:

- C:\Windows\Microsoft.NET\Framework64\v4.0.30319\SMSvcHost.exe.config

Then you have to restart the Net.TCP Port Sharing Service.

More information:
- [Configuring the Net.TCP Port Sharing Service](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/configuring-the-net-tcp-port-sharing-service/)
- [Well-known security identifiers in Windows operating systems](https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems/)
- [Modifying SMSvcHost.exe.config for WCF - Some common mistakes](https://blogs.msdn.microsoft.com/asiatech/2012/07/16/modifying-smsvchost-exe-config-for-wcf-some-common-mistakes/)

If you are logged in as an administrator with your work-domain-account, but offline from the domain, you may have to change it to this:

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
	    <runtime>
		    <gcConcurrent enabled="false" />
	    </runtime>
	    <system.serviceModel>
		    <diagnostics performanceCounters="Off" etwProviderId="{f18839f5-27ff-4e66-bd2d-639b768cf18b}" />
	    </system.serviceModel>
	    <system.serviceModel.activation>
		    <net.tcp listenBacklog="10" maxPendingConnections="100" maxPendingAccepts="2" receiveTimeout="00:00:10" teredoEnabled="false">
			    <allowAccounts>
				    <!-- Authenticated Users -->
				    <add securityIdentifier="S-1-5-11" />
			    </allowAccounts>
		    </net.tcp>
		    <diagnostics performanceCountersEnabled="true" />
	    </system.serviceModel.activation>
    </configuration>

## Build-configurations


- Publish-Debug (subscriber)
- Publish-Release (publisher)


App.config transforms are made on build by using [SlowCheetah](https://github.com/Microsoft/slow-cheetah/). So by changing build-configuration before running it from Visual Studio you can run it both in publish- or subscribe-mode.

## Configurations
App.publish.release etc contains configs for both SOAP:UDP and TCP tests.

## Notes

If you get SlowCheetah-errors when building the solution, like:

- \packages\Microsoft.VisualStudio.SlowCheetah.3.1.66\build\Microsoft.VisualStudio.SlowCheetah.targets(68,5): error MSB4096: The item "Build\Build.props" in item list "_NoneWithTargetPath" does not define a value for metadata "TransformOnBuild"....

You can try to change the build-configuration and build again. Then go back to the previous build-configuration and build again.

Credits to Hans Kindberg for making the origin of this.
