//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

using System;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModel;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// BinaryFile Service class
    /// </summary>
    public partial class BinaryFileService : Service<BinaryFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public BinaryFileService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( BinaryFile item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<AchievementType>( Context ).Queryable().Any( a => a.ImageBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, AchievementType.FriendlyTypeName );
                return false;
            }

            if ( new Service<BackgroundCheck>( Context ).Queryable().Any( a => a.ResponseDocumentId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, BackgroundCheck.FriendlyTypeName );
                return false;
            }

            if ( new Service<BenevolenceRequestDocument>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, BenevolenceRequestDocument.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationAttachment>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationAttachment.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationResponseAttachment>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationResponseAttachment.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationTemplate>( Context ).Queryable().Any( a => a.ImageFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationTemplate>( Context ).Queryable().Any( a => a.LogoBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationTemplate>( Context ).Queryable().Any( a => a.PushImageBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<CommunicationTemplateAttachment>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, CommunicationTemplateAttachment.FriendlyTypeName );
                return false;
            }

            if ( new Service<ConnectionOpportunity>( Context ).Queryable().Any( a => a.PhotoId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, ConnectionOpportunity.FriendlyTypeName );
                return false;
            }

            if ( new Service<EventItem>( Context ).Queryable().Any( a => a.PhotoId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, EventItem.FriendlyTypeName );
                return false;
            }

            if ( new Service<FinancialAccount>( Context ).Queryable().Any( a => a.ImageBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, FinancialAccount.FriendlyTypeName );
                return false;
            }

            if ( new Service<FinancialStatementTemplate>( Context ).Queryable().Any( a => a.LogoBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, FinancialStatementTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<FinancialTransactionImage>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, FinancialTransactionImage.FriendlyTypeName );
                return false;
            }

            if ( new Service<Location>( Context ).Queryable().Any( a => a.ImageId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Location.FriendlyTypeName );
                return false;
            }

            if ( new Service<MergeTemplate>( Context ).Queryable().Any( a => a.TemplateBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, MergeTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<NoteAttachment>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, NoteAttachment.FriendlyTypeName );
                return false;
            }

            if ( new Service<Page>( Context ).Queryable().Any( a => a.IconBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Page.FriendlyTypeName );
                return false;
            }

            if ( new Service<Person>( Context ).Queryable().Any( a => a.PhotoId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Person.FriendlyTypeName );
                return false;
            }

            if ( new Service<SignatureDocument>( Context ).Queryable().Any( a => a.BinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, SignatureDocument.FriendlyTypeName );
                return false;
            }

            if ( new Service<Site>( Context ).Queryable().Any( a => a.ConfigurationMobilePhoneBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }

            if ( new Service<Site>( Context ).Queryable().Any( a => a.ConfigurationMobileTabletBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }

            if ( new Service<Site>( Context ).Queryable().Any( a => a.FavIconBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }

            if ( new Service<Site>( Context ).Queryable().Any( a => a.SiteLogoBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }

            if ( new Service<Site>( Context ).Queryable().Any( a => a.ThumbnailBinaryFileId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", BinaryFile.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// BinaryFile View Model Helper
    /// </summary>
    public partial class BinaryFileViewModelHelper : ViewModelHelper<BinaryFile, Rock.ViewModel.BinaryFileViewModel>
    {
        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.BinaryFileViewModel CreateViewModel( BinaryFile model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.BinaryFileViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                BinaryFileTypeId = model.BinaryFileTypeId,
                ContentLastModified = model.ContentLastModified,
                Description = model.Description,
                FileName = model.FileName,
                FileSize = model.FileSize,
                Height = model.Height,
                IsSystem = model.IsSystem,
                IsTemporary = model.IsTemporary,
                MimeType = model.MimeType,
                Path = model.Path,
                StorageEntitySettings = model.StorageEntitySettings,
                Width = model.Width,
                CreatedDateTime = model.CreatedDateTime,
                ModifiedDateTime = model.ModifiedDateTime,
                CreatedByPersonAliasId = model.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = model.ModifiedByPersonAliasId,
            };

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );
            return viewModel;
        }
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class BinaryFileExtensionMethods
    {
        /// <summary>
        /// Clones this BinaryFile object to a new BinaryFile object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static BinaryFile Clone( this BinaryFile source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as BinaryFile;
            }
            else
            {
                var target = new BinaryFile();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this BinaryFile object to a new BinaryFile object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static BinaryFile CloneWithoutIdentity( this BinaryFile source )
        {
            var target = new BinaryFile();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another BinaryFile object to this BinaryFile object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this BinaryFile target, BinaryFile source )
        {
            target.Id = source.Id;
            target.BinaryFileTypeId = source.BinaryFileTypeId;
            target.ContentLastModified = source.ContentLastModified;
            target.Description = source.Description;
            target.FileName = source.FileName;
            target.FileSize = source.FileSize;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Height = source.Height;
            target.IsSystem = source.IsSystem;
            target.IsTemporary = source.IsTemporary;
            target.MimeType = source.MimeType;
            target.Path = source.Path;
            target.StorageEntitySettings = source.StorageEntitySettings;
            target.Width = source.Width;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }

        /// <summary>
        /// Creates a view model from this entity
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson" >The currentPerson.</param>
        /// <param name="loadAttributes" >Load attributes?</param>
        public static Rock.ViewModel.BinaryFileViewModel ToViewModel( this BinaryFile model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new BinaryFileViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
