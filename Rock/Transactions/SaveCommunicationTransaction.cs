﻿// <copyright>
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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class SaveCommunicationTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the recipient emails.
        /// </summary>
        /// <value>
        /// The recipient emails.
        /// </value>
        public List<string> RecipientEmails { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from address.
        /// </summary>
        /// <value>
        /// From address.
        /// </value>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the reply to.
        /// </summary>
        /// <value>
        /// The reply to.
        /// </value>
        public string ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the HTML message.
        /// </summary>
        /// <value>
        /// The HTML message.
        /// </value>
        public string HtmlMessage { get; set; }

        /// <summary>
        /// Gets or sets the text message.
        /// </summary>
        /// <value>
        /// The text message.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete("Text Message property is no longer supported for emails", true )]
        public string TextMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [bulk communication].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bulk communication]; otherwise, <c>false</c>.
        /// </value>
        public bool BulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the recipient status.
        /// </summary>
        /// <value>
        /// The recipient status.
        /// </value>
        public CommunicationRecipientStatus RecipientStatus { get; set; }

        /// <summary>
        /// On optional guid to use if one and only one recipient will be created. Used for tracking opens/clicks.
        /// </summary>
        /// <value>
        /// The recipient unique identifier.
        /// </value>
        public Guid? RecipientGuid { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was sent.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        public SaveCommunicationTransaction()
        {
            RecipientStatus = CommunicationRecipientStatus.Delivered;
            SendDateTime = RockDateTime.Now;
            BulkCommunication = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        private SaveCommunicationTransaction( string fromName, string fromAddress, string subject, string message )
            : this()
        {
            FromName = fromName;
            FromAddress = fromAddress;
            ReplyTo = fromAddress;
            Subject = subject;
            HtmlMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public SaveCommunicationTransaction( string to, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            RecipientEmails = to.SplitDelimitedValues().ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public SaveCommunicationTransaction( List<string> to, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            RecipientEmails = to;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                int? senderPersonAliasId = null;
                if ( FromAddress.IsNotNullOrWhiteSpace() )
                {
                    var sender = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Email == FromAddress )
                        .FirstOrDefault();
                    senderPersonAliasId = sender != null ? sender.PrimaryAliasId : (int?)null;
                }

                var communication = new CommunicationService( rockContext ).CreateEmailCommunication(
                    RecipientEmails, FromName, FromAddress, ReplyTo, Subject, HtmlMessage, BulkCommunication, SendDateTime,
                    RecipientStatus, senderPersonAliasId );

                if ( communication != null && communication.Recipients.Count() == 1 && RecipientGuid.HasValue )
                {
                    communication.Recipients.First().Guid = RecipientGuid.Value;
                }

                rockContext.SaveChanges();
            }
        }
    }
}