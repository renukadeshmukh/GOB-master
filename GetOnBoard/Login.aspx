<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="GetOnBoard.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="Styles/Login.css" rel="stylesheet" />
    <script src="Scripts/Login.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="loginbox">
        <form>
            <table>
                <tr>
                    <th>Sign In..</th>
                    <th class="right"><a href="Register.aspx">Register</a></th>
                </tr>
                <tr>
                    <td>Username</td>
                    <td><input type="text" id="txtUserName" name="txtUserName" value=""/></td>
                </tr>
                <tr>
                    <td>Password</td>
                    <td><input type="password" id="txtPassword" name="txtPassword" value=""/></td>
                </tr>
                <tr>
                    <td></td>
                    <td></td>
                </tr>
                <tr>
                    <td></td>
                    <td class="right"><input type="button" id="btnLogin" name="btnLogin" value="Login" /></td>
                </tr>
            </table>
        </form>
    </div>
</asp:Content>
