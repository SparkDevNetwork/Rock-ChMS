﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContinualCareList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.ContinualCareList" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        
        <asp:Panel runat="server" ID="pnlInfo" Visible="false">
            <div class="panel-heading">
                <asp:Literal Text="Information" runat="server" ID="ltHeading" />
            </div>
            <div class="panel-body">
                <asp:Literal Text="" runat="server" ID="ltBody" />
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlMain" Visible="true">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-wheelchair"></i> Continual Care List</h1>
                </div>
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" DataKeyNames="Id" OnRowSelected="gReport_RowSelected">
                    <Columns>
                        <Rock:RockBoundField DataField="NursingHome" HeaderText="Nursing Home" SortExpression="NursingHome"></Rock:RockBoundField>
                        <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person.LastName" />
                        <Rock:PersonField DataField="Person.PrimaryFamily.Campus" HeaderText="Campus" SortExpression="Person.PrimaryFamily.Campus" />
                        <Rock:PersonField DataField="Person.ConnectionStatusValue" HeaderText="Connection Status" SortExpression="Person.ConnectionStatusValue" />
                        <Rock:RockBoundField DataField="Person.Age" HeaderText="Age" SortExpression="Person.Age"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Room" HeaderText="Room" SortExpression="Room"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="AdmitDate" DataFormatString="{0:MM/dd/yyyy}" HeaderText="Admit Date" SortExpression="AdmitDate"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Visits" HeaderText="Visits" SortExpression="Visits"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitor" HeaderText="Last Visitor" SortExpression="LastVisitor"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitDate" HeaderText="Last Visit Date" SortExpression="LastVisitDate"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitNotes" HeaderText="Last Visit Notes" SortExpression="LastVisitNotes"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="DischargeDate" HeaderText="Discharge Date" SortExpression="DischargeDate" Visible="false"></Rock:RockBoundField>
                        <Rock:RockTemplateField HeaderText="Status" SortExpression="Status">
                            <ItemTemplate>
                                <span class="label <%# Convert.ToString(Eval("Status"))=="Active"?"label-success":"label-default" %>"><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:BoolField DataField="Communion" HeaderText="Com." />
                        <Rock:RockTemplateField HeaderText="Actions" ItemStyle-Width="160px">
                            <ItemTemplate>
                                <a href="<%# "https://maps.google.com/?q="+Eval("Address").ToString() %>" target="_blank" class="btn btn-default"><i class="fa fa-map-o" title="View Map"></i></a>
                                <a href="<%# "/Pastoral/NursingHome/"+Eval("Workflow.Id") %>" class="btn btn-default"><i class="fa fa-pencil"></i></a>
                                <Rock:BootstrapButton id="btnReopen" runat="server" CommandArgument='<%# Eval("Workflow.Id") %>' CssClass="btn btn-warning" ToolTip="Reopen Workflow" OnCommand="btnReopen_Command" Visible='<%# Convert.ToString(Eval("Status"))!="Active" %>'><i class="fa fa-undo"></i></Rock:BootstrapButton>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
