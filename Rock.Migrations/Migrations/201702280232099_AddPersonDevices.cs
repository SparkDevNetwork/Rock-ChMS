// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddPersonDevices : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonalDevice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        DeviceRegistrationId = c.String(),
                        PersonalDeviceTypeId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.PersonalDeviceTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => t.PersonAliasId)
                .Index(t => t.PersonalDeviceTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.AddDefinedType( "Global", "Personal Device Type", "Device type for notifications", SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE, "Mobile", "Personal Device Type Mobile", SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonalDevice", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonalDevice", "PersonalDeviceTypeId", "dbo.DefinedValue");
            DropForeignKey("dbo.PersonalDevice", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonalDevice", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonalDevice", new[] { "Guid" });
            DropIndex("dbo.PersonalDevice", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonalDevice", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonalDevice", new[] { "PersonalDeviceTypeId" });
            DropIndex("dbo.PersonalDevice", new[] { "PersonAliasId" });
            DropTable("dbo.PersonalDevice");
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE ); // Personal Device Type
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE ); // Personal Device Type Mobile
        }
    }
}
