﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MCNet = MailChimp.Net;
using MCInterfaces = MailChimp.Net.Interfaces;
using MCModels = MailChimp.Net.Models;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;
using System.Data.Entity;

namespace com.bemaservices.MailChimp.Utility
{
    public class MailChimpApi
    {
        private MCInterfaces.IMailChimpManager _mailChimpManager;
        private string _apiKey;
        private DefinedValueCache _mailChimpAccount;

        public MailChimpApi( DefinedValueCache mailChimpAccount )
        {
            if ( !mailChimpAccount.DefinedType.Guid.ToString().Equals( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, StringComparison.OrdinalIgnoreCase ) )
            {
                var newException = new Exception( "Defined Value is not of type Mail Chimp Account." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                _mailChimpAccount = mailChimpAccount;

                var apiKeyAttributeKey = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE ).Key;
                _apiKey = _mailChimpAccount.GetAttributeValue( apiKeyAttributeKey );

                if ( _apiKey.IsNullOrWhiteSpace() )
                {
                    var newException = new Exception( "No Api Key provided on the Mail Account Defined Value" );
                    ExceptionLogService.LogException( newException );
                }
                else
                {
                    _mailChimpManager = new MCNet.MailChimpManager( _apiKey );
                }
            }
        }

        public List<DefinedValue> GetMailChimpLists()
        {
            List<DefinedValue> mailChimpListValues = null;

            if ( _mailChimpAccount is null || _apiKey.IsNullOrWhiteSpace() )
            {
                var newException = new Exception( "The Helper Class has not been properly intialized with a valid Mail Chimp Account Defined Value, or the Mail Chimp Account does not have an APIKEY." );
                ExceptionLogService.LogException( newException );
            }
            else
            {
                RockContext rockContext = new RockContext();
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );

                var accountAttributeGuid = MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ACCOUNT_ATTRIBUTE.AsGuid();
                var mailChimpListDefinedValueIds = attributeValueService.Queryable().Where( a => a.Attribute.Guid == accountAttributeGuid && a.Value.Equals( _mailChimpAccount.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) ).Select( a => a.EntityId ).ToList();

                mailChimpListValues = definedValueService.GetByDefinedTypeGuid( new Guid( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES ) ).Where( v => mailChimpListDefinedValueIds.Contains( v.Id ) ).ToList();

                try
                {
                    var mailChimpListCollection = _mailChimpManager.Lists.GetAllAsync().Result;

                    // Loop over each List from Mail Chimp and attempt to find the related Defined Value in Rock.  Then Update / Add those defined values into Rock
                    foreach ( var mailChimpList in mailChimpListCollection )
                    {
                        var mailChimpListValue = mailChimpListValues.Where( x => x.ForeignId == mailChimpList.WebId &&
                                                                                x.ForeignKey == MailChimp.Constants.ForeignKey )
                                                                    .FirstOrDefault();
                        if ( mailChimpListValue is null )
                        {

                            mailChimpListValue = new DefinedValue();
                            mailChimpListValue.ForeignId = mailChimpList.WebId;
                            mailChimpListValue.ForeignKey = MailChimp.Constants.ForeignKey;
                            mailChimpListValue.IsSystem = true;
                            mailChimpListValue.DefinedTypeId = DefinedTypeCache.Get( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_AUDIENCES.AsGuid() ).Id;
                            mailChimpListValue.Value = mailChimpList.Name;

                            definedValueService.Add( mailChimpListValue );

                            rockContext.SaveChanges();

                            mailChimpListValues.Add( mailChimpListValue );

                        }
                        UpdateMailChimpListDefinedValue( mailChimpList, ref mailChimpListValue, rockContext );
                    }

                    // Look for any DefinedValues in Rock that are no longer in Mail Chimp and remove them.  We also need to remove any attribute Values assigned to these lists.
                    var mailChimpListValuesToRemove = mailChimpListValues
                                                       .Where( x => !mailChimpListCollection.Any( y => y.WebId == x.ForeignId && x.ForeignKey == MailChimp.Constants.ForeignKey )
                                                       && mailChimpListDefinedValueIds.Contains( x.Id )
                                                       );

                    var attributeValuesToRemove = attributeValueService.Queryable().Where( av => mailChimpListValuesToRemove.Any( dv => dv.Guid.ToString() == av.Value ) );

                    foreach ( var definedValue in mailChimpListValuesToRemove )
                    {
                        mailChimpListValues.Remove( definedValue );
                    }

                    attributeValueService.DeleteRange( attributeValuesToRemove );
                    definedValueService.DeleteRange( mailChimpListValuesToRemove );

                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            return mailChimpListValues;
        }

        public void SyncMembers( DefinedValueCache mailChimpList )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            GroupService groupService = new GroupService( rockContext );
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );
            PersonService personService = new PersonService( rockContext );

