JetWixExtension features the most comprehensive WiX preprocessor and BootstrapperApplication frameworks supporting either native Qt-based or managed WPF-based user interface for WiX bundles
Contact the owner to obtain a license for JetWixExtension

## JetBA

The most comprehensive, fully customizable, extensible, WPF-based BootstrapperApplication.

## JetBA++

The only native fully customizable, extensible, Qt-based BootstrapperApplication.

## Preprocessor

- Harvest directly from WiX source code by executing Heat commands
  ~~~~~~~
  <?pragma heat.dir "$(sys.SOURCEFILEDIR)..\bin\Release" -cg BIN -dr INSTALLFOLDER -ag?>
  ~~~~~~~
- Collection variables
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


