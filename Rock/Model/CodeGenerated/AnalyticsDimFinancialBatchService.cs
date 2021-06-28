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
    /// AnalyticsDimFinancialBatch Service class
    /// </summary>
    public partial class AnalyticsDimFinancialBatchService : Service<AnalyticsDimFinancialBatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsDimFinancialBatchService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public AnalyticsDimFinancialBatchService(RockContext context) : base(context)
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
        public bool CanDelete( AnalyticsDimFinancialBatch item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class AnalyticsDimFinancialBatchExtensionMethods
    {
        /// <summary>
        /// Clones this AnalyticsDimFinancialBatch object to a new AnalyticsDimFinancialBatch object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static AnalyticsDimFinancialBatch Clone( this AnalyticsDimFinancialBatch source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as AnalyticsDimFinancialBatch;
            }
            else
            {
                var target = new AnalyticsDimFinancialBatch();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this AnalyticsDimFinancialBatch object to a new AnalyticsDimFinancialBatch object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static AnalyticsDimFinancialBatch CloneWithoutIdentity( this AnalyticsDimFinancialBatch source )
        {
            var target = new AnalyticsDimFinancialBatch();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;

            return target;
        }

        /// <summary>
        /// Copies the properties from another AnalyticsDimFinancialBatch object to this AnalyticsDimFinancialBatch object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this AnalyticsDimFinancialBatch target, AnalyticsDimFinancialBatch source )
        {
            target.Id = source.Id;
            target.AccountingSystemCode = source.AccountingSystemCode;
            target.BatchEndDateTime = source.BatchEndDateTime;
            target.BatchId = source.BatchId;
            target.BatchStartDateTime = source.BatchStartDateTime;
            target.Campus = source.Campus;
            target.ControlAmount = source.ControlAmount;
            target.Count = source.Count;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Name = source.Name;
            target.Status = source.Status;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }

    }

}
