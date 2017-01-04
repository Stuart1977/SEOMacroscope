﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Threading;

namespace SEOMacroscope
{

	public partial class MacroscopeDocument : Macroscope
	{

		/**************************************************************************/

		/** BEGIN: Configuration **/
		public Boolean ProbeHrefLangs { get; set; }
		/** END: Configuration **/
		
		string Url;
		int Timeout;

		Boolean IsRedirect;
		string UrlRedirectFrom;

		HtmlDocument HtmlDoc;
		
		public MacroscopeDocument parent;

		public string Scheme;
		public string Hostname;
		public int Port;
		public string Path;
		public string Fragment;
		public string QueryString;

		public int StatusCode;
		public long ContentLength;
		public string MimeType;
		public Boolean IsHtml;
		public string ContentEncoding;
		public string Locale;
		public DateTime DateServer;
		public DateTime DateModified;

		public string Canonical;
		public Hashtable HrefLang;

		// Outbound links to pages and linked assets to follow
		public Hashtable Outlinks;

		// Inbound links from other pages in the scanned collection
		public Hashtable HyperlinksIn;

		// Outbound hypertext links
		public Hashtable HyperlinksOut;

		public Hashtable EmailAddresses;
		public Hashtable TelephoneNumbers;
		
		public string Title;
		public string Description;
		public string Keywords;

		public ArrayList Headings1;
		public ArrayList Headings2;

		public int Depth;
		
		/**************************************************************************/

		public MacroscopeDocument ( string sURL )
		{
			Url = sURL;
			Timeout = 10000;
			IsRedirect = false;
			StatusCode = 0;
			MimeType = null;
			IsHtml = false;
			Locale = "null";
			DateServer = new DateTime ();
			DateModified = new DateTime ();
			HrefLang = new Hashtable ();
			Outlinks = new Hashtable ();
			HyperlinksIn = new Hashtable ();
			HyperlinksOut = new Hashtable ();
			EmailAddresses = new Hashtable ();
			TelephoneNumbers = new Hashtable ();
			Headings1 = new ArrayList ( 16 );
			Headings2 = new ArrayList ( 16 );
			Depth = MacroscopeURLTools.FindUrlDepth( Url );
		}
		
		/**************************************************************************/

		public string GetUrl ()
		{
			return( this.Url );
		}

		/**************************************************************************/

		public string GetIsRedirect ()
		{
			return( this.IsRedirect.ToString() );
		}

		/**************************************************************************/
		
		public int GetStatusCode ()
		{
			return( this.StatusCode );
		}

		/**************************************************************************/
		
		public string GetMimeType ()
		{
			string sMimeType = null;
			if( this.MimeType == null ) {
				sMimeType = "";
			} else {
				MatchCollection matches = Regex.Matches( this.MimeType, "^([^\\s;/]+)/([^\\s;/]+)" );
				foreach( Match match in matches ) {
					sMimeType = String.Format( "{0}/{1}", match.Groups[ 1 ].Value, match.Groups[ 2 ].Value );
				}
				if( sMimeType == null ) {
					sMimeType = this.MimeType;
				}
			}
			return( sMimeType );
		}

		/**************************************************************************/
		
		public Boolean GetIsHtml ()
		{
			return( this.IsHtml );
		}

		/**************************************************************************/
		
		public string GetLang ()
		{
			return( this.Locale );
		}
		
		/**************************************************************************/
		
		public string GetLocale ()
		{
			return( this.Locale );
		}

		/**************************************************************************/
		
		public string GetCanonical ()
		{
			return( this.Canonical );
		}

		/**************************************************************************/
		
		public string GetDateServer ()
		{
			return( this.DateServer.ToShortDateString() );
		}
		
		/**************************************************************************/
		
		public string GetDateModified ()
		{
			return( this.DateModified.ToShortDateString() );
		}
				
		/**************************************************************************/

		public Hashtable GetOutlinks ()
		{
			return( this.Outlinks );
		}
		
		/**************************************************************************/
		
