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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Displays play statistics for a Media Element.
    /// </summary>
    [DisplayName( "Media Element Statistics" )]
    [Category( "CMS" )]
    [Description( "Displays play statistics for a Media Element." )]

    public partial class MediaElementStatistics : RockBlock
    {
        #region PageParameterKeys

        /// <summary>
        /// Parameter keys for this block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The media element identifier parameter key.
            /// </summary>
            public const string MediaElementId = "MediaElementId";
        }

        #endregion PageParameterKey

        #region Methods

        /// <summary>
        /// Called at an early stage when the system knows this block will
        /// be in the DOM at some point.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/MediaelementStatistics/mediaElementStatistics.js" );
        }

        /// <summary>
        /// Called when the block has been loaded into the DOM.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.MediaElementId ).AsInteger() );

                RegisterStartupScript();
            }

        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            var script = string.Format( @"Sys.Application.add_load(function () {{
    Rock.controls.mediaElementStatistics.initialize({{
        chartId: '{0}',
        defaultDataId: '{1}',
        tabContainerId: '{2}'
    }});
}});",
                cChart.ClientID,
                hfLast90DaysVideoData.ClientID,
                pnlViewDetails.ClientID );
            RockPage.ClientScript.RegisterStartupScript( GetType(), "startup-script", script, true );
        }

        /// <summary>
        /// Shows the details for the media element.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        private void ShowDetail( int mediaElementId )
        {
            var rockContext = new RockContext();
            var mediaElement = new MediaElementService( rockContext ).Get( mediaElementId );
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionData = new InteractionService( rockContext ).Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.InteractionComponent.EntityId == mediaElementId
                    && i.Operation == "WATCH" )
                .Select( i => new
                {
                    i.Id,
                    i.InteractionDateTime,
                    i.InteractionData
                } )
                .ToList();

            // Do some post-processing on the data to parse the watch map and
            // filter out any that had invalid watch maps.
            var interactions = interactionData
                .Select( i => new InteractionData
                {
                    InteractionDateTime = i.InteractionDateTime,
                    WatchMap = i.InteractionData.FromJsonOrNull<WatchMapData>()
                } )
                .Where( i => i.WatchMap != null && i.WatchMap.WatchedPercentage > 0 )
                .ToList();

            lPanelTitle.Text = mediaElement.Name ?? "Statistics";

            // If we have no media element or no interactions yet then put up
            // a friendly message about not having any statistic data yet.
            if ( mediaElement == null || !interactions.Any() )
            {
                nbNoData.Visible = true;
                pnlViewDetails.Visible = false;

                return;
            }

            // Put up a thumbnail to remind them what video they are viewing.
            var thumbnailSrc = mediaElement.ThumbnailData
                .OrderByDescending( t => t.Width )
                .FirstOrDefault();

            imgThumbnail.ImageUrl = thumbnailSrc?.Link ?? string.Empty;
            imgThumbnail.Visible = thumbnailSrc != null;

            // Show all the statistics.
            ShowAllTimeDetails( mediaElement, interactions );
            ShowLast12MonthsDetails( mediaElement, interactions );
            ShowLast90DaysDetails( mediaElement, interactions );
        }

        /// <summary>
        /// Gets the interaction data grouped by date.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <param name="dateSelector">The date selector used to determine the date of the interaction for grouping purposes.</param>
        /// <returns>A collection of <see cref="InteractionDataForDate"/> instances with the grouped data.</returns>
        private List<InteractionDataForDate> GetInteractionDataForDates( MediaElement mediaElement, IEnumerable<InteractionData> interactions, Func<InteractionData, DateTime> dateSelector )
        {
            // Group the interactions by date, using the provided date selector
            // function to get the date for the interaction.
            var interactionsByDate = interactions
                .Select( i => new
                {
                    Date = dateSelector( i ),
                    i.InteractionDateTime,
                    i.WatchMap
                } )
                .GroupBy( i => i.Date );

            // Convert the data into a collection of InteractionDataForDate
            // instances.
            return interactionsByDate
                .Select( i => new InteractionDataForDate
                {
                    Date = i.Key,
                    Count = i.Count(),
                    Engagement = i.Sum( a => a.WatchMap.WatchedPercentage ) / i.Count(),
                    MinutesWatched = ( int ) ( mediaElement.DurationSeconds * i.Sum( a => a.WatchMap.WatchedPercentage ) / 100 / 60 )
                } )
                .OrderBy( i => i.Date )
                .ToList();
        }

        /// <summary>
        /// Gets the standard KPI metric lava snippets for the interactions.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <returns>
        /// A collection of KPI metric snippets.
        /// </returns>
        private List<string> GetStandardKpiMetrics( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var kpiMetrics = new List<string>();

            var engagement = interactions.Sum( a => a.WatchMap.WatchedPercentage ) / interactions.Count();
            kpiMetrics.Add( GetKpiMetricLava( "blue-400", "fa fa-broadcast-tower", $"{engagement:n1}%", "Engagement", $"On average, people played {engagement:n1}% of the video." ) );

            var playCount = interactions.Count();
            var playCountText = GetFormattedNumber( playCount );
            kpiMetrics.Add( GetKpiMetricLava( "green-500", "fa fa-play-circle", playCountText, "Plays", $"This video was played {playCount:n0} times." ) );

            var minutesWatched = ( int ) ( interactions.Sum( a => a.WatchMap.WatchedPercentage ) * mediaElement.DurationSeconds / 100 / 60 );
            var minutesWatchedText = GetFormattedNumber( minutesWatched );
            kpiMetrics.Add( GetKpiMetricLava( "yellow-400", "fa fa-clock", minutesWatchedText, "Minutes Watched", $"A total of {minutesWatched:n0} minutes were watched." ) );

            return kpiMetrics;
        }

        /// <summary>
        /// Gets the standard trend chart lava snippets.
        /// </summary>
        /// <param name="interactions">The interactions.</param>
        /// <param name="chartPeriodTitle">The period title that goes above the trend chart.</param>
        /// <param name="periodTitle">The period title that goes on each element of the trend chart.</param>
        /// <returns>A collection of HTML and lava snippets to render the trend charts.</returns>
        private List<string> GetStandardTrendCharts( List<InteractionDataForDate> interactions, string chartPeriodTitle, string periodTitle )
        {
            var engagement = interactions
                .Select( i => $"[[ dataitem label:'{i.Engagement:n1}% average engagement {periodTitle} {i.Date.ToShortDateString()}' value:'{i.Engagement}']][[ enddataitem ]]" )
                .ToList();

            var playCount = interactions
                .Select( i => $"[[ dataitem label:'{i.Count} plays {periodTitle} {i.Date.ToShortDateString()}' value:'{i.Count}']][[ enddataitem ]]" )
                .ToList();

            var minutesWatched = interactions
                .Select( i => $"[[ dataitem label:'{i.MinutesWatched} minutes {periodTitle} {i.Date.ToShortDateString()}' value:'{i.MinutesWatched}']][[ enddataitem ]]" )
                .ToList();

            return new List<string>()
            {
                $"<hr><p>Engagement {chartPeriodTitle}</p>{{[trendchart]}}" + string.Join( "", engagement ) + "{[endtrendchart]}",
                $"<hr><p>Plays {chartPeriodTitle}</p>{{[trendchart]}}" + string.Join( "", playCount ) + "{[endtrendchart]}",
                $"<hr><p>Minutes {chartPeriodTitle}</p>{{[trendchart]}}" + string.Join( "", minutesWatched ) + "{[endtrendchart]}"
            };
        }

        /// <summary>
        /// Gets the video overlay data as a JSON string. This is the data that
        /// will be used to draw our chart information over top of the video.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        /// <returns>a JSON string containing the data.</returns>
        private string GetVideoDataJson( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var duration = mediaElement.DurationSeconds ?? 0;
            var totalCount = interactions.Count;

            // Construct the arrays that will hold the value at each second.
            var engagementMap = new double[duration];
            var watchedMap = new int[duration];
            var rewatchedMap = new int[duration];

            // Loop through every second of the video and build the maps.
            // Be cautious when modifying this code. A 70 minute video is
            // 4,200 seconds long, and if there are 10,000 watch maps to
            // process that means we are going to have 42 million calls to
            // GetCountAtPosition().
            for ( int second = 0; second < duration; second++ )
            {
                var counts = interactions.Select( i => i.WatchMap.GetCountAtPosition( second ) ).ToList();

                watchedMap[second] = counts.Count( a => a > 0 );
                rewatchedMap[second] = counts.Sum();
            }

            return new
            {
                PlayCount = totalCount,
                Duration = duration,
                Watched = watchedMap,
                Rewatched = rewatchedMap
            }.ToJson();
        }

        /// <summary>
        /// Shows the interaction details for all time.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowAllTimeDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var metrics = GetStandardKpiMetrics( mediaElement, interactions );

            var lava = "{[kpis]}" + string.Join( string.Empty, metrics ) + "{[endkpis]}";

            lAllTimeContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );

            hfAllTimeVideoData.Value = GetVideoDataJson( mediaElement, interactions );
        }

        /// <summary>
        /// Shows the interaction details for the last 12 months.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowLast12MonthsDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            // Filter interactions to the past 12 months.
            var past12Months = RockDateTime.Now.AddMonths( -12 ).Date;
            interactions = interactions.Where( a => a.InteractionDateTime >= past12Months ).ToList();

            // Get the standard KPI metrics that will be shown.
            var metrics = GetStandardKpiMetrics( mediaElement, interactions );

            // Filter interactions to be after the start of the next week
            // from our initial filtering. This way we get a whole week so
            // the trend chart doesn't look sad.
            var past12MonthsStartOfWeek = GetNextStartOfWeek( past12Months );
            var recentInteractions = interactions.Where( i => i.InteractionDateTime >= past12MonthsStartOfWeek );
            var interactionsPerWeek = GetInteractionDataForDates( mediaElement, recentInteractions, i => GetPreviousStartOfWeek( i.InteractionDateTime.Date ) );
            var trendCharts = GetStandardTrendCharts( interactionsPerWeek, "Per Week", "during the week of" );

            var lava = "{[kpis]}" + string.Join( string.Empty, metrics ) + "{[endkpis]}"
                + string.Join( string.Empty, trendCharts );

            lLast12MonthsContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );

            hfLast12MonthsVideoData.Value = GetVideoDataJson( mediaElement, interactions );
        }

        /// <summary>
        /// Shows the interaction details for the last 90 days details.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="interactions">The interactions.</param>
        private void ShowLast90DaysDetails( MediaElement mediaElement, List<InteractionData> interactions )
        {
            // Filter interactions to the past 90 days.
            var past90Days = RockDateTime.Now.AddDays( -90 ).Date;
            interactions = interactions.Where( a => a.InteractionDateTime >= past90Days ).ToList();

            // Get the standard KPI metrics that will be shown.
            var metrics = GetStandardKpiMetrics( mediaElement, interactions );

            // Get the interaction counts per day and the associated trend charts.
            var interactionsPerDay = GetInteractionDataForDates( mediaElement, interactions, i => i.InteractionDateTime.Date );
            var trendCharts = GetStandardTrendCharts( interactionsPerDay, "Per Day", "on" );

            var lava = "{[kpis]}" + string.Join( string.Empty, metrics ) + "{[endkpis]}"
                + string.Join( string.Empty, trendCharts );

            lLast90DaysContent.Text = lava.ResolveMergeFields( new Dictionary<string, object>() );

            hfLast90DaysVideoData.Value = GetVideoDataJson( mediaElement, interactions );
        }

        /// <summary>
        /// Gets the date of the previous start of the Rock week. If the date
        /// passed is already the first day of the week then the date is returned.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="DateTime"/> instance that represents the new date.</returns>
        private static DateTime GetPreviousStartOfWeek( DateTime date )
        {
            while ( date.DayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                date = date.AddDays( -1 );
            }

            return date.Date;
        }

        /// <summary>
        /// Gets the date of the next start of the Rock week. If the date
        /// passed is already the first day of the week then the date is returned.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="DateTime"/> instance that represents the new date.</returns>
        private static DateTime GetNextStartOfWeek( DateTime date )
        {
            while ( date.DayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                date = date.AddDays( 1 );
            }

            return date.Date;
        }

        /// <summary>
        /// Gets the formatted number in thousands and millions. That is, if
        /// number is more than 1,000 then divide by 1,000 and append "K".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string that represents the formatted number.</returns>
        private static string GetFormattedNumber( long value )
        {
            if ( value >= 1000000 )
            {
                return $"{value / 1000000f:n2}M";
            }
            else if ( value >= 1000 )
            {
                return $"{value / 1000f:n2}K";
            }
            else
            {
                return value.ToString( "n0" );
            }
        }

        /// <summary>
        /// Gets the metric HTML for the given values.
        /// </summary>
        /// <param name="styleClass">The background style class.</param>
        /// <param name="iconClass">The icon class.</param>
        /// <param name="value">The value.</param>
        /// <param name="label">The value label.</param>
        /// <param name="description">The tool tip description text.</param>
        /// <returns>A string of HTML content.</returns>
        private static string GetKpiMetricLava( string styleClass, string iconClass, string value, string label, string description )
        {
            return $"[[kpi icon:'{iconClass}' value:'{value}' label:'{label}' color:'{styleClass}' description:'{description}']][[ endkpi ]]";
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Holds the data retrieved from the database so we can pass it
        /// around to different methods.
        /// </summary>
        private class InteractionData
        {
            /// <summary>
            /// Gets or sets the interaction date time.
            /// </summary>
            /// <value>
            /// The interaction date time.
            /// </value>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the watch map.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public WatchMapData WatchMap { get; set; }
        }

        /// <summary>
        /// Used to record grouped interaction data into a single date.
        /// </summary>
        private class InteractionDataForDate
        {
            /// <summary>
            /// Gets or sets the date this data is for.
            /// </summary>
            /// <value>
            /// The date this data is for.
            /// </value>
            public DateTime Date { get; set; }

            /// <summary>
            /// Gets or sets the number of interactions for this date.
            /// </summary>
            /// <value>
            /// The number of interactions for this date.
            /// </value>
            public long Count { get; set; }

            /// <summary>
            /// Gets or sets the average engagement for this date.
            /// </summary>
            /// <value>
            /// The average engagement for this date.
            /// </value>
            public double Engagement { get; set; }

            /// <summary>
            /// Gets or sets the number of minutes watched for this date.
            /// </summary>
            /// <value>
            /// The minutes number of watched for this date.
            /// </value>
            public int MinutesWatched { get; set; }
        }

        /// <summary>
        /// Watch Map Data that has been saved into the interaction.
        /// </summary>
        private class WatchMapData
        {
            /// <summary>
            /// Gets or sets the watch map run length encoded data.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public string WatchMap
            {
                get
                {
                    return _watchMap;
                }
                set
                {
                    _watchMap = value;

                    var segments = new List<WatchSegment>();
                    var segs = _watchMap.Split( ',' );

                    foreach ( var seg in segs )
                    {
                        if ( seg.Length < 2 )
                        {
                            continue;
                        }

                        var duration = seg.Substring( 0, seg.Length - 1 ).AsInteger();
                        var count = seg.Substring( seg.Length - 1 ).AsInteger();

                        segments.Add( new WatchSegment { Duration = duration, Count = count } );
                    }

                    Segments = segments;
                }
            }
            private string _watchMap;

            /// <summary>
            /// Gets or sets the watched percentage.
            /// </summary>
            /// <value>
            /// The watched percentage.
            /// </value>
            public double WatchedPercentage { get; set; }

            /// <summary>
            /// The segments
            /// </summary>
            /// <remarks>
            /// Defined as field instead of property as it is accessed
            /// millions of times per page load.
            /// </remarks>
            public IReadOnlyList<WatchSegment> Segments;

            /// <summary>
            /// Gets the count at the specified position of the segment map.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <returns>The count or 0 if not found.</returns>
            public int GetCountAtPosition( int position )
            {
                // Walk through each segment until we find a segment that
                // "owns" the position we are interested in. Generally
                // speaking, there should probably always be less than 4 or 5
                // segments so this should be pretty fast.
                foreach ( var segment in Segments )
                {
                    // This segment owns the position we care about.
                    if ( position < segment.Duration )
                    {
                        return segment.Count;
                    }

                    // Decrement the position before moving to the next segment.
                    position -= segment.Duration;

                    if ( position < 0 )
                    {
                        break;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Individual segment of the watch map. Use fields instead of properties
        /// for performance reasons as they can be access tens of millions of
        /// times per page load.
        /// </summary>
        private class WatchSegment
        {
            /// <summary>
            /// The duration of this segment in seconds.
            /// </summary>
            public int Duration;

            /// <summary>
            /// The number of times this segment was watched.
            /// </summary>
            public int Count;
        }

        #endregion
    }
}