            Dictionary<int, MCModels.Member> mailChimpMemberLookUp = new Dictionary<int, MCModels.Member>();
            var mailChimpListIdAttributeKey = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ID_ATTRIBUTE.AsGuid() ).Key;
            var mailChimpListId = mailChimpList.GetAttributeValue( mailChimpListIdAttributeKey );

            try
            {
                var mailChimpMembers = _mailChimpManager.Members.GetAllAsync( mailChimpListId ).Result;

                //Match all the mailChimpMembers to people in Rock.
                foreach ( var member in mailChimpMembers )
                {
                    var personId = getRockPerson( member ).Id;
                    mailChimpMemberLookUp.Add( personId, member );
                }

                //Get all Groups that have an attribute set to this Mail Chimp List's Defined Value.
                var groupIds = attributeValueService.Queryable().AsNoTracking()
                    .Where( x => x.Value.Equals( mailChimpList.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) &&
                                 x.Attribute.EntityType.FriendlyName == Rock.Model.Group.FriendlyTypeName )
                    .Select( x => x.EntityId );

                if ( groupIds.Any() )
                {
                    foreach ( var groupId in groupIds.Where( g => g.HasValue ).Distinct().ToList() )
                    {
                        var group = groupService.Get( groupId.Value );
                        if ( group != null )
                        {
                            var rockGroupMembers = groupMemberService.GetByGroupId( group.Id );
                            // Filter out Inactive and Archived Group Members for adding new members to Mail Chimp.
                            var rockPeopleToAdd = rockGroupMembers.Where( m => !mailChimpMemberLookUp.Keys.ToList().Contains( m.PersonId ) &&
                                                                                m.GroupMemberStatus != GroupMemberStatus.Inactive && !m.IsArchived );
                            foreach ( var groupMember in rockPeopleToAdd )
                            {
                                addPersonToMailChimp( groupMember.Person, mailChimpListId );
                            }

                            foreach ( var person in mailChimpMemberLookUp )
                            {
                                var groupMember = rockGroupMembers.Where( m => m.PersonId == person.Key ).FirstOrDefault();
                                if ( groupMember.IsNull() )
                                {
                                    groupMember = new GroupMember { PersonId = person.Key, GroupId = group.Id };
                                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? group.GroupType.Roles.First().Id;
                                    groupMemberService.Add( groupMember );
                                }
                                groupMember.GroupMemberStatus = GetRockGroupMemberStatus( person.Value.Status );
                                var rockPerson = personService.Get( person.Key );
                                var mailChimpPerson = person.Value;
                                syncPerson( ref rockPerson, ref mailChimpPerson, mailChimpListId );
                            }
                        }
                    }
                }

                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        private bool addPersonToMailChimp( Person person, string mailChimpListId )
        {
            bool foundMember = false;
            bool addedPerson = false;

            try
            {
                foundMember = _mailChimpManager.Members.ExistsAsync( mailChimpListId, person.Email, null, false ).Result;

                if ( !foundMember )
                {
                    RockContext rockContext = new RockContext();
                    var emailTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL ).Id;
                    var EmailAddresses = person.GetPersonSearchKeys( rockContext ).AsNoTracking().Where( k => k.SearchTypeValueId == emailTypeId ).Select( x => x.SearchValue );
                    foreach ( var email in EmailAddresses )
                    {
                        try
                        {
                            foundMember = _mailChimpManager.Members.ExistsAsync( mailChimpListId, email, null, false ).Result;
                            if ( foundMember )
                            {
                                // if the Person is found using an alternate email address, their email address needs to be updated in mail chimp.
                                var member = new MCModels.Member
                                {
                                    EmailAddress = person.Email,
                                    StatusIfNew = MCModels.Status.Subscribed,
                                    Id = _mailChimpManager.Members.Hash( email )
                                };
                                break;
                            }
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            if ( !foundMember )
            {
                MCModels.Member member = null;
                syncPerson( ref person, ref member, mailChimpListId );
                addedPerson = true;
            }

            return addedPerson;
        }

        private void UpdateMailChimpListDefinedValue( MCModels.List mailChimpList, ref DefinedValue mailChimpListValue, RockContext rockContext )
        {

            mailChimpListValue.Value = mailChimpList.Name;
            mailChimpListValue.Description = mailChimpList.SubscribeUrlLong;

            var mailChimpAccountAttribute = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ACCOUNT_ATTRIBUTE );
            var mailChimpIdAttribute = AttributeCache.Get( MailChimp.SystemGuid.Attribute.MAIL_CHIMP_AUDIENCE_ID_ATTRIBUTE );

            Rock.Attribute.Helper.SaveAttributeValue( mailChimpListValue, mailChimpAccountAttribute, _mailChimpAccount.Guid.ToString(), rockContext );
            Rock.Attribute.Helper.SaveAttributeValue( mailChimpListValue, mailChimpIdAttribute, mailChimpList.Id, rockContext );
        }

        private Person getRockPerson( MCModels.Member member )
        {
            Person person = null;
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var firstName = member.MergeFields["FNAME"].ToString();
            var lastName = member.MergeFields["LNAME"].ToString();
            var email = member.EmailAddress;
            string emailNote = null;
            bool isEmailActive = getIsEmailActive( member.Status, out emailNote );

            var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, null, null, null, null, null );

            person = personService.FindPerson( personQuery, false );

            if ( person.IsNull() )
            {
                // Add New Person
                person = new Person();
                person.FirstName = firstName.FixCase();
                person.LastName = lastName.FixCase();
                person.IsEmailActive = isEmailActive;
                person.Email = email;
                person.EmailPreference = getRockEmailPrefernce( member.Status );
                person.EmailNote = emailNote;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                var familyGroup = PersonService.SaveNewPerson( person, rockContext, null, false );
                if ( familyGroup != null && familyGroup.Members.Any() )
                {
                    person = familyGroup.Members.Select( m => m.Person ).First();
                }
            }

            rockContext.SaveChanges();

            return person;
        }

        private void syncPerson( ref Person rockPerson, ref MCModels.Member mailChimpMember, string mailChimpListId )
        {
            if ( mailChimpMember.IsNull() )
            {
                if ( rockPerson.Email.IsNotNullOrWhiteSpace() )
                {
                    mailChimpMember = new MCModels.Member { EmailAddress = rockPerson.Email, StatusIfNew = MCModels.Status.Subscribed };
                }
                else
                {
                    Exception ex = new Exception( rockPerson.FullName + " was not synced to Mail Chimp because they do not have an email address" );
                    ExceptionLogService.LogException( ex );
                }
            }
            else
            {
                // If the Email Addresses Match, Check the Mail Chimp's Email Status to Update the Rock record.
                // There's a chance they won't match, because Rock matches on Search Keys which could have contained the old email address.
                // If They Don't Match, Update Mail Chimp to the Rock Person's Email Address
                if ( mailChimpMember.EmailAddress.Equals( rockPerson.Email, StringComparison.OrdinalIgnoreCase ) )
                {
                    string emailNote;
                    rockPerson.EmailPreference = getRockEmailPrefernce( mailChimpMember.Status );
                    rockPerson.IsEmailActive = getIsEmailActive( mailChimpMember.Status, out emailNote );
                    rockPerson.EmailNote = emailNote;
                }
                else
                {
                    mailChimpMember.EmailAddress = rockPerson.Email;
                }
            }

            try
            {
                if ( mailChimpMember.IsNotNull() )
                {

                    // Update Mail Chimp with the Rock Person's First And Last Name
                    mailChimpMember.MergeFields.AddOrReplace( "FNAME", rockPerson.NickName );
                    mailChimpMember.MergeFields.AddOrReplace( "LNAME", rockPerson.LastName );

                    var result = _mailChimpManager.Members.AddOrUpdateAsync( mailChimpListId, mailChimpMember ).Result;

                }
            }
            catch ( System.AggregateException e )
            {
                foreach ( var ex in e.InnerExceptions )
                {
                    if ( ex is MCNet.Core.MailChimpException )
                    {
                        var mailChimpException = ex as MCNet.Core.MailChimpException;
                        ExceptionLogService.LogException( new Exception( mailChimpException.Message + mailChimpException.Detail ) );
                    }
                    else
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        private EmailPreference getRockEmailPrefernce( MCModels.Status mailChimpEmailStatus )
        {
            EmailPreference preference = EmailPreference.EmailAllowed;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Unsubscribed:
                    preference = EmailPreference.NoMassEmails;
                    break;
                case MCModels.Status.Cleaned:
                    preference = EmailPreference.DoNotEmail;
                    break;
            }

            return preference;
        }

        private bool getIsEmailActive( MCModels.Status mailChimpEmailStatus, out string emailNote )
        {
            bool isActive = true;
            emailNote = null;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Cleaned:
                    isActive = false;
                    emailNote = "Email was marked as cleaned in Mail Chimp";
                    break;
            }

            return isActive;
        }

        private GroupMemberStatus GetRockGroupMemberStatus( MCModels.Status mailChimpEmailStatus )
        {
            GroupMemberStatus memberStatus = GroupMemberStatus.Inactive;

            switch ( mailChimpEmailStatus )
            {
                case MCModels.Status.Subscribed:
                    memberStatus = GroupMemberStatus.Active;
                    break;
                case MCModels.Status.Pending:
                    memberStatus = GroupMemberStatus.Pending;
                    break;
            }

            return memberStatus;
        }
    }
}