		public int CountOutlinks ()
		{
			int iCount = this.GetOutlinks().Count;
			return( iCount );
		}

		/**************************************************************************/
				
		public Hashtable AddHyperlinkIn ( string sURL )
		{
			if( this.HyperlinksIn.ContainsKey( sURL ) ) {
				int count = ( int )this.HyperlinksIn[ sURL ] + 1;
				this.HyperlinksIn[ sURL ] = count;
			} else {
				this.HyperlinksIn.Add( sURL, 1 );
			}
			return( this.HyperlinksIn );
		}

		/**************************************************************************/

		public Hashtable GetHyperlinksIn ()
		{
			return( this.HyperlinksIn );
		}

		/**************************************************************************/

		public Hashtable GetHyperlinksOut ()
		{
			return( this.HyperlinksOut );
		}
		
		/**************************************************************************/

		public int CountHyperlinksIn ()
		{
			int iCount = this.GetHyperlinksIn().Count;
			return( iCount );
		}
		
		/**************************************************************************/
		
		public int CountHyperlinksOut ()
		{
			int iCount = this.GetHyperlinksOut().Count;
			return( iCount );
		}

		/**************************************************************************/

		public Hashtable AddEmailAddress ( string sString )
		{
			debug_msg( string.Format( "AddEmailAddress: {0}", sString ) );
			if( this.EmailAddresses.ContainsKey( sString ) ) {
				this.EmailAddresses[ sString ] = this.GetUrl();
			} else {
				this.EmailAddresses.Add( sString, this.GetUrl() );
			}
			return( this.EmailAddresses );
		}

		/**************************************************************************/

		public Hashtable GetEmailAddresses ()
		{
			return( this.EmailAddresses );
		}

		/**************************************************************************/

		public Hashtable AddTelephoneNumber ( string sString )
		{
			debug_msg( string.Format( "AddTelephoneNumber: {0}", sString ) );
			if( this.TelephoneNumbers.ContainsKey( sString ) ) {
				this.TelephoneNumbers[ sString ] = this.GetUrl();
			} else {
				this.TelephoneNumbers.Add( sString, this.GetUrl() );
			}
			return( this.TelephoneNumbers );
		}

		/**************************************************************************/

		public Hashtable GetTelephoneNumbers ()
		{
			return( this.TelephoneNumbers );
		}

		/**************************************************************************/
		
		public string GetTitle ()
		{
			string sValue;
			if( this.Title != null ) {
				sValue = this.Title;
			} else {
				sValue = "";
			}
			return( sValue );
		}
		
		/**************************************************************************/
		
		public int GetTitleLength ()
		{
			return( this.GetTitle().Length );
		}

		/**************************************************************************/
		
		public string GetDescription ()
		{
			string sValue;
			if( this.Description != null ) {
				sValue = this.Description;
			} else {
				sValue = "";
			}
			return( sValue );
		}
		
		/**************************************************************************/
				
		public int GetDescriptionLength ()
		{
			return( this.GetDescription().Length );
		}
				
		/**************************************************************************/
				
		public string GetKeywords ()
		{
			string sValue;
			if( this.Keywords != null ) {
				sValue = this.Keywords;
			} else {
				sValue = "";
			}
			return( sValue );
		}

		/**************************************************************************/
				
		public int GetKeywordsLength ()
		{
			return( this.GetKeywords().Length );
		}
				
		/**************************************************************************/
				
		public int GetKeywordsCount ()
		{
			int uiCount = 0;
			string[] aKeywords = Regex.Split( this.GetKeywords(), "[\\s,]+" );
			uiCount = aKeywords.GetLength( 0 );
			return( uiCount );
		}
				
		/**************************************************************************/

		void SetHreflang ( string sLocale, string sURL )
		{
			MacroscopeHrefLang msHrefLang = new MacroscopeHrefLang ( false, sLocale, sURL );
			this.HrefLang[ sLocale ] = msHrefLang;
		}

		/**************************************************************************/

		public Hashtable GetHrefLangs ()
		{
			return( this.HrefLang );
		}

		/**************************************************************************/

