using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Reflection;
using System.Resources;

// Contains assembly attributes shared by all  EnterpriseDB.EDBClient projects

[assembly: CLSCompliant(false)]
[assembly: AllowPartiallyTrustedCallers()]
[assembly: SecurityRules(SecurityRuleSet.Level1)]
[assembly: AssemblyCompany("EnterpriseDB Development Team")]
[assembly: AssemblyProduct("EnterpriseDB.EDBClient")]
[assembly: AssemblyCopyright("Copyright © 2024 EnterpriseDB Development Team")]
[assembly: AssemblyTrademark("")]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]

// The following version attributes get rewritten by GitVersion as part of the build
[assembly: AssemblyVersion("9.0.3.1")]
[assembly: AssemblyFileVersion("9.0.3.1")]
[assembly: AssemblyInformationalVersion("9.0.3.1")]
