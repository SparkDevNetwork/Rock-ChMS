﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementStatistics.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementStatistics" %>

<style>
    .video-container {
        position: relative;
    }

    .video-container > .chart-container {
        position: absolute;
        width: 100%;
        height: 100%;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading ">
                <h1 class="panel-title">
                    <i class="fa fa-chart"></i>
                    <asp:Literal ID="lPanelTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoData" runat="server" NotificationBoxType="Info" Visible="false" Text="No statistical data is available yet." />

                <asp:Panel ID="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <ul class="nav nav-tabs">
                              <li role="presentation" class="active">
                                  <a href="#<%= pnlAllTimeDetails.ClientID %>" data-toggle="tab">All Time</a>
                              </li>

                              <li role="presentation">
                                  <a href="#<%= pnlLast12MonthsDetails.ClientID %>" data-toggle="tab">Last 12 Months</a>
                              </li>

                              <li role="presentation">
                                  <a href="#<%= pnlLast90DaysDetails.ClientID %>" data-toggle="tab">Last 90 Days</a>
                              </li>
                            </ul>

                            <div class="tab-content">
                                <asp:Panel ID="pnlAllTimeDetails" runat="server" CssClass="tab-pane active">
                                    <asp:Literal ID="lAllTimeContent" runat="server" />
                                </asp:Panel>

                                <asp:Panel ID="pnlLast12MonthsDetails" runat="server" CssClass="tab-pane">
                                    <asp:Literal ID="lLast12MonthsContent" runat="server" />
                                </asp:Panel>

                                <asp:Panel ID="pnlLast90DaysDetails" runat="server" CssClass="tab-pane">
                                    <asp:Literal ID="lLast90DaysContent" runat="server" />
                                </asp:Panel>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <asp:Panel ID="pnlChart" runat="server" CssClass="video-container">
                                <div class="chart-container">
                                    <canvas id="cChart" runat="server" class="chart-canvas"></canvas>
                                </div>
                                <asp:Image ID="imgThumbnail" runat="server" CssClass="img-responsive" />
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
    Sys.Application.add_load(function () {
        const data = {
            labels: ['Engagement', 'Engagement', 'Engagement'],
            datasets: [{
                data: [65, 83, 70],
                fill: false,
                borderColor: '#63B3ED',
                tension: 0,
                pointRadius: 0
            }]
        };

        const config = {
            type: 'line',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: false
                },
                scales: {
                    xAxes: [{
                        display: false
                    }],
                    yAxes: [{
                        display: false,
                        ticks: {
                            beginAtZero: true
                        }
                    }],
                }
            }
        };

        var myChart = new Chart(
            document.getElementById('<%= cChart.ClientID %>'),
            config
        );
    });
</script>
