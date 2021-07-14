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
    /// InteractionChannel Service class
    /// </summary>
    public partial class InteractionChannelService : Service<InteractionChannel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionChannelService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public InteractionChannelService(RockContext context) : base(context)
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
        public bool CanDelete( InteractionChannel item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<InteractionComponent>( Context ).Queryable().Any( a => a.InteractionChannelId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", InteractionChannel.FriendlyTypeName, InteractionComponent.FriendlyTypeName );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// InteractionChannel View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( InteractionChannel ) )]
    public partial class InteractionChannelViewModelHelper : ViewModelHelper<InteractionChannel, Rock.ViewModel.InteractionChannelViewModel>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.InteractionChannelViewModel CreateViewModel( InteractionChannel model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.InteractionChannelViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                ChannelData = model.ChannelData,
                ChannelDetailTemplate = model.ChannelDetailTemplate,
                ChannelEntityId = model.ChannelEntityId,
                ChannelListTemplate = model.ChannelListTemplate,
                ChannelTypeMediumValueId = model.ChannelTypeMediumValueId,
                ComponentCacheDuration = model.ComponentCacheDuration,
                ComponentCustom1Label = model.ComponentCustom1Label,
                ComponentCustom2Label = model.ComponentCustom2Label,
                ComponentCustomIndexed1Label = model.ComponentCustomIndexed1Label,
                ComponentDetailTemplate = model.ComponentDetailTemplate,
                ComponentEntityTypeId = model.ComponentEntityTypeId,
                ComponentListTemplate = model.ComponentListTemplate,
                EngagementStrength = model.EngagementStrength,
                InteractionCustom1Label = model.InteractionCustom1Label,
                InteractionCustom2Label = model.InteractionCustom2Label,
                InteractionCustomIndexed1Label = model.InteractionCustomIndexed1Label,
                InteractionDetailTemplate = model.InteractionDetailTemplate,
                InteractionEntityTypeId = model.InteractionEntityTypeId,
                InteractionListTemplate = model.InteractionListTemplate,
                IsActive = model.IsActive,
                Name = model.Name,
                RetentionDuration = model.RetentionDuration,
                SessionDetailTemplate = model.SessionDetailTemplate,
                SessionListTemplate = model.SessionListTemplate,
                UsesSession = model.UsesSession,
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
    public static partial class InteractionChannelExtensionMethods
    {
        /// <summary>
        /// Clones this InteractionChannel object to a new InteractionChannel object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static InteractionChannel Clone( this InteractionChannel source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as InteractionChannel;
            }
            else
            {
                var target = new InteractionChannel();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this InteractionChannel object to a new InteractionChannel object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static InteractionChannel CloneWithoutIdentity( this InteractionChannel source )
        {
            var target = new InteractionChannel();
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
        /// Copies the properties from another InteractionChannel object to this InteractionChannel object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this InteractionChannel target, InteractionChannel source )
        {
            target.Id = source.Id;
            target.ChannelData = source.ChannelData;
            target.ChannelDetailTemplate = source.ChannelDetailTemplate;
            target.ChannelEntityId = source.ChannelEntityId;
            target.ChannelListTemplate = source.ChannelListTemplate;
            target.ChannelTypeMediumValueId = source.ChannelTypeMediumValueId;
            target.ComponentCacheDuration = source.ComponentCacheDuration;
            target.ComponentCustom1Label = source.ComponentCustom1Label;
            target.ComponentCustom2Label = source.ComponentCustom2Label;
            target.ComponentCustomIndexed1Label = source.ComponentCustomIndexed1Label;
            target.ComponentDetailTemplate = source.ComponentDetailTemplate;
            target.ComponentEntityTypeId = source.ComponentEntityTypeId;
            target.ComponentListTemplate = source.ComponentListTemplate;
            target.EngagementStrength = source.EngagementStrength;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.InteractionCustom1Label = source.InteractionCustom1Label;
            target.InteractionCustom2Label = source.InteractionCustom2Label;
            target.InteractionCustomIndexed1Label = source.InteractionCustomIndexed1Label;
            target.InteractionDetailTemplate = source.InteractionDetailTemplate;
            target.InteractionEntityTypeId = source.InteractionEntityTypeId;
            target.InteractionListTemplate = source.InteractionListTemplate;
            target.IsActive = source.IsActive;
            target.Name = source.Name;
            target.RetentionDuration = source.RetentionDuration;
            target.SessionDetailTemplate = source.SessionDetailTemplate;
            target.SessionListTemplate = source.SessionListTemplate;
            target.UsesSession = source.UsesSession;
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
        public static Rock.ViewModel.InteractionChannelViewModel ToViewModel( this InteractionChannel model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new InteractionChannelViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
