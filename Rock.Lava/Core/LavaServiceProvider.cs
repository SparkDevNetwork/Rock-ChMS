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
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// A simple service container that is capable of providing instances of Lava services.
    /// </summary>
    /// <remarks>
    /// In the future, this container should be reimplemented using the Microsoft.Extensions.DependencyInjection library.
    /// </remarks>
    public class LavaServiceProvider : ILavaServiceProvider
    {
        private Dictionary<Type, Func<Type, object, ILavaService>> _services = new Dictionary<Type, Func<Type, object, ILavaService>>();
        //private Dictionary<Type, Func<Type, ILavaService>> _services = new Dictionary<Type, Func<Type, ILavaService>>();

        public void RegisterService<TService>( Func<Type, object, TService> serviceInstance )
            where TService : class, ILavaService
        {
            _services.AddOrReplace( typeof( TService ), serviceInstance );
        }

        public void RegisterService( Type serviceType, Func<Type, object, ILavaService> factoryMethod )
        {
            // Resolve the Type
            //var engineType = Type.GetType( typeName );

            _services.AddOrReplace( serviceType, factoryMethod );
        }

        // Get an instance of a Lava service component of the specified type.
        public TService GetService<TService>()
            where TService : class, ILavaService
        {
            var service = GetService( typeof( TService ) ) as TService;

            return service;
        }

        // Get an instance of a Lava service component of the specified type.
        public ILavaService GetService( Type serviceType, object configuration = null )
        {
            var factoryFunc = _services.GetValueOrNull( serviceType );

            if ( factoryFunc == null )
            {
                throw new LavaException( $"GetService failed. The service type \"{ serviceType.FullName }\" is not registered." );
            }

            var service = factoryFunc( serviceType, configuration );

            if ( service == null )
            {
                throw new LavaException( $"GetService failed. The service type \"{ serviceType.FullName }\" could not be created." );
            }

            return service;
        }
    }
}