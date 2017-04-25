using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Reflection;
using System.Resources;

// Contains assembly attributes shared by all  EnterpriseDB.EDBClient projects

[assembly: CLSCompliant(false)]
[assembly: AllowPartiallyTrustedCallers()]
[assembly: SecurityRules(SecurityRuleSet.Level1)]
[assembly: AssemblyCompany(" EnterpriseDB.EDBClient Development Team")]
[assembly: AssemblyProduct(" EnterpriseDB.EDBClient")]
[assembly: AssemblyCopyright("Copyright © 2002 - 2017  EnterpriseDB.EDBClient Development Team")]
[assembly: AssemblyTrademark("")]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]

// The following version attributes get rewritten by GitVersion as part of the build
[assembly: AssemblyVersion("3.1.9.3")]
[assembly: AssemblyFileVersion("3.1.9.3")]
[assembly: AssemblyInformationalVersion("3.1.9.3")]
