<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:wix="http://wixtoolset.org/schemas/v4/wxs">
	<xsl:output method="xml" indent="yes"/>

	<xsl:template match="/">
		<wix:Wix>
			<wix:Fragment>
				<wix:PayloadGroup>
					<xsl:attribute name="Id">
						<xsl:value-of select="//wix:ComponentGroup/@Id"/>
					</xsl:attribute>
					<xsl:for-each select="//wix:File">
						<wix:Payload>
							<xsl:attribute name="Id">
								<xsl:value-of select="./@Id"/>
							</xsl:attribute>
							<xsl:attribute name="SourceFile">
								<xsl:value-of select="./@Source"/>
							</xsl:attribute>
							<xsl:attribute name="Name">
								<xsl:value-of select="substring-after(./@Source, '\')"/>
							</xsl:attribute>
						</wix:Payload>
					</xsl:for-each>
				</wix:PayloadGroup>
			</wix:Fragment>
		</wix:Wix>
	</xsl:template>

</xsl:stylesheet>
