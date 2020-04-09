﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntityMatching.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.TransactionEntityMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfBatchId" runat="server" />
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>&nbsp;<asp:Literal ID="lPanelTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbBlockConfigurationWarning" runat="server" NotificationBoxType="Warning" Text="Please set the Entity Type in block settings" Visible="false" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:DataViewPicker ID="dvpDataView" runat="server" Label="Dataview" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" EnhanceForLongLists="true" />
                        <Rock:RockDropDownList ID="ddlBatch" runat="server" Label="Open Batches" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" EnhanceForLongLists="true" />
                    </div>
                </div>
                <div class="grid grid-panel margin-t-md">
                    <asp:Panel ID="pnlTransactions" runat="server">
                        <table class="grid-table table table-striped">
                            <thead>
                                <asp:Literal ID="lHeaderHtml" runat="server" />
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="phTableRows" runat="server" />
                            </tbody>
                        </table>

                        <br />
                        <div class="margin-all-md">
                            <Rock:NotificationBox ID="nbSaveSuccess" runat="server" NotificationBoxType="Success" Text="Changes Saved" Visible="false" />
                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            </div>

                        </div>
                    </asp:Panel>
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSettings" runat="server">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick">
                <Content>
                    <asp:UpdatePanel ID="upnlSettings" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="ddlTransactionType" runat="server" Label="Transaction Type" Help="The Transaction Type that the transaction should be set to. Leave blank to get the original Transaction Type." />
                                    <Rock:EntityTypePicker ID="etpEntityType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                    <Rock:RockDropDownList ID="ddlDefinedTypePicker" runat="server" Visible="false" Label="Defined Type" EnhanceForLongLists="true" />
                                    <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Visible="false" Label="Group Type" EnhanceForLongLists="true" />
                                    <Rock:RockCheckBox ID="cbLimitToActiveGroups" runat="server" Visible="false" Text="Limit to Active Groups" Checked="true" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
