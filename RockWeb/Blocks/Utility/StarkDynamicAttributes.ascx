﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDynamicAttributes.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDynamicAttributes" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-magic"></i> Blank Block that supports adding additional attributes at runtime</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <div class="alert alert-info">
                    <h4>Stark Dynamic Attributes Template Block</h4>
                    <p>This block serves as a starting point for creating new blocks that support Dynamic Attributes. After copy/pasting it and renaming the resulting file be sure to make the following changes:</p>

                    <strong>Changes to the Codebehind (ascx.cs) File</strong>
                    <ul>
                        <li>Update the namespace to match your directory</li>
                        <li>Update the class name</li>
                        <li>Fill in the DisplayName, Category and Description attributes</li>
                    </ul>

                    <strong>Changes to the Usercontrol (.ascx) File</strong>
                    <ul>
                        <li>Update the Inherits to match the namespace and class file</li>
                        <li>Remove this text... unless you really like it...</li>
                    </ul>
                </div>

                <h1>Example Lava Template Output</h1>
                <asp:Literal ID="lLavaTemplateHtml" runat="server" />

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>