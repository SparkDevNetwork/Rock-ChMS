﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Datamart.Migrations
{
    [MigrationNumber( 5, "1.3.4" )]
    class StepsColumns : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"ALTER TABLE [_church_ccv_Datamart_Person] ADD [GivingLast12Months] money NULL" );
            Sql( @"ALTER TABLE [_church_ccv_Datamart_Person] ADD [IsCoaching] bit NULL" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