		public void AddHeading1 ( string sString )
		{
			this.Headings1.Add( sString );
		}
		
		/**************************************************************************/
				
		public ArrayList GetHeadings1 ()
		{
			return( this.Headings1 );
		}

		/**************************************************************************/

		public void AddHeading2 ( string sString )
		{
			this.Headings2.Add( sString );
		}
		
		/**************************************************************************/
				
		public ArrayList GetHeadings2 ()
		{
			return( this.Headings2 );
		}

		/**************************************************************************/

		public Boolean Execute ()
		{

			if( this.IsRedirectPage() ) {
				debug_msg( string.Format( "IS REDIRECT: {0}", this.Url ), 2 );
				this.IsRedirect = true;
			} 

			if( this.IsHtmlPage() ) {
				debug_msg( string.Format( "IS HTML PAGE: {0}", this.Url ), 2 );
				this.ProcessHtmlPage();

			} else if( this.IsCssPage() ) {
				debug_msg( string.Format( "IS CSS PAGE: {0}", this.Url ), 2 );
				this.ProcessCssPage();

			} else if( this.IsImagePage() ) {
				debug_msg( string.Format( "IS IMAGE PAGE: {0}", this.Url ), 2 );
				this.ProcessImagePage();
				
			} else if( this.IsJavascriptPage() ) {
				debug_msg( string.Format( "IS JAVASCRIPT PAGE: {0}", this.Url ), 2 );
				this.process_javascript_page();

			} else if( this.IsPdfPage() ) {
				debug_msg( string.Format( "IS PDF PAGE: {0}", this.Url ), 2 );
				this.ProcessPdfPage();

			} else if( this.IsBinaryPage() ) {
				debug_msg( string.Format( "IS BINARY PAGE: {0}", this.Url ), 2 );
				this.ProcessBinaryPage();

			} else {
				debug_msg( string.Format( "UNKNOWN PAGE TYPE: {0}", this.Url ), 2 );
			}
			
			return( true );

		}

		/**************************************************************************/

		Boolean IsRedirectPage ()
		{
			HttpWebRequest req = null;
			HttpWebResponse res = null;
			Boolean bIsRedirect = false;
			string sOriginalURL = this.Url;

			try {

				req = WebRequest.CreateHttp( this.Url );
				req.Method = "HEAD";
				req.Timeout = this.Timeout;
				req.KeepAlive = false;
				req.AllowAutoRedirect = false;
				res = ( HttpWebResponse )req.GetResponse();

				debug_msg( string.Format( "Status: {0}", res.StatusCode ), 2 );

				if( res.StatusCode == HttpStatusCode.Moved ) {
					bIsRedirect = true;
				} else if( res.StatusCode == HttpStatusCode.MovedPermanently ) {
					bIsRedirect = true;
				}
			
				if( bIsRedirect ) {
					this.IsRedirect = true;
					this.Url = res.GetResponseHeader( "Location" );
					this.UrlRedirectFrom = sOriginalURL;



					//this.url = MacroscopeURLTools.make_url_absolute( this.url, this.UrlRedirectFrom );









				}
				res.Close();

			} catch( WebException ex ) {
				debug_msg( string.Format( "is_redirect :: WebException: {0}", ex.Message ), 2 );
			}

			return( bIsRedirect );
		}

		/**************************************************************************/

		int ProcessStatusCode ( HttpStatusCode status )
		{
			int iStatus = 0;
			switch( status ) {
				case HttpStatusCode.OK:
					iStatus = 200;
					break;
				case HttpStatusCode.MovedPermanently:
					iStatus = 301;
					break;
				case HttpStatusCode.NotFound:
					iStatus = 404;
					break;
				case HttpStatusCode.Gone:
					iStatus = 410;
					break;
				case HttpStatusCode.InternalServerError:
					iStatus = 500;
					break;
			}
			return( iStatus );
		}

		/**************************************************************************/

		public void debug_msg ( String sMsg )
		{
		}

		public void debug_msg ( String sMsg, int iOffset )
		{
		}

		/**************************************************************************/

	}

}
