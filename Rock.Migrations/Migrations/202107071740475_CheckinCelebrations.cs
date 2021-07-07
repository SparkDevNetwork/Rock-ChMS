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
    public partial class CheckinCelebrations : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.AchievementType", "IsPublic", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.AchievementType", "ImageBinaryFileId", c => c.Int() );
            AddColumn( "dbo.AchievementType", "CustomSummaryLavaTemplate", c => c.String() );
            CreateIndex( "dbo.AchievementType", "ImageBinaryFileId" );
            AddForeignKey( "dbo.AchievementType", "ImageBinaryFileId", "dbo.BinaryFile", "Id" );

            // We want the "AchievementTypes" GroupType attribute to only apply to GroupType's with a GroupTypePurpose of 'Checkin Template'
            // So, we need to get the checkInTemplatePurposeValueId for that so we can use it as the EntityTypeQualifierValue
            var checkInTemplatePurposeValueId = SqlScalar( $" SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE}'" ) as int?;

            // Add the "AchievementTypes" group type attribute with a EntityTypeQualifier of GroupTypePurposeValueId
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.TEXT, "GroupTypePurposeValueId", checkInTemplatePurposeValueId?.ToString(), "Achievement Types", "", 0, "", "EECDA094-E5E2-4A47-804D-65701590F2A1", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AchievementType", "ImageBinaryFileId", "dbo.BinaryFile" );
            DropIndex( "dbo.AchievementType", new[] { "ImageBinaryFileId" } );
            DropColumn( "dbo.AchievementType", "CustomSummaryLavaTemplate" );
            DropColumn( "dbo.AchievementType", "ImageBinaryFileId" );
            DropColumn( "dbo.AchievementType", "IsPublic" );
        }
    }
}
