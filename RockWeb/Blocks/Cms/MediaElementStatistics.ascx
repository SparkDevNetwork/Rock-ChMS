<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementStatistics.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementStatistics" %>

<style>
    .media-element-statistics .video-container {
        position: relative;
    }

    .media-element-statistics .video-container > .chart-container {
        position: absolute;
        width: 100%;
        height: 100%;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Label ID="lbTest" runat="server" />

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
                    <asp:HiddenField ID="hfAllTimeVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast12MonthsVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast90DaysVideoData" runat="server" Value="" />

                    <div class="row">
                        <div class="col-md-6">
                            <ul class="nav nav-tabs">
                              <li role="presentation" class="active">
                                  <a href="#<%= pnlLast90DaysDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfLast90DaysVideoData.ClientID %>" data-days-back="90">Last 90 Days</a>
                              </li>

                              <li role="presentation">
                                  <a href="#<%= pnlLast12MonthsDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfLast12MonthsVideoData.ClientID %>" data-days-back="365">Last 12 Months</a>
                              </li>

                              <li role="presentation">
                                  <a href="#<%= pnlAllTimeDetails.ClientID %>" data-toggle="tab" data-video-data="#<%= hfAllTimeVideoData.ClientID %>" data-days-back="36500">All Time</a>
                              </li>
                            </ul>

                            <div class="tab-content">
                                <asp:Panel ID="pnlLast90DaysDetails" runat="server" CssClass="tab-pane active">
                                    <asp:Literal ID="lLast90DaysContent" runat="server" />
                                </asp:Panel>

                                <asp:Panel ID="pnlLast12MonthsDetails" runat="server" CssClass="tab-pane">
                                    <asp:Literal ID="lLast12MonthsContent" runat="server" />
                                </asp:Panel>

                                <asp:Panel ID="pnlAllTimeDetails" runat="server" CssClass="tab-pane">
                                    <asp:Literal ID="lAllTimeContent" runat="server" />
                                </asp:Panel>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <asp:Panel ID="pnlChart" runat="server">
                                <div class="video-container">
                                    <div class="chart-container">
                                        <canvas id="cChart" runat="server" class="chart-canvas"></canvas>
                                    </div>
                                    <asp:Image ID="imgThumbnail" runat="server" CssClass="img-responsive" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
