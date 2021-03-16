using System.Collections.Generic;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// This class is used to serialize and de-serialize the core_RockSecuritySettings attribute.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Gets or sets the account protection profiles for duplicate detection to ignore.
        /// </summary>
        /// <value>
        /// The account protection profiles for duplicate detection to ignore.
        /// </value>
        /// <remarks>
        /// If the user's account protection profile is in this list then that user will be ignored by the duplicate detection algorithm.
        /// </remarks>
        public List<AccountProtectionProfile> AccountProtectionProfilesForDuplicateDetectionToIgnore { get; set; }

        /// <summary>
        /// Gets or sets the account protection profile security group.
        /// </summary>
        /// <value>
        /// The account protection profile security group.
        /// </value>
        /// <remarks>
        /// This is the list of Security groups required to be able to merge the given account protection profile.
        /// </remarks>
        public Dictionary<AccountProtectionProfile, RoleCache> AccountProtectionProfileSecurityGroup { get; set; }

        /// <summary>
        /// Gets or sets the disable tokens for account protection profiles.
        /// </summary>
        /// <value>
        /// The disable tokens for account protection profiles.
        /// </value>
        public List<AccountProtectionProfile> DisableTokensForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
        /// </summary>
        public SecuritySettings()
        {
            AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile>();
            AccountProtectionProfileSecurityGroup = new Dictionary<AccountProtectionProfile, RoleCache>();
            DisableTokensForAccountProtectionProfiles = new List<AccountProtectionProfile>();
        }
    }
}
