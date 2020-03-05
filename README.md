JetWixExtension features the most comprehensive WiX preprocessor and BootstrapperApplication frameworks supporting either native Qt-based or managed WPF-based user interfaces for WiX bundles.

Contact the owner to obtain a license for JetWixExtension

## JetBA

The most comprehensive, fully customizable, extensible, WPF-based UI for WiX bootstrappers.

SampleJetBA project presents usage of JetBA. It features a WPF user interface:
  - Target folder selection
  - SQL Server connection customization
  - Summary page to review selections
  - Input fields validations with pop up messages to notify errors
  - Usage of JetBA View-models to bind burn variables, installation states, wizard navigation etc.

## Preprocessor Extension

- Harvest directly from WiX source code by executing Heat commands
  ~~~~~~~
  <?pragma heat.dir "$(sys.SOURCEFILEDIR)..\bin\Release" -cg BIN -dr INSTALLFOLDER -ag?>
  ~~~~~~~
- Collection variables.
  The following example shows how to use collection variables to deploy a file six times with different target names- permutations of (a, b, c) and (x, y, z):
  ~~~~~~~
  <?pragma tuple.NIR a; b; c?>
  <?pragma tuple.BAR x; y; z?>
  <?foreach i in $(tuple_range.BAR())?>
  <Component Feature="ProductFeature" Directory="INSTALLFOLDER">
      <File Source="$(sys.SOURCEFILEPATH)" Name="Product.$(tuple.NIR($(var.i))).$(tuple.BAR($(var.i))).wxs"/>
  </Component>
  <?endforeach?>
  <?pragma endtuple.BAR?>
  <?pragma endtuple.NIR?>
  ~~~~~~~
- Generate random Id. 
  Useful when deploying files with same name to different target folders:
  ~~~~~~~
	<ComponentGroup Id="random">
		<Component Directory="Product.Dir">
		<File Source="$(sys.SOURCEFILEPATH)" Id="$(jet.random_id())"/>
		</Component>
		<Component Directory="INSTALL_FOLDER">
		<File Source="$(sys.SOURCEFILEPATH)" Id="$(jet.random_id())"/>
		</Component>
	</ComponentGroup>
  ~~~~~~~
- Generate random or pseudo-random Guid. 
  ~~~~~~~
  	<ComponentGroup Id="random">
      <?foreach account in localService;localSystem;networkService;applicationPoolIdentity?>
      <Component Id="webapp.$(var.account)" Guid="$(jet.Guid($(var.account)))" Transitive="yes">
        <Condition><![CDATA[IIS_ACCOUNT ~= "$(var.account)"]]></Condition>
        <CreateFolder />
        <iis:WebAppPool Id="$(var.account)" Name="WebUI" Identity="$(var.account)" ManagedRuntimeVersion="v4.0" ManagedPipelineMode="Integrated" />
        <iis:WebVirtualDir Id="$(var.account)" Directory="INSTALLFOLDER" Alias="WebUI" WebSite="WebUI">
          <iis:WebApplication Id="$(var.account)" Name="WebUI" WebAppPool="$(var.account)" />
        </iis:WebVirtualDir>
      </Component>
      <?endforeach?>
  	</ComponentGroup>
  ~~~~~~~
- Detect bundles and get their versions in a variable:
  ~~~~~~~
    <jet:BundleSearch UpgradeCode="{1D1DB5E6-E0D8-3103-8570-369A82A9BF33}" VersionVariable="DetectedVC2013x86Version" NamePattern="\bx86\b"/>
  ~~~~~~~
- RebootBoundary attrbiute on bundle packages: force reboot after the package if any preceding package required reboot

## JetBA++

The only native fully customizable, extensible, Qt-based UI for WiX bootstrappers.

## Building the sample

1. Clone the repo including git submodules
1. Restore Nuget packages
1. Close the solution and then reopen it in Visual Studio.
1. Rebuild project Bootstrapper
   The installer will be built in $(SolutionDir)build\bin\\$(Configuration)\Bootstrapper\JetBA_Showcase.exe

Note that Debug builds attempt to attach a debugger while Release builds do not.