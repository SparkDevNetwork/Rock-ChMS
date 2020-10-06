﻿// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using RestSharp;
using RestSharp.Deserializers;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data.SqlClient;

namespace RockWeb.Plugins.com_bemaservices.Reporting
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Test SQL Connectivity" )]
    [Category( "BEMA Services > Reporting" )]
    [Description( "Used to Test SQL" )]
    [TextField("SQL Server", "Database Server")]
    [TextField( "SQL Database", "Database" )]
    [TextField( "SQL User", "SQL User" )]
    [TextField( "SQL Password", "SQL Password" )]
    public partial class TestSQLConnectivity : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        protected void btnRun_Click( object sender, EventArgs e )
        {
            if( string.IsNullOrEmpty( GetAttributeValue( "SQLServer" ) ) ||
                string.IsNullOrEmpty( GetAttributeValue( "SQLDatabase" ) ) ||
                string.IsNullOrEmpty( GetAttributeValue( "SQLUser" ) ) ||
                string.IsNullOrEmpty( GetAttributeValue( "SQLPassword" ) )
            )
            {
                lAlert.Text = "<div class='alert alert-warning'>You must configure the block settings before running SQL</div>";
                lAlert.Visible = true;
            }
            else
            {
                var connectionString = string.Format( "Data Source={0};Initial Catalog={1}; User Id={2}; password={3};MultipleActiveResultSets=true",
                    GetAttributeValue( "SQLServer" ),
                    GetAttributeValue( "SQLDatabase" ),
                    GetAttributeValue( "SQLUser" ),
                    GetAttributeValue( "SQLPassword" )
                );

                SqlCommand sql = new SqlCommand();
                sql.Connection = new SqlConnection( connectionString );
                sql.CommandText = ceMain.Text;
                sql.Connection.Open();

                try
                {
                    SqlDataReader sqlDataReader = sql.ExecuteReader();
                    while ( sqlDataReader.Read() )
                    {
                        tbResult.Text = sqlDataReader[0].ToString();
                        tbResult.Visible = true;
                    }
                }
                catch ( SqlException exception )
                {
                    lAlert.Text = "<div class='alert alert-warning'>" + exception.Message + "</div>";
                    lAlert.Visible = true;
                }

                sql.Connection.Close();
            }
        }

        #endregion
    }
}