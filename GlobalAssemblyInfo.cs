using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("OpenDNS DNSCrypt Windows Client")]

[assembly: AssemblyCompany("OpenDNS")]
[assembly: AssemblyCopyright("Copyright © OpenDNS 2012")]
[assembly: AssemblyTrademark("")]

#if DEBUG
    [assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

// AssemblyVersion: The Version number that the CLR cares about.
// Update this manually, but only when there is a "breaking change" to the assembly.
// This can be specified on a per-assembly basis if desired.
[assembly: AssemblyVersion("1.0")]

// AssemblyFileVersion: Uniquely identifies the build.
// Must be of the format "Major.Minor.Build.Revision".
// Update Major and Minor manually, allow the build system to update Build and Revision.
// The maximum number is 65535, so don't use SVN build numbers here!
// This number appears in the Windows Property Page.
// This can be specified on a per-assembly basis if desired.
[assembly: AssemblyFileVersion("0.0.4")]

// AssemblyInformationalVersion: Identifies the product version.
// Can contain extra strings (ie. "1.0.0 RC1" or "1.0 (build 12345)").
// This is the public/marketing version, and should common across all assemblies in the product.
// This number appears in the Windows Property Page.
[assembly: AssemblyInformationalVersion("0.0.4")]

