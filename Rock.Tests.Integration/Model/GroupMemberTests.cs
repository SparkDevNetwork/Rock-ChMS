using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class GroupMemberTests
    {
        [TestMethod]
        [DataRow( ElevatedSecurityLevel.None, AccountProtectionProfile.Low )]
        [DataRow( ElevatedSecurityLevel.Low, AccountProtectionProfile.High )]
        [DataRow( ElevatedSecurityLevel.High, AccountProtectionProfile.Extreme )]
        public void PostSave_ShouldUpdatePersonAccountProtectionProfileToCorrectValue( int expectedElevatedSecurityLevel, int expectedAccountProtectionProfile )
        {
            var personGuid = Guid.NewGuid();
            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid,
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( AccountProtectionProfile.Low, person.AccountProtectionProfile );

                var group = CreateTestGroup( rockContext, ( ElevatedSecurityLevel ) expectedElevatedSecurityLevel );
                var groupMember = new GroupMember
                {
                    Group = group,
                    PersonId = person.Id,
                    GroupRole = group.GroupType.Roles.FirstOrDefault(),
                    GroupMemberStatus = GroupMemberStatus.Active
                };

                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.Add( groupMember );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( ( AccountProtectionProfile ) expectedAccountProtectionProfile, person.AccountProtectionProfile );
            }
        }

        [TestMethod]
        [DataRow( ElevatedSecurityLevel.None )]
        [DataRow( ElevatedSecurityLevel.Low )]
        [DataRow( ElevatedSecurityLevel.High )]
        public void PostSave_ShouldNotUpdatePersonAccountProtectionProfileToLowerValue( int expectedElevatedSecurityLevel )
        {
            var personGuid = Guid.NewGuid();
            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid,
                AccountProtectionProfile = AccountProtectionProfile.Extreme
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( AccountProtectionProfile.Extreme, person.AccountProtectionProfile );

                var group = CreateTestGroup( rockContext, ( ElevatedSecurityLevel ) expectedElevatedSecurityLevel );
                var groupMember = new GroupMember
                {
                    Group = group,
                    PersonId = person.Id,
                    GroupRole = group.GroupType.Roles.FirstOrDefault(),
                    GroupMemberStatus = GroupMemberStatus.Active
                };

                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.Add( groupMember );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( AccountProtectionProfile.Extreme, person.AccountProtectionProfile );
            }
        }

        private Group CreateTestGroup( RockContext rockContext, ElevatedSecurityLevel elevatedSecurityLevel )
        {
            var groupGuid = Guid.NewGuid();
            var groupTypeGuid = Guid.NewGuid();

            var group = new Group
            {
                Name = $"Test Group {groupGuid}",
                IsSecurityRole = true,
                Guid = groupGuid,
                ElevatedSecurityLevel = elevatedSecurityLevel,
                GroupType = new GroupType
                {
                    Name = $"Test Group Type {groupTypeGuid}",
                    Guid = groupTypeGuid,
                },
            };


            var groupService = new GroupService( rockContext );
            groupService.Add( group );
            rockContext.SaveChanges();

            var groupTypeRole = new GroupTypeRole
            {
                Name = "Test Role",
                GroupTypeId = group.GroupTypeId
            };
            var groupTypeRoleService = new GroupTypeRoleService( rockContext );
            groupTypeRoleService.Add( groupTypeRole );
            rockContext.SaveChanges();

            return group;
        }
    }
}
