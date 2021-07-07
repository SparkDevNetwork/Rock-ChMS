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
    /// SignatureDocumentTemplate Service class
    /// </summary>
    public partial class SignatureDocumentTemplateService : Service<SignatureDocumentTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureDocumentTemplateService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public SignatureDocumentTemplateService(RockContext context) : base(context)
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
        public bool CanDelete( SignatureDocumentTemplate item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<Group>( Context ).Queryable().Any( a => a.RequiredSignatureDocumentTemplateId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", SignatureDocumentTemplate.FriendlyTypeName, Group.FriendlyTypeName );
                return false;
            }

            if ( new Service<RegistrationTemplate>( Context ).Queryable().Any( a => a.RequiredSignatureDocumentTemplateId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", SignatureDocumentTemplate.FriendlyTypeName, RegistrationTemplate.FriendlyTypeName );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// SignatureDocumentTemplate View Model Helper
    /// </summary>
    public partial class SignatureDocumentTemplateViewModelHelper : ViewModelHelper<SignatureDocumentTemplate, Rock.ViewModel.SignatureDocumentTemplateViewModel>
    {
        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.SignatureDocumentTemplateViewModel CreateViewModel( SignatureDocumentTemplate model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.SignatureDocumentTemplateViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                BinaryFileTypeId = model.BinaryFileTypeId,
                Description = model.Description,
                InviteSystemCommunicationId = model.InviteSystemCommunicationId,
                Name = model.Name,
                ProviderEntityTypeId = model.ProviderEntityTypeId,
                ProviderTemplateKey = model.ProviderTemplateKey,
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
    public static partial class SignatureDocumentTemplateExtensionMethods
    {
        /// <summary>
        /// Clones this SignatureDocumentTemplate object to a new SignatureDocumentTemplate object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static SignatureDocumentTemplate Clone( this SignatureDocumentTemplate source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as SignatureDocumentTemplate;
            }
            else
            {
                var target = new SignatureDocumentTemplate();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this SignatureDocumentTemplate object to a new SignatureDocumentTemplate object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static SignatureDocumentTemplate CloneWithoutIdentity( this SignatureDocumentTemplate source )
        {
            var target = new SignatureDocumentTemplate();
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
        /// Copies the properties from another SignatureDocumentTemplate object to this SignatureDocumentTemplate object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this SignatureDocumentTemplate target, SignatureDocumentTemplate source )
        {
            target.Id = source.Id;
            target.BinaryFileTypeId = source.BinaryFileTypeId;
            target.CompletionSystemCommunicationId = source.CompletionSystemCommunicationId;
            target.Description = source.Description;
            target.DocumentTerm = source.DocumentTerm;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.InviteSystemCommunicationId = source.InviteSystemCommunicationId;
            #pragma warning disable 612, 618
            target.InviteSystemEmailId = source.InviteSystemEmailId;
            #pragma warning restore 612, 618
            target.IsActive = source.IsActive;
            target.LavaTemplate = source.LavaTemplate;
            target.Name = source.Name;
            target.ProviderEntityTypeId = source.ProviderEntityTypeId;
            target.ProviderTemplateKey = source.ProviderTemplateKey;
            target.SignatureType = source.SignatureType;
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
        public static Rock.ViewModel.SignatureDocumentTemplateViewModel ToViewModel( this SignatureDocumentTemplate model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new SignatureDocumentTemplateViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
