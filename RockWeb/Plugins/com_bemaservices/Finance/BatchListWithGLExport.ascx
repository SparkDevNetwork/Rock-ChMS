﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchListWithGLExport.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.BatchListWithGLExport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMain" runat="server">
            <asp:LinkButton ID="lbDownload" runat="server" CssClass="hidden" CausesValidation="false" OnClick="lbDownload_Click">Download</asp:LinkButton>
        </asp:Panel>

        <asp:Panel ID="pnlExportModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdExport" runat="server" OnSaveClick="lbExportSave_Click" SaveButtonText="Export" Title="Export to GL">
                <Content>
                    <asp:UpdatePanel ID="upnlExport" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-12">

                                    <Rock:NotificationBox ID="nbAlreadyExported" runat="server" Text="You have already exported this batch to GL. Make certain that you want to re-export it before proceeding." NotificationBoxType="Warning" Visible="false" />
                                    <Rock:NotificationBox ID="nbNotClosed" runat="server" Text="The batch you are trying to export has not been closed and could be modified after you have exported to GL. You may wish to close the batch first." NotificationBoxType="Warning" Visible="false" />
                                    <Rock:DatePicker ID="dpDate" runat="server" Label="Deposit Date" Help="The date to mark the general ledger entry as deposited." Visible="False" />
                                    <Rock:RockTextBox ID="tbAccountingPeriod" runat="server" Label="Accounting Period" Help="Accounting period for this deposit." MaxLength="2" Visible="False" />
                                    <Rock:RockTextBox ID="tbJournalType" runat="server" Label="Journal Entry Type" Help="The type of journal entry for this deposit." MaxLength="12" Required="true" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBatchList" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-archive"></i>&nbsp;Batch List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfBatchFilter" runat="server">
                            <Rock:RockTextBox ID="tbBatchId" runat="server" Label="Batch Id"></Rock:RockTextBox>
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpBatchDate" runat="server" Label="Date Range" />
                            <Rock:CampusPicker ID="campCampus" runat="server" />
                            <Rock:RockDropDownList ID="ddlTransactionType" runat="server"  Label="Contains Transaction Type" />
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                            <Rock:RockTextBox ID="tbAccountingCode" runat="server" Label="Accounting Code"></Rock:RockTextBox>
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gBatchList" runat="server" RowItemText="Batch" OnRowSelected="gBatchList_Edit" AllowSorting="true" CssClass="js-grid-batch-list">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:DateField DataField="BatchStartDateTime" HeaderText="Date" SortExpression="BatchStartDateTime" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                                <Rock:RockBoundField DataField="AccountingSystemCode" HeaderText="Accounting Code" SortExpression="AccountingSystemCode" />
                                <Rock:RockBoundField DataField="TransactionCount" HeaderText="<span class='hidden-print'>Transaction Count</span><span class='visible-print-inline'>Txns</span>" HtmlEncode="false" SortExpression="TransactionCount" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                                <Rock:CurrencyField DataField="TransactionAmount" HeaderText="<span class='hidden-print'>Transaction Total</span><span class='visible-print-inline'>Txn Total</span>" HtmlEncode="false" SortExpression="TransactionAmount" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockTemplateField HeaderText="Variance" ItemStyle-HorizontalAlign="Right">
                                    <ItemTemplate>
                                        <span class='<%# (decimal)Eval("Variance") != 0 ? "label label-danger" : "" %>'><%# this.FormatValueAsCurrency((decimal)Eval("Variance")) %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="AccountSummaryHtml" HeaderText="Accounts" HtmlEncode="false" />
                                <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="Campus.Name" ColumnPriority="Desktop" />
                                <Rock:RockTemplateField HeaderText="Status" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='<%# Eval("StatusLabelClass") %>'><%# Eval("StatusText") %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false" ColumnPriority="Desktop" />
                                <Rock:DeleteField OnClick="gBatchList_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title">Total Results</h1>
                        </div>
                        <div class="panel-body">
                            <asp:Repeater ID="rptAccountSummary" runat="server">
                                <ItemTemplate>
                                    <div class='row'>
                                        <div class='col-xs-8'><%#Eval("Name")%></div>
                                        <div class='col-xs-4 text-right'><%#Eval("TotalAmount")%></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <div class='row'>
                                <div class='col-xs-8'><b>Total: </div>
                                <div class='col-xs-4 text-right'>
                                    <asp:Literal ID="lGrandTotal" runat="server" /></b>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
            <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true"></Rock:NotificationBox>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>