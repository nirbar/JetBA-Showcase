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

## JetBA++

The only native fully customizable, extensible, Qt-based UI for WiX bootstrappers.

## Building the sample

1. Restore Nuget packages
1. Close the solution and then reopen it in Visual Studio.
1. Rebuild project Bootstrapper
   The installer will be built in $(SolutionDir)build\bin\$(Configuration)\Bootstrapper\JetBA_Setup.exe