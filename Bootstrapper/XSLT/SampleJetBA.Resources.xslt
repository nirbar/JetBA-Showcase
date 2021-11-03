<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
	<xsl:output method="xml" indent="yes"/>

	<xsl:key name="KeepFiles" match="wix:Component[not(
			 contains(./wix:File/@Source, '\SampleJetBA.resources.dll')
			 or contains(./wix:File/@Source, '\PanelSW.Installer.JetBA.resources.dll')
			 or contains(./wix:File/@Source, '\PanelSW.Installer.JetBA.JetPack.resources.dll')
			 )]" use="@Id"/>

	<xsl:template match="@* | node()">
		<xsl:copy>
			<xsl:apply-templates select="@* | node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="//wix:Component[key('KeepFiles', @Id)]"/>
	<xsl:template match="//wix:ComponentRef[key('KeepFiles', @Id)]"/>
</xsl:stylesheet>