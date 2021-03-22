using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class UserLoginTests
    {
        [TestMethod]
        public void PostSave_ShouldUpdatePersonAccountProtectionProfileToMedium()
        {
            var personGuid = Guid.NewGuid();
            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( AccountProtectionProfile.Low, person.AccountProtectionProfile );

                var userLogin = new UserLogin
                {
                    UserName = personGuid.ToString(),
                    Password = "$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C",
                    PersonId = person.Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
                };

                var userLoginService = new UserLoginService( rockContext );
                userLoginService.Add( userLogin );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( AccountProtectionProfile.Medium, person.AccountProtectionProfile );
            }
        }

        [TestMethod]
        [DataRow( AccountProtectionProfile.High )]
        [DataRow( AccountProtectionProfile.Extreme )]
        public void PostSave_ShouldNotUpdatePersonAccountProtectionProfileToMedium( int expectedAccountProtectionProfile )
        {
            var personGuid = Guid.NewGuid();

            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid,
                AccountProtectionProfile = ( AccountProtectionProfile ) expectedAccountProtectionProfile
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( ( AccountProtectionProfile ) expectedAccountProtectionProfile, person.AccountProtectionProfile );

                var userLogin = new UserLogin
                {
                    UserName = personGuid.ToString(),
                    Password = "$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C",
                    PersonId = person.Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
                };

                var userLoginService = new UserLoginService( rockContext );
                userLoginService.Add( userLogin );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.That.AreEqual( ( AccountProtectionProfile ) expectedAccountProtectionProfile, person.AccountProtectionProfile );
            }
        }
    }
}
