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
using System.Drawing;
using System.Windows.Forms;

namespace SEOMacroscope
{

  public sealed class MacroscopeDisplayHistory : Macroscope
  {

    /**************************************************************************/

    private MacroscopeMainForm MainForm;

    private ListView lvListView;

    private Boolean ListViewConfigured = false;
    
    /**************************************************************************/

    public MacroscopeDisplayHistory ( MacroscopeMainForm MainForm, ListView lvListView )
    {

      this.MainForm = MainForm;
      this.lvListView = lvListView;

      if( this.MainForm.InvokeRequired )
      {
        this.MainForm.Invoke(
          new MethodInvoker (
            delegate
            {
              this.ConfigureListView();
            }
          )
        );
      }
      else
      {
        this.ConfigureListView();
      }

    }

    /**************************************************************************/

    private void ConfigureListView ()
    {
      if( !this.ListViewConfigured )
      {
        this.ListViewConfigured = true;
      }
    }

    /**************************************************************************/

    public void ClearData ()
    {
      if( this.MainForm.InvokeRequired )
      {
        this.MainForm.Invoke(
          new MethodInvoker (
            delegate
            {
              this.lvListView.Items.Clear();
            }
          )
        );
      }
      else
      {
        this.lvListView.Items.Clear();
      }
    }

    /**************************************************************************/

    public void RefreshData ( Dictionary<string,Boolean> History )
    {
      if( this.MainForm.InvokeRequired )
      {
        this.MainForm.Invoke(
          new MethodInvoker (
            delegate
            {
              lock( this.lvListView )
              {
                Cursor.Current = Cursors.WaitCursor;
                this.RenderListView( History: History );
                Cursor.Current = Cursors.Default;

              }
            }
          )
        );
      }
      else
      {
        lock( this.lvListView )
        {
          Cursor.Current = Cursors.WaitCursor;
          this.RenderListView( History: History );
          Cursor.Current = Cursors.Default;
        }
      }
    }

    /**************************************************************************/

    private void RenderListView ( Dictionary<string,Boolean> History )
    {

      if( History.Count == 0 )
      {
        return;
      }
      
      MacroscopeAllowedHosts AllowedHosts = this.MainForm.GetJobMaster().GetAllowedHosts();
      MacroscopeSinglePercentageProgressForm ProgressForm = new MacroscopeSinglePercentageProgressForm ();
      decimal Count = 0;
      decimal TotalDocs = ( decimal )History.Count;
      decimal MajorPercentage = ( ( decimal )100 / TotalDocs ) * Count;
      
      ProgressForm.Show();
      
      ProgressForm.UpdatePercentages(
        Title: "Preparing Display",
        Message: "Processing document collection for display:",
        MajorPercentage: MajorPercentage,
        ProgressLabelMajor: string.Format( "Document {0} / {1}", Count, TotalDocs )
      );  
      
      this.lvListView.BeginUpdate();
              
      foreach( string Url in History.Keys )
      {

        ListViewItem lvItem = null;
        string Visited = "No";

        if( History[ Url ] )
        {
          Visited = "Yes";
        }

        if( this.lvListView.Items.ContainsKey( Url ) )
        {

          try
          {
            lvItem = this.lvListView.Items[ Url ];
            lvItem.SubItems[ 1 ].Text = Visited;
          }
          catch( Exception ex )
          {
            DebugMsg( string.Format( "RenderListView 1: {0}", ex.Message ) );
          }

        }
        else
        {

          try
          {
            lvItem = new ListViewItem ( Url );
            lvItem.UseItemStyleForSubItems = false;
            lvItem.Name = Url;
            lvItem.SubItems.Add( Visited );
            this.lvListView.Items.Add( lvItem );
          }
          catch( Exception ex )
          {
            DebugMsg( string.Format( "RenderListView 2: {0}", ex.Message ) );
          }

        }

        if( lvItem != null )
        {

          lvItem.ForeColor = Color.Blue;

          if( AllowedHosts.IsInternalUrl( Url ) )
          {
            lvItem.SubItems[ 0 ].ForeColor = Color.Green;
            if( History[ Url ] )
            {
              lvItem.SubItems[ 1 ].ForeColor = Color.Green;
            }
            else
            {
              lvItem.SubItems[ 1 ].ForeColor = Color.Red;
            }
          }
          else
          {
            lvItem.SubItems[ 0 ].ForeColor = Color.Gray;
            lvItem.SubItems[ 1 ].ForeColor = Color.Gray;
          }

        }

        Count++;
        MajorPercentage = ( ( decimal )100 / TotalDocs ) * Count;
        
        ProgressForm.UpdatePercentages(
          Title: null,
          Message: null,
          MajorPercentage: MajorPercentage,
          ProgressLabelMajor: string.Format( "Document {0} / {1}", Count, TotalDocs )
        );

      }

      this.lvListView.EndUpdate();
              
      ProgressForm.Close();
      
      ProgressForm.Dispose();
      
    }

    /**************************************************************************/

  }

}
