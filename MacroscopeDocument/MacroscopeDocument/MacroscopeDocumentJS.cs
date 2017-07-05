﻿/*

	This file is part of SEOMacroscope.

	Copyright 2017 Jason Holland.

	The GitHub repository may be found at:

		https://github.com/nazuke/SEOMacroscope

	Foobar is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Foobar is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SEOMacroscope
{

  public partial class MacroscopeDocument : Macroscope
  {

    /**************************************************************************/

    private void ProcessJavascriptPage ()
    {

      HttpWebRequest req = null;
      HttpWebResponse res = null;
      string ResponseErrorCondition = null;
      Boolean IsAuthenticating = false;
      
      try
      {

        req = WebRequest.CreateHttp( this.DocUrl );
        req.Method = "GET";
        req.Timeout = this.Timeout;
        req.KeepAlive = false;
        
        this.PrepareRequestHttpHeaders( req: req );
                
        IsAuthenticating = this.AuthenticateRequest( req );
                                      
        MacroscopePreferencesManager.EnableHttpProxy( req );
                
        res = ( HttpWebResponse )req.GetResponse();

      }
      catch( UriFormatException ex )
      {
        DebugMsg( string.Format( "ProcessJavascriptPage :: UriFormatException: {0}", ex.Message ) );
        ResponseErrorCondition = ex.Message;
      }
      catch( TimeoutException ex )
      {
        DebugMsg( string.Format( "ProcessJavascriptPage :: TimeoutException: {0}", ex.Message ) );
        ResponseErrorCondition = ex.Message;
      }
      catch( WebException ex )
      {

        DebugMsg( string.Format( "ProcessJavascriptPage :: WebException: {0}", ex.Message ) );
        DebugMsg( string.Format( "ProcessJavascriptPage :: WebException: {0}", ex.Status ) );
        DebugMsg( string.Format( "ProcessJavascriptPage :: WebException: {0}", ( int )ex.Status ) );

        ResponseErrorCondition = ex.Status.ToString();

      }

      if( res != null )
      {

        string RawData = "";

        this.ProcessResponseHttpHeaders( req, res );

        /** ---------------------------------------------------------------- **/

        if( IsAuthenticating )
        {
          this.VerifyOrPurgeCredential();
        }

        /** Get Response Body ---------------------------------------------- **/
        
        try
        {

          DebugMsg( string.Format( "MIME TYPE: {0}", this.MimeType ) );

          Encoding encUseEncoding = Encoding.UTF8;

          if( this.CharSet != null )
          {
            encUseEncoding = this.CharSet;
          }
          else
          {
            encUseEncoding = this.JavascriptSniffCharset();
          }

          Stream ResponseStream = res.GetResponseStream();
          StreamReader ResponseStreamReader = new StreamReader ( ResponseStream, encUseEncoding );
          RawData = ResponseStreamReader.ReadToEnd();
          this.ContentLength = RawData.Length; // May need to find bytes length
          this.SetChecksum( RawData );

        }
        catch( WebException ex )
        {

          DebugMsg( string.Format( "WebException: {0}", ex.Message ) );
          
          if( ex.Response != null )
          {
            this.SetStatusCode( ( ( HttpWebResponse )ex.Response ).StatusCode );
          }
          else
          {
            this.SetStatusCode( ( HttpStatusCode )ex.Status );
          }
          
          RawData = "";
          this.ContentLength = 0;

        }
        catch( Exception ex )
        {

          DebugMsg( string.Format( "Exception: {0}", ex.Message ) );
          this.SetStatusCode( HttpStatusCode.BadRequest );
          RawData = "";
          this.ContentLength = 0;

        }

        /** ---------------------------------------------------------------- **/

        if( !string.IsNullOrEmpty( RawData ) )
        {

          if(
            MacroscopePreferencesManager.GetCustomFiltersEnable()
            && MacroscopePreferencesManager.GetCustomFiltersApplyToJavascripts() )
          {
          
            MacroscopeCustomFilters CustomFilter = this.DocCollection.GetJobMaster().GetCustomFilter();

            if( ( CustomFilter != null ) && ( CustomFilter.IsEnabled() ) )
            {
              this.ProcessJavascriptCustomFiltered( CustomFilter: CustomFilter, JavascriptText: RawData );           
            }

          }
          
        }

        /** ---------------------------------------------------------------- **/

        if( !string.IsNullOrEmpty( RawData ) )
        {

          if(
            MacroscopePreferencesManager.GetDataExtractorsEnable()
            && MacroscopePreferencesManager.GetDataExtractorsApplyToJavascripts() )
          {

            this.ProcessJavascriptDataExtractors( JavascriptText: RawData );

          }

        }

        /** ---------------------------------------------------------------- **/

        { // Title
          MatchCollection reMatches = Regex.Matches( this.DocUrl, "/([^/]+)$" );
          string DocumentTitle = null;
          foreach( Match match in reMatches )
          {
            if( match.Groups[ 1 ].Value.Length > 0 )
            {
              DocumentTitle = match.Groups[ 1 ].Value.ToString();
              break;
            }
          }
          if( DocumentTitle != null )
          {
            this.SetTitle( DocumentTitle, MacroscopeConstants.TextProcessingMode.NO_PROCESSING );
            DebugMsg( string.Format( "TITLE: {0}", this.GetTitle() ) );
          }
          else
          {
            DebugMsg( string.Format( "TITLE: {0}", "MISSING" ) );
          }
        }

        /** ---------------------------------------------------------------- **/

        res.Close();
        
        res.Dispose();

      }

      if( ResponseErrorCondition != null )
      {
        this.ProcessErrorCondition( ResponseErrorCondition );
      }

    }

    /** Process Custom Filtered *********************************************/

    private void ProcessJavascriptCustomFiltered (
      MacroscopeCustomFilters CustomFilter,
      string JavascriptText
    )
    {

      Dictionary<string, MacroscopeConstants.TextPresence> Analyzed;

      Analyzed = CustomFilter.AnalyzeText( Text: JavascriptText );

      foreach( string Key in Analyzed.Keys )
      {
        this.SetCustomFiltered( Text: Key, Presence: Analyzed[ Key ] );
      }

      return;
      
    }

    /** Process Data Extractors ***********************************************/

    private void ProcessJavascriptDataExtractors ( string JavascriptText )
    {

      MacroscopeJobMaster JobMaster = this.DocCollection.GetJobMaster();

      {

        MacroscopeDataExtractorRegexes DataExtractor = JobMaster.GetDataExtractorRegexes();

        if( ( DataExtractor != null ) && ( DataExtractor.IsEnabled() ) )
        {
          this.ProcessJavascriptDataExtractorRegexes(
            DataExtractor: DataExtractor,
            JavascriptText: JavascriptText
          );
        }

      }

      return;
      
    }

    /** -------------------------------------------------------------------- **/

    private void ProcessJavascriptDataExtractorRegexes (
      MacroscopeDataExtractorRegexes DataExtractor,
      string JavascriptText
    )
    {

      List<KeyValuePair<string, string>> Analyzed;

      Analyzed = DataExtractor.AnalyzeText( Text: JavascriptText );

      foreach( KeyValuePair<string, string> Pair in Analyzed )
      {
        this.SetDataExtractedRegexes( 
          Label: Pair.Key, 
          Text: Pair.Value 
        );
      }

      return;

    }

    /** Sniff Charset *********************************************************/

    Encoding JavascriptSniffCharset ()
    {

      Encoding encSniffed = Encoding.UTF8;

      // TODO: Implement code to download JS and detect charset

      return( encSniffed );

    }

    /**************************************************************************/

  }

}
