<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementGenerator.ascx.cs" Inherits="RockWeb.Blocks.Finance.ContributionStatementGenerator" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <iframe runat="server" id="statementIframe" src="javascript: window.frameElement.getAttribute('src')" frameborder="0" border="0" cellspacing="0" />
    </ContentTemplate>
</asp:UpdatePanel>
