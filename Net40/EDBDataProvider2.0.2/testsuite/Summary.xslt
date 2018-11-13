<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html" doctype-public="-//W3C//DTD html 4.01 Transitional//EN" encoding="utf-8"/>



<xsl:template match="/">

	<html>

		<head>

			<title>NUnit Report - Unit Test Results</title>

			<style>

				body

				{

				font-family: verdana, arial, helvetica, Sans-Serif;

				font-size: x-small;

				}

				a

				{

				text-decoration: none;

				background-color: Transparent

				}

				a:link

				{

				color: #0033ff;

				}

				a:visited

				{

				color:	#003399;

				}

				a:active, a:hover

				{

				color:#69c;

				}

				h2

				{

				padding: 4px 4px 4px 6px;

				border: 1px solid #999;

				color: #900;

				background-color: #ddd;

				font-weight:900;

				font-size: Medium;

				}

				h3

				{

				padding: 4px 4px 4px 6px;

				border: 1px solid #aaa;

				color: #900;

				background-color: #eee;

				font-weight: normal;

				font-size: medium;

				}

				table

				{

				padding:0px;

				width: 100%;

				margin-left: -2px;

				margin-right: -2px;

				}

				th, td

				{

				padding: 2px 4px 2px 4px;

				vertical-align: top;

				font-size: x-small;

				}

				address

				{

				font-family: verdana, arial, helvetica, Sans-Serif;

				font-size: 8pt;

				font-style: normal;

				text-align: right;

				}



				.title

				{

				background-color: #bbb;

				color: white;

				}

				success

				{

				background-color: #eee;

				color: black;

				}

				failure

				{

				background-color: #eee;

				color: red;

				}

				.notrun

				{

				background-color: yellow;

				color: black;

				}

				.errorreport

				{

				background-color: #f1f1f1;

				color: black;

				font-size: 9pt;

				}

				.right

				{

				font-size: 8pt;

				text-align: right;

				}

			</style>

		</head>		

		<body>			

			<p>Following is the summary of NUnit test results on Windows XP. For more detail please see the attached file.</p>			

			<xsl:apply-templates/>			

		</body>

	</html>

</xsl:template>



<xsl:template match="test-results">

	<h2>Summary</h2>

	<table border="0" rules="none" width="100%">

		<tr align="left" class="title" border="1">

			<th width="52%" align="left" colspan="2">Name</th>

			<th width="7%" align="left">Total</th>

			<th width="7%" align="left">Failures</th>

			<th width="7%" align="left">Not-Run</th>

			<th width="11%" align="left">Success Rate</th>

			<th width="9%" align="left">Date</th>

			<th width="7%" align="left">Time</th>

		</tr>

		<xsl:choose>

			<xsl:when test="@failures&gt;0">

				<xsl:call-template name="summary_detail">

					<xsl:with-param name="className">failure</xsl:with-param>

				</xsl:call-template>

			</xsl:when>

			<xsl:otherwise>

				<xsl:call-template name="summary_detail">

					<xsl:with-param name="className">success</xsl:with-param>

				</xsl:call-template>

			</xsl:otherwise>

		</xsl:choose>

	</table>	

    </xsl:template>



<xsl:template name="summary_detail">

	<xsl:param name="className">success</xsl:param> 

	<xsl:variable name="name" select="@name" />

	<xsl:variable name="totalCount" select="@total" />

	<xsl:variable name="failureCount" select="@failures" />

	<xsl:variable name="notRunCount" select="@not-run" />

	<xsl:variable name="successRate" select="($totalCount - $failureCount) div $totalCount" />

	<xsl:variable name="date" select="@date" />

	<xsl:variable name="time" select="@time" />

	<tr>

		<td></td>

		<td><xsl:value-of select="$name"/></td>

		<td><xsl:value-of select="format-number($totalCount, '0')"/></td>

		<td><xsl:value-of select="format-number($failureCount, '0')"/></td>

		<xsl:choose>

			<xsl:when test="$notRunCount&gt;0">

				<td class="notrun"><xsl:value-of select="format-number($notRunCount, '0')"/></td>

			</xsl:when>

			<xsl:otherwise>

				<td><xsl:value-of select="format-number($notRunCount, '0')"/></td>

			</xsl:otherwise>

		</xsl:choose>

		<td><xsl:value-of select="format-number($successRate, '0.00%')"/></td>

		<td><xsl:value-of select="$date"/></td>

		<td><xsl:value-of select="$time"/></td>

	</tr>

</xsl:template>

</xsl:stylesheet>