(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.mediaElementStatistics = (function () {
        /**
         * Default configuration for the chart. Basically turn off everything
         * that we can so we get left with a bare bones chart that fits perfectly
         * inside the video thumbnail.
         */
        const baseChartConfig = {
            type: 'line',
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: false
                },
                scales: {
                    xAxes: [
                        {
                            display: false
                        }
                    ],
                    yAxes: [
                        {
                            id: 'y-axis-watched',
                            display: false,
                            ticks: {
                                min: 0,
                                max: 100
                            }
                        },
                        {
                            id: 'y-axis-rewatched',
                            display: false,
                            ticks: {
                                min: 0,
                                max: 100
                            }
                        }
                    ],
                },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            let value = tooltipItem.value;
                            let label = data.datasets[tooltipItem.datasetIndex].label;
                            let valueSuffix = data.datasets[tooltipItem.datasetIndex].valueSuffix || '';

                            // If this is the engagement dataset do some special
                            // processing to get the actual percentage value.
                            if (tooltipItem.datasetIndex === 0) {
                                label = 'Engagement'
                                value = Math.round(value / data.totalPlayCount * 1000) / 10;
                                valueSuffix = '%';
                            }

                            // If this is the rewatched dataset, we need to
                            // subtract the two values to get the number of
                            // rewatched times.
                            else if (tooltipItem.datasetIndex === 1) {
                                value = data.datasets[1].data[tooltipItem.index] - data.datasets[0].data[tooltipItem.index];
                            }

                            return label + ': ' + value + valueSuffix;
                        }
                    }
                }

            }
        };

        /**
         * Takes the data calculated in the C# code and converts it into data
         * that can be used by the chart plug in.
         * 
         * @param source The source data to be converted into ChartJS data.
         */
        const buildChartData = function (source) {
            let chartData = {
                labels: [],
                datasets: [],
                totalPlayCount: 0
            };

            // Shouldn't happen, but just in case...
            if (source.Duration === undefined) {
                chartData.datasets.push({ data: [] });
                chartData.datasets.push({ data: [] });
                return chartData;
            }

            chartData.totalPlayCount = source.PlayCount;

            // Build the labels as time offsets.
            for (var second = 0; second < source.Duration; second++) {
                let hour = Math.floor(second / 3600);
                let minute = Math.floor((second % 3600) / 60);
                let sec = second % 60;
                let time = '';

                if (hour > 0) {
                    time = hour + ':' + (minute < 10 ? '0' + minute : minute) + ':' + (sec < 10 ? '0' + sec : sec);
                }
                else {
                    time = minute + ':' + (sec < 10 ? '0' + sec : sec);
                }

                chartData.labels.push(time);
            }

            // Get the dataset for the number of individuals played each second.
            if (source.Watched !== undefined) {
                chartData.datasets.push({
                    label: 'Watched',
                    yAxisID: 'y-axis-watched',
                    data: source.Watched,
                    fill: 'origin',
                    borderColor: 'rgb(99, 179, 237)',
                    backgroundColor: 'rgba(99, 179, 237, 0.6)',
                    tension: 0,
                    pointRadius: 0,
                    pointHitRadius: 2
                });
            }
            else {
                chartData.datasets.push({ data: [] });
            }

            // Get the dataset for the total number of times each second was played.
            if (source.Rewatched !== undefined) {
                chartData.datasets.push({
                    label: 'Rewatched',
                    yAxisID: 'y-axis-rewatched',
                    data: source.Rewatched,
                    fill: '-1',
                    borderColor: 'rgb(72, 187, 120)',
                    backgroundColor: 'rgba(72, 187, 120, 0.6)',
                    tension: 0,
                    pointRadius: 0,
                    pointHitRadius: 2
                });
            }
            else {
                chartData.datasets.push({ data: [] });
            }

            return chartData;
        };

        /**
         * Updates the lower and upper limits of the chart to properly display
         * everything we need it to.
         *
         * @param chart The chart instance to be updated.
         */
        const updateChartLimits = function (chart) {
            var maxWatched = Math.max.apply(null, chart.data.datasets[1].data);

            chart.config.options.scales.yAxes[0].ticks.max = parseInt(maxWatched * 1.1);
            chart.config.options.scales.yAxes[1].ticks.max = parseInt(maxWatched * 1.1);
        };

        /**
         * Updates the chart from the data we got from the API.
         * 
         * @param chart The chart instance to be updated.
         * @param data The data to use when building the chart.
         */
        const updateChartData = function (chart, data) {
            chart.data = buildChartData(data);
            updateChartLimits(chart);
            chart.update();
        };

        /**
         * Update the chart with a new user selection. This handles automatically
         * downloading data from the API on the first call and then caching that
         * data to be used later.
         * 
         * @param chart The chart to be updated.
         * @param options The configuration options.
         * @param $hiddenField The hidden field to get and store metric data to.
         * @param daysBack The number of days back to go when requesting data from the API.
         */
        const updateChart = function (chart, options, $hiddenField, daysBack) {
            // Cheap way to determine if we updated the chart while waiting for API.
            const updateValue = ++options.updateCount;

            // If we already have data, we can just update the chart.
            if ($hiddenField.val() !== "") {
                updateChartData(chart, JSON.parse($hiddenField.val()));
                return;
            }

            // Clear the chart so the user knows something happened.
            chart.clear();

            $.get("/api/blocks/action/" + options.blockGuid + "/GetVideoMetricData?MediaElementGuid=" + options.mediaElementGuid + "&daysBack=" + daysBack)
                .then(function (data) {
                    if (data) {
                        $hiddenField.val(JSON.stringify(data));

                        // Make sure we haven't already repainted the chart.
                        if (options.updateCount === updateValue) {
                            updateChartData(chart, data);
                        }
                    }
                });
        };

        var exports = {
            /**
             * Initialize the statistics block for dynamically building charts.
             * 
             * @param options The configuration options to use.
             */
            initialize: function (options) {
                if (!options.chartId) {
                    throw 'chartId is required';
                }

                if (!options.tabContainerId) {
                    throw 'tabContainerId is required';
                }

                // Chart is hidden.
                if ($("#" + options.chartId).length === 0) {
                    return;
                }

                options.updateCount = 0;

                // Initialize the chart.
                const chart = new Chart(document.getElementById(options.chartId), baseChartConfig);

                // Set the default data, limits and then draw the chart.
                if (options.defaultDataId) {
                    updateChart(chart, options, $("#" + options.defaultDataId), options.defaultDaysBack);
                }

                // Whenever the user changes tabs, load in a new dataset.
                $('#' + options.tabContainerId + ' a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                    const videoDataSelector = $(e.target).data('video-data');

                    updateChart(chart, options, $(videoDataSelector), $(e.target).data('days-back'));
                });
            },
        };

        return exports;
    }());
}(jQuery));
