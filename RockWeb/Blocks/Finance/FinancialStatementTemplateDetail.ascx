﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialStatementTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.FinancialStatementTemplateDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-invoice-dollar"></i>
                        <asp:Literal ID="lTitle" runat="server" />
                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        </div>
                    </h1>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">
                    <div id="pnlEditDetails" runat="server">
                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                        <asp:HiddenField ID="hfStatementTemplateId" runat="server" />
                        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server"
                                    SourceTypeName="Rock.Model.FinancialStatementTemplate, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server"
                                    SourceTypeName="Rock.Model.FinancialStatementTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>
                        <Rock:PanelWidget ID="pwTransactionSettings" runat="server" Title="Transaction Settings" Expanded="true">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwAccountOptions" runat="server" Label="Accounts">
                                        <Rock:RockRadioButton ID="rbAllTaxDeductibleAccounts" runat="server" GroupName="gAccountSelection" Text="All Tax Deductible Account" AutoPostBack="true" OnCheckedChanged="rbAllTaxDeductibleAccounts_CheckedChanged" />
                                        <Rock:RockRadioButton ID="rbUseCustomAccountIds" runat="server" GroupName="gAccountSelection" Text="Custom" AutoPostBack="true" OnCheckedChanged="rbAllTaxDeductibleAccounts_CheckedChanged" />
                                    </Rock:RockControlWrapper>
                                </div>

                                <div class="col-md-6">
                                    <Rock:AccountPicker ID="apTransactionAccountsCustom" runat="server" AllowMultiSelect="true" Label="Selected Accounts" Required="true" />
                                    <Rock:RockCheckBox ID="cbIncludeChildAccountsCustom" runat="server" Text="Include children of selected accounts" />
                                </div>
                            </div>
                            <Rock:DefinedValuesPicker ID="dvpCurrencyTypesCashGifts" runat="server" Label="Currency Types for Cash Gifts" RepeatColumns="5" />
                            <Rock:DefinedValuesPicker ID="dvpCurrencyTypesNonCashGifts" runat="server" Label="Currency Types for Non-Cash Gifts" RepeatColumns="5" />
                            <label>Filter Settings</label>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbHideRefundedTransactions" runat="server" Text="Hide Refunded Transactions" />
                                    <Rock:RockCheckBox ID="cbHideModifiedTransactions" runat="server" Text="Hide Transactions that are corrected on the same date. <br/> Transactions that have a matching negative amount on the same date and same account will not be shown." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DefinedValuesPicker ID="dvpTransactionType" runat="server" Label="Transaction Types" RepeatDirection="Vertical" Required="true" />
                                </div>
                            </div>
                        </Rock:PanelWidget>
                        <Rock:PanelWidget ID="pwPledgeSettings" runat="server" Title="Pledge Settings" Expanded="true">
                            <Rock:AccountPicker ID="apPledgeAccounts" runat="server" AllowMultiSelect="true" Label="Accounts for Pledges" />
                            <label>Filter Settings</label>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbIncludeGiftsToChildAccounts" runat="server" Text="Include gifts to child accounts as a part of pledge" />
                                    <Rock:RockCheckBox ID="cbIncludeNonCashGifts" runat="server" Text="Include non-cash gifts." />
                                </div>
                            </div>
                        </Rock:PanelWidget>
                        <Rock:PanelWidget ID="pwReportSettings" runat="server" Title="Report Settings" Expanded="true">
                            <Rock:CodeEditor ID="ceReportTemplate" runat="server" Label="Report Template" EditorMode="Lava" EditorHeight="200" />

                            <label>Footer Template</label>
                            <Rock:HelpBlock ID="hbFooterTemplate" runat="server" Text="The PDF Generator has these fields available. {page} {total-pages} {url} {date} {time} {html-title} {pdf-title}. To use these, put them in a lava {% raw %} block to avoid conflicts with lava merge fields." />
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:CodeEditor ID="ceFooterTemplateLeft" runat="server" Label="Left" EditorMode="Lava" EditorHeight="100" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:CodeEditor ID="ceFooterTemplateCenter" runat="server" Label="Center " EditorMode="Lava" EditorHeight="100" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:CodeEditor ID="ceFooterTemplateRight" runat="server" Label="Right" EditorMode="Lava" EditorHeight="100" />
                                </div>
                            </div>

                            <Rock:ImageUploader ID="imgTemplateLogo" runat="server" Label="Logo" />
                            <Rock:KeyValueList ID="kvlPDFObjectSettings" runat="server" Label="PDF Object Settings (Advanced)" />
                        </Rock:PanelWidget>
                        <div class="actions">
                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                        </div>
                    </div>

                    <fieldset id="fieldsetViewSummary" runat="server">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                        <p class="description">
                            <asp:Literal ID="lAccountDescription" runat="server"></asp:Literal>
                        </p>

                        <div class="row">
                            <div class="col-md-6">
                                <label>Transaction Settings</label>
                                <asp:Literal ID="lDetails" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        </div>
                    </fieldset>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
