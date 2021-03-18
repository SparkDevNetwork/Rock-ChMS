using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;

namespace Rock.Tests.Integration.Jobs
{
    [TestClass]
    public class RockCleanupJobTests
    {
        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithLoginsToAccountProtectionProfileMedium()
        {
            var expectedPerson = CreateTestPerson();
            CreateTestUserLogin( expectedPerson.Id );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedPerson.Guid );
                Assert.That.AreEqual( Rock.Utility.Enums.AccountProtectionProfile.Medium, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleInSecurityGroupsWithLowToAccountProtectionProfileHigh()
        {
            var expectedHighSecurityGroupPerson = CreateTestPerson();
            var expectedLowGroupPerson = CreateTestPerson();

            CreateTestSecurityGroupWithPersonAsMember( expectedInheritedSecurityGroupPerson.Id, ElevatedSecurityLevel.Low);
            CreateTestSecurityGroupWithPersonAsMember( expectedInheritedSecurityGroupPerson.Id, ElevatedSecurityLevel.High );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedPerson.Guid );
                Assert.That.AreEqual( Rock.Utility.Enums.AccountProtectionProfile.Medium, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleInSecurityGroupsWithHighToAccountProtectionProfileExtreme()
        {
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialPersonBankAccountToAccountProtectionProfileHigh()
        {
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialPersonSavedAccountToAccountProtectionProfileHigh()
        {
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialScheduledTransactionToAccountProtectionProfileHigh()
        {
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialTransactionToAccountProtectionProfileHigh()
        {
        }

        private Person CreateTestPerson()
        {
            var personGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                // We have to manually add the records to the database so that we can ensure the post save methods don't updated the data.
                var createPersonScript = $@"INSERT INTO [Person] (
	                [IsSystem]
	                , [IsDeceased]
	                , [Gender]
	                , [IsEmailActive]
	                , [Guid]
	                , [EmailPreference]
	                , [CommunicationPreference]
	                , [AgeClassification]
	                , [IsLockedAsChild]
	                , [GivingLeaderId]
	                , [AccountProtectionProfile]
                    , [FirstName]
                    , [LastName]
                    , [Email]
                    , [RecordTypeValueId]
                ) VALUES (
	                0 --@IsSystem
	                , 0 --@IsDeceased
	                , 1 --@Gender
	                , 0 --@IsEmailActive
	                , '{personGuid}' --@Guid
	                , 0 --@EmailPreference
	                , 0 --@CommunicationPreference
	                , 0 --@AgeClassification
	                , 0 --@IsLockedAsChild
	                , 0 --@GivingLeaderId
	                , 0 --@AccountProtectionProfile)
                    , 'Test' --[FirstName]
                    , '{personGuid}' --[LastName]
                    , '{personGuid}@test.com' --[Email]
                    , 1 --[RecordTypeValueId]
                )";
                rockContext.Database.ExecuteSqlCommand( createPersonScript );

                return new PersonService( rockContext ).Get( personGuid );
            }
        }

        private UserLogin CreateTestUserLogin( int personId )
        {
            var userLoginGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                // We have to manually add the records to the database so that we can ensure the post save methods don't updated the data.
                var createUserLoginScript = $@"INSERT INTO [UserLogin] (
                    [UserName]
                    , [Guid]
                    , [EntityTypeId]
                    , [Password]
                    , [PersonId]
                ) VALUES (
                    '{userLoginGuid}' -- UserName
                    , '{userLoginGuid}' -- Guid
                    , 27 -- EntityTypeId
                    , '$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C' -- [Password]
                    , {personId} -- [PersonId]
                )";

                rockContext.Database.ExecuteSqlCommand( createUserLoginScript );
                return new UserLoginService( rockContext ).Get( userLoginGuid );
            }
        }

        private void CreateTestSecurityGroupWithPersonAsMember( int personId, ElevatedSecurityLevel securityLevel )
        {
            var createGroupScript = $@"INSERT INTO [Group] (
	            [IsSystem]
	            , [GroupTypeId]
	            , [Name]
	            , [IsSecurityRole]
	            , [IsActive]
	            , [Order]
	            , [Guid]
	            , [IsPublic]
	            , [IsArchived]
	            , [SchedulingMustMeetRequirements]
	            , [AttendanceRecordRequiredForCheckIn]
	            , [DisableScheduleToolboxAccess]
	            , [DisableScheduling]
	            , [ElevatedSecurityLevel]
            ) VALUES (
	            0 --IsSystem
	            , 1 --GroupTypeId
	            , --Name
	            , --IsSecurityRole
	            , --IsActive
	            , --Order
	            , --Guid
	            , --IsPublic
	            , --IsArchived
	            , --SchedulingMustMeetRequirements
	            , --AttendanceRecordRequiredForCheckIn
	            , --DisableScheduleToolboxAccess
	            , --DisableScheduling
	            , --ElevatedSecurityLevel
            )";
        }

        private void ExecuteRockCleanupJob()
        {
            var jobContext = new TestJobContext();
            var job = new Rock.Jobs.RockCleanup();

            job.Execute( jobContext );
        }
    }
}
