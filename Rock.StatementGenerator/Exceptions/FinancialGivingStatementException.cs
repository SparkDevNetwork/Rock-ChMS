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

namespace Rock.StatementGenerator.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class FinancialGivingStatementException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGivingStatementException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FinancialGivingStatementException( string message )
            : base( message )
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ArgumentException" />
    public class FinancialGivingStatementArgumentException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialGivingStatementArgumentException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public FinancialGivingStatementArgumentException( string message )
            : base( message )
        {

        }
    }
}
