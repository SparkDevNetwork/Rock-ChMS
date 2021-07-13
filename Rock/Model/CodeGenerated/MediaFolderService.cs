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
    /// MediaFolder Service class
    /// </summary>
    public partial class MediaFolderService : Service<MediaFolder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFolderService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public MediaFolderService(RockContext context) : base(context)
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
        public bool CanDelete( MediaFolder item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// MediaFolder View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( MediaFolder ) )]
    public partial class MediaFolderViewModelHelper : ViewModelHelper<MediaFolder, Rock.ViewModel.MediaFolderViewModel>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.MediaFolderViewModel CreateViewModel( MediaFolder model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.MediaFolderViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                ContentChannelAttributeId = model.ContentChannelAttributeId,
                ContentChannelId = model.ContentChannelId,
                ContentChannelItemStatus = ( int? ) model.ContentChannelItemStatus,
                Description = model.Description,
                IsContentChannelSyncEnabled = model.IsContentChannelSyncEnabled,
                IsPublic = model.IsPublic,
                MediaAccountId = model.MediaAccountId,
                MetricData = model.MetricData,
                Name = model.Name,
                SourceData = model.SourceData,
                SourceKey = model.SourceKey,
                WorkflowTypeId = model.WorkflowTypeId,
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
    public static partial class MediaFolderExtensionMethods
    {
        /// <summary>
        /// Clones this MediaFolder object to a new MediaFolder object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static MediaFolder Clone( this MediaFolder source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as MediaFolder;
            }
            else
            {
                var target = new MediaFolder();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this MediaFolder object to a new MediaFolder object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static MediaFolder CloneWithoutIdentity( this MediaFolder source )
        {
            var target = new MediaFolder();
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
        /// Copies the properties from another MediaFolder object to this MediaFolder object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this MediaFolder target, MediaFolder source )
        {
            target.Id = source.Id;
            target.ContentChannelAttributeId = source.ContentChannelAttributeId;
            target.ContentChannelId = source.ContentChannelId;
            target.ContentChannelItemStatus = source.ContentChannelItemStatus;
            target.Description = source.Description;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IsContentChannelSyncEnabled = source.IsContentChannelSyncEnabled;
            target.IsPublic = source.IsPublic;
            target.MediaAccountId = source.MediaAccountId;
            target.MetricData = source.MetricData;
            target.Name = source.Name;
            target.SourceData = source.SourceData;
            target.SourceKey = source.SourceKey;
            target.WorkflowTypeId = source.WorkflowTypeId;
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
        public static Rock.ViewModel.MediaFolderViewModel ToViewModel( this MediaFolder model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new MediaFolderViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
