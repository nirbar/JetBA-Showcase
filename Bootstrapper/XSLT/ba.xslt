<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                xmlns:wix="http://wixtoolset.org/schemas/v4/wxs"
                xmlns:bal="http://wixtoolset.org/schemas/v4/wxs/bal"
                >
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Payload" >
    <xsl:copy>
      <xsl:attribute name="Name">
        <xsl:value-of select="substring-after( @SourceFile, '\')"/>
      </xsl:attribute>
      <xsl:attribute name="Compressed">yes</xsl:attribute>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Payload[contains(@SourceFile, 'SampleJetBA.dll')]" >
    <xsl:copy>
      <xsl:attribute name="bal:BAFactoryAssembly">yes</xsl:attribute>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
