<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>
                <asp:Literal ID="lTitle" runat="server" /></h1>
        </div>

        <div class="checkin-body">
            <div class="checkin-scroll-panel">
                <div class="scroller">
                    <%-- <asp:Literal ID="lCheckinResultsHtml" runat="server" /> --%>
                    <asp:Literal ID="lCheckinQRCodeHtml" runat="server" />

                    <asp:Panel ID="pnlAchievementSuccess" runat="server" CssClass="row" Visible="false">
                        <asp:Repeater ID="rptAchievementsSuccess" runat="server" OnItemDataBound="rptAchievementsSuccess_ItemDataBound">
                            <ItemTemplate>
                                <asp:Literal ID="lAchievementSuccessHtml" runat="server" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>

                    <asp:Panel ID="pnlCheckinResults" runat="server" Visible="true">
                        <asp:Repeater ID="rptCheckinResults" runat="server" OnItemDataBound="rptCheckinResults_ItemDataBound">
                            <ItemTemplate>
                                <div class="md-col-3">
                                    <asp:Literal ID="lCheckinResultHtml" runat="server" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <div class="checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
                <asp:LinkButton CssClass="btn btn-default" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
