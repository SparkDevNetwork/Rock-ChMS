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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    [DisplayName( "Model Map" )]
    [Category( "Examples" )]
    [Description( "Displays details about each model classes in Rock.Model." )]
    [IntegerField( "Minutes To Cache", "Numer of whole minutes to cache the class data (since reflecting on the assembly can be time consuming).", false, 60 )]
    public partial class ModelMap : RockBlock
    {
        Dictionary<string, XElement> _docuDocMembers;
        ObjectCache cache = MemoryCache.Default;

        #region Properties

        protected string SelectedCategory { get; set; }
        protected Guid? SelectedModelId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SelectedCategory = ViewState["SelectedCategory"].ToString();
            SelectedModelId = ViewState["SelectedCategory"] as Guid?;
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptCategory.ItemCommand += rptCategory_ItemCommand;
            rptCategory.ItemCreated += rptCategory_ItemCreated;
            rptModel.ItemCommand += rptModel_ItemCommand;
            rptModel.ItemCreated += rptModel_ItemCreated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            GetData();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["SelectedCategory"] = SelectedCategory;
            ViewState["SelectedModelId"] = SelectedModelId;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCreated event of the rptCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCategory_ItemCreated( object sender, RepeaterItemEventArgs e )
        {
            var lbCategory = e.Item.FindControl( "lbCategory" ) as LinkButton;
            if ( lbCategory != null )
            {
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbCategory );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCategory control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCategory_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string category = e.CommandArgument.ToString();
            SelectedCategory = category;
            SelectedModelId=null;
            GetData();
        }

        /// <summary>
        /// Handles the ItemCreated event of the rptModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptModel_ItemCreated( object sender, RepeaterItemEventArgs e )
        {
            var lbModel = e.Item.FindControl( "lbModel" ) as LinkButton;
            if ( lbModel != null )
            {
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbModel );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptModel control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptModel_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            Guid guidModel = e.CommandArgument.ToString().AsGuid();
            SelectedModelId = guidModel;
            GetData();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            var sb = new StringBuilder();

            List<MClass> allClasses = GetModelClasses() as List<MClass>;
            if ( allClasses != null )
            {
                var qry = allClasses.GroupBy( a => a.Category.Name )
                    .OrderBy( a => a.Key );

                if ( string.IsNullOrWhiteSpace( SelectedCategory ) )
                {
                    SelectedCategory = qry.Select( a => a.Key ).First();
                }

                var displayCategoryList = qry.
                                    Select( a => new
                                    {
                                        Category = new Category
                                        {
                                            Name = a.Key,
                                            IconCssClass = a.Select( g => g.Name ).First(),
                                        },
                                        Count = a.Count(),
                                        Class = ( !string.IsNullOrWhiteSpace( SelectedCategory ) && SelectedCategory == a.Key ) ? "active" : ""
                                    } );

                rptCategory.DataSource = displayCategoryList.ToList();
                rptCategory.DataBind();

                var modelQry = allClasses
                                .Where( a => a.Category.Name == SelectedCategory )
                                .OrderBy( a => a.Name );

                if ( !SelectedModelId.HasValue )
                {
                    SelectedModelId = modelQry.Select( a => a.Guid ).First().AsGuidOrNull();
                }

                var displayModel = modelQry
                                        .Select( a => new
                                        {
                                            GuidId = a.Guid,
                                            Name = a.Name,
                                            Class = ( SelectedModelId.HasValue && SelectedModelId == a.Guid.AsGuidOrNull() ) ? "active" : ""
                                        } );

                rptModel.DataSource = displayModel.ToList();
                rptModel.DataBind();

                var model = modelQry
                            .Where( a => a.Guid == SelectedModelId.ToString() )
                            .First();
                sb.Append( ClassNode( model ) );

            }
            else
            {
                sb.AppendLine( "Error reading classes from assembly." );
            }
            lClasses.Text = sb.ToString();
        }

        /// <summary>
        /// Build a "node" of the class/model with its properties and methods inside.
        /// </summary>
        /// <param name="aClass"></param>
        /// <param name="expandedClassNameGuidList"></param>
        /// <returns></returns>
        private string ClassNode( MClass aClass )
        {
            var sb = new StringBuilder();

            var name = HttpUtility.HtmlEncode( aClass.Name );
            sb.AppendFormat(
                "<div class='panel panel-default' data-id='{0}'><div class='panel-heading'><h1 class='panel-title rollover-container'><strong>{1}</strong></h1><p class='description'>{2}</p></div>",
                aClass.Guid,
                name,
                aClass.Comment.Summary
                );

            if ( aClass.Properties.Any() || aClass.Methods.Any() )
            {
                sb.AppendFormat( "<div class='panel-body'>" );

                if ( aClass.Properties.Any() )
                {
                    sb.AppendLine( "<small class='pull-right js-model-inherited'>Show: <i class='js-model-check fa fa-fw fa-square-o'></i> inherited</small><h2>Properties</h2><ul>" );
                    foreach ( var property in aClass.Properties.OrderBy( p => p.Name ) )
                    {
                        //  data-expanded='false' data-model='Block' data-id='b{0}'
                        sb.AppendFormat( "<li data-id='p{0}' class='{6}'><strong><tt>{1}</tt></strong>{3}{4}{5}{2}{7}</li>{8}",
                            property.Id,
                            HttpUtility.HtmlEncode( property.Name ),
                            ( property.Comment != null && !string.IsNullOrWhiteSpace( property.Comment.Summary ) ) ? " - " + property.Comment.Summary : "",
                            property.Required ? " <strong class='text-danger'>*</strong> " : string.Empty,
                            property.IsLavaInclude ? " <small><span class='tip tip-lava'></span></small> " : string.Empty,
                            property.NotMapped || property.IsVirtual ? " <span class='fa-stack small'><i class='fa fa-database fa-stack-1x'></i><i class='fa fa-ban fa-stack-2x text-danger'></i></span> " : string.Empty,
                            property.IsInherited ? " js-model hidden " : " ",
                            property.IsInherited ? " (inherited)" : "",
                            Environment.NewLine );
                    }
                    sb.AppendLine( "</ul>" );
                }

                if ( aClass.Methods.Any() )
                {
                    sb.AppendLine( "<h2>Methods</h2><ul>" );

                    if ( aClass.Methods.Where( m => m.IsInherited == false ).Count() == 0 )
                    {
                        sb.AppendLine( "<small class='text-muted'><i>all inherited</i></small>" );
                    }

                    foreach ( var method in aClass.Methods.OrderBy( m => m.Name ) )
                    {
                        //<li data-expanded='false' data-model='Block' data-id='b{0}'><span>{1}{2}:{3}</span></li>{4}
                        sb.AppendFormat( "<li data-id='m{0}' class='{3}'><strong><tt>{1}</tt></strong> {2}{4}</li>{5}",
                            method.Id,
                            HttpUtility.HtmlEncode( method.Signature ),
                            ( method.Comment != null && !string.IsNullOrWhiteSpace( method.Comment.Summary ) ) ? " - " + method.Comment.Summary : "",
                            method.IsInherited ? " js-model hidden " : " ",
                            method.IsInherited ? " (inherited)" : "",
                            Environment.NewLine );
                    }

                    sb.AppendLine( "</ul>" );
                }

                sb.AppendLine( "</div>" );
            }

            sb.AppendLine( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Reads the model classes from the cache or fetches them and caches them for reuse.
        /// </summary>
        /// <returns></returns>
        private List<MClass> GetModelClasses()
        {
            List<MClass> list = cache.Get( "classes" ) as List<MClass>;

            if ( list == null )
            {
                list = ReadClassesFromAssembly();
                var cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes( GetAttributeValue( "MinutesToCache" ).AsInteger() );
                cache.Set( "classes", list, cacheItemPolicy );
            }
            else
            {
                try
                {
                    list = cache.Get( "classes" ) as List<MClass>;
                }
                catch ( InvalidCastException )
                {
                    list = ReadClassesFromAssembly();
                    var cacheItemPolicy = new CacheItemPolicy();
                    cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes( GetAttributeValue( "MinutesToCache" ).AsInteger() );
                    cache.Set( "classes", list, cacheItemPolicy );
                }
            }

            return (List<MClass>)list;
        }

        /// <summary>
        /// Reads the classes from assembly.
        /// </summary>
        /// <returns></returns>
        private List<MClass> ReadClassesFromAssembly()
        {
            List<MClass> classes = new List<MClass>();

            Assembly rockDll = typeof( Rock.Model.EntitySet ).Assembly;

            ReadXMLComments( rockDll );

            foreach ( Type type in rockDll.GetTypes().OrderBy( t => t.Name ).ToArray() )
            {
                if ( type.FullName.StartsWith( "Rock.Model" ) )
                {
                    if ( type.InheritsOrImplements( typeof( Rock.Data.Entity<> ) ) )
                    {
                        var mClass = GetPropertiesAndMethods( type );
                        classes.Add( mClass );
                    }
                }
            }
            return classes;
        }

        /// <summary>
        /// Reads the XML comments from the assembly XML file.
        /// </summary>
        /// <param name="rockDll">The rock DLL.</param>
        private void ReadXMLComments( Assembly rockDll )
        {
            string rockDllPath = rockDll.Location;

            string docuPath = rockDllPath.Substring( 0, rockDllPath.LastIndexOf( "." ) ) + ".XML";

            if ( !File.Exists( docuPath ) )
            {
                docuPath = HttpContext.Current.Server.MapPath( "~" ) + @"bin\Rock.XML";
            }

            if ( File.Exists( docuPath ) )
            {
                var _docuDoc = XDocument.Load( docuPath );
                _docuDocMembers = _docuDoc.Descendants( "member" ).ToDictionary( a => a.Attribute( "name" ).Value, v => v );
            }
        }

        /// <summary>
        /// Gets the properties and methods.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private MClass GetPropertiesAndMethods( Type type, bool includeInherited = true )
        {
            MClass mClass = new MClass
            {
                Name = type.Name,
                Category = new Category
                {
                    Name = type.IsDefined( typeof( RockDomainAttribute ), true ) ? type.GetCustomAttribute<RockDomainAttribute>( true ).Name : "Other",
                    IconCssClass = type.IsDefined( typeof( RockDomainAttribute ), true ) ? type.GetCustomAttribute<RockDomainAttribute>( true ).IconCssClass : "fa fa-file-o"
                },
                Guid = type.GUID.ToStringSafe(),
                Comment = GetComments( type )
            };


            PropertyInfo[] properties = type.GetProperties( BindingFlags.Public | ( includeInherited ? BindingFlags.Instance : BindingFlags.Instance | BindingFlags.DeclaredOnly ) ).Where( m => ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) ).ToArray();
            foreach ( PropertyInfo p in properties.OrderBy( i => i.Name ).ToArray() )
            {
                mClass.Properties.Add( new MProperty
                {
                    Name = p.Name,
                    IsInherited = p.DeclaringType != type,
                    IsVirtual = p.GetGetMethod() != null && p.GetGetMethod().IsVirtual,
                    IsLavaInclude = p.IsDefined( typeof( LavaIncludeAttribute) ),
                    NotMapped = p.IsDefined( typeof( NotMappedAttribute ) ),
                    Required = p.IsDefined( typeof( RequiredAttribute ) ),
                    Id = p.MetadataToken,
                    Comment = GetComments( p )
                } );
            }

            MethodInfo[] methods = type.GetMethods( BindingFlags.Public | ( includeInherited ? BindingFlags.Instance : BindingFlags.Instance | BindingFlags.DeclaredOnly ) ).Where( m => !m.IsSpecialName && ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) ).ToArray();
            foreach ( MethodInfo m in methods.OrderBy( i => i.Name ).ToArray() )
            {
                // crazy, right?
                var param = string.Join( ", ", m.GetParameters().Select( pi => { var x = pi.ParameterType + " " + pi.Name; return x; } ) );

                mClass.Methods.Add( new MMethod
                {
                    Name = m.Name,
                    IsInherited = m.DeclaringType != type,
                    Id = m.MetadataToken,
                    Signature = string.Format( "{0}({1})", m.Name, param ),
                    Comment = GetComments( m )
                } );
            }

            return mClass;
        }

        /// <summary>
        /// Gets the comments from the data in the assembly's XML file for the 
        /// given member object.
        /// </summary>
        /// <param name="p">The MemberInfo instance.</param>
        /// <returns>an XmlComment object</returns>
        private XmlComment GetComments( MemberInfo p )
        {
            XmlComment xmlComment = new XmlComment();

            try
            {
                var prefix = string.Empty;

                if ( p.MemberType == MemberTypes.Property )
                {
                    prefix = "P:";
                }
                else if ( p.MemberType == MemberTypes.Method )
                {
                    prefix = "M:";
                }
                else if ( p.MemberType == MemberTypes.TypeInfo )
                {
                    prefix = "T:";
                }
                else
                {
                    return null;
                }

                string path = string.Format( "{0}{1}.{2}", prefix, ( p.DeclaringType != null ) ? p.DeclaringType.FullName : "Rock.Model", p.Name );

                var name = _docuDocMembers.ContainsKey( path ) ? _docuDocMembers[path] : null;
                if ( name != null )
                {
                    // Read the InnerXml contents of the summary Element.
                    var reader = name.Element( "summary" ).CreateReader();
                    reader.MoveToContent();
                    xmlComment.Summary = MakeSummaryHtml( reader.ReadInnerXml() );
                    xmlComment.Value = name.Element( "value" ).ValueSafe();
                    xmlComment.Remarks = name.Element( "remarks" ).ValueSafe();
                    xmlComment.Returns = name.Element( "returns" ).ValueSafe();
                }
            }
            catch { }
            return xmlComment;

        }

        /// <summary>
        /// Makes the summary HTML; converting any type/class cref (ex. <see cref="T:Rock.Model.Campus" />) 
        /// references to HTML links (ex <a href="#Campus">Campus</a>)
        /// </summary>
        /// <param name="innerXml">The inner XML.</param>
        /// <returns></returns>
        private string MakeSummaryHtml( string innerXml )
        {
            innerXml = System.Text.RegularExpressions.Regex.Replace( innerXml, @"\s+", " " );
            innerXml = System.Text.RegularExpressions.Regex.Replace( innerXml, @"<see cref=""T:(.*)\.([^.]*)"" />", "<a href=\"#$2\">$2</a>" );
            return innerXml;
        }
    }

    #endregion

    #region Helper Classes

    class MClass
    {
        public Category Category { get; set; }
        public string Name { get; set; }
        public XmlComment Comment { get; set; }
        public string Guid { get; set; }
        public List<MProperty> Properties { get; set; }
        public List<MMethod> Methods { get; set; }

        public MClass()
        {
            Properties = new List<MProperty>();
            Methods = new List<MMethod>();
        }
    }

    class MProperty
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsInherited { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsLavaInclude { get; set; }
        public bool NotMapped { get; set; }
        public bool Required { get; set; }
        public XmlComment Comment { get; set; }
    }

    class MMethod
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsInherited { get; set; }
        public string Signature { get; set; }
        public XmlComment Comment { get; set; }
    }

    class XmlComment
    {
        public string Summary { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
        public string[] Params { get; set; }
        public string Returns { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public string IconCssClass { get; set; }
    }
    # endregion

    #region Extension Methods

    public static class Extensions
    {
        public static string ValueSafe( this XElement element, string defaultValue = "" )
        {
            if ( element != null )
            {
                return element.Value;
            }

            return defaultValue;
        }

        // from @fir3rpho3nixx at http://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        public static bool InheritsOrImplements( this Type child, Type parent )
        {
            parent = ResolveGenericTypeDefinition( parent );

            var currentChild = child.IsGenericType
                                ? child.GetGenericTypeDefinition()
                                : child;

            while ( currentChild != typeof( object ) )
            {
                if ( parent == currentChild || HasAnyInterfaces( parent, currentChild ) )
                {
                    return true;
                }

                currentChild = currentChild.BaseType != null
                            && currentChild.BaseType.IsGenericType
                                ? currentChild.BaseType.GetGenericTypeDefinition()
                                : currentChild.BaseType;

                if ( currentChild == null )
                {
                    return false;
                }
            }
            return false;
        }

        private static bool HasAnyInterfaces( Type parent, Type child )
        {
            return child.GetInterfaces()
                .Any( childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                } );
        }

        private static Type ResolveGenericTypeDefinition( Type parent )
        {
            var shouldUseGenericType = true;
            if ( parent.IsGenericType && parent.GetGenericTypeDefinition() != parent )
            {
                shouldUseGenericType = false;
            }

            if ( parent.IsGenericType && shouldUseGenericType )
            {
                parent = parent.GetGenericTypeDefinition();
            }
            return parent;
        }
    }

    #endregion

}