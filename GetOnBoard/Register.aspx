<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="GetOnBoard.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="Styles/Login.css" rel="stylesheet" />
    <script src="Scripts/Register.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="loginbox">
        <table>
            <tr>
                <th>Register..</th>
                <th class="right"><a href="Login.aspx">Sign In</a></th>
            </tr>
            <tr>
                <td>Username</td>
                <td><input type="text" id="txtUserName" name="txtUserName"/></td>
            </tr>
            <tr>
                <td>First Name</td>
                <td><input type="text" id="txtFname" name="txtFname"/></td>
            </tr>
            <tr>
                <td>Last Name</td>
                <td><input type="text" id="txtLname" name="txtLname"/></td>
            </tr>
            <tr>
                <td>Email</td>
                <td><input type="text" id="txtEmail" name="txtEmail"/></td>
            </tr>
            <tr>
                <td>Password</td>
                <td><input type="password" id="txtPassword" name="txtPassword"/></td>
            </tr>
            <tr>
                <td></td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="right"><input type="button" id="btnRegister" name="btnRegister" value="Register" /></td>
            </tr>
        </table>
    </div>
</asp:Content>
