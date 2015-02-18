<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Site.Master" AutoEventWireup="true" CodeBehind="HowToPlay.aspx.cs" Inherits="GetOnBoard.Portal.HowToPlay" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="width: 60%; margin: 10px">
        <table>
            <h3>How to play?</h3>
            <tbody>
                <tr>
                    <td colspan="2">This is the turn based game played by two players.</td>
                </tr>
                <tr>
                    <td colspan="2"><b>How to choose other player?</b></td>
                </tr>
                <tr>
                    <td colspan="2">
                        <ol>
                            <li>
                                Random Player
                                <ul>
                                    <li>
                                        When you choose this option we search for an available player.
                                    </li>
                                    <li>
                                        If a player is available then we assign you to the game created by him and you can start playing with him.
                                    </li>
                                    <li>
                                        If no player is available then we create a game in the waiting state. This means that you shall wait till a player becomes available to join your game. You will be notified once a player joins to your game.
                                    </li>
                                </ul>
                            </li>
                        </ol>
                    </td>
                </tr>
                <tr>
                    <td colspan="2"><b>Game Rules:</b></td>
                </tr>
                <tr>
                    <td colspan="2">1. This game is played with tiles that have one of 6 colors and one of 6 numbers(1 to 6).</td>
                </tr>
                <tr>
                    <td colspan="2">2. Each player gets 36 tiles.</td>
                </tr>
                <tr>
                    <td colspan="2">3. At the start of the game 6 random tiles are given to each player.</td>
                </tr>
                <tr>
                    <td colspan="2">4. The player can place a maximum of 6 tiles on the board in each turn. The player who joins in an existing game plays the first move.
                    </td>
                </tr>
                <tr>
                    <td colspan="2">5. The game ends if any player uses up all his tiles.</td>
                </tr>
                <tr>
                    <td colspan="2">6. The player who gets the most points wins the game.</td>
                </tr>
                <tr>
                    <td colspan="2"><b>Rules to place tiles on the board:</b></td>
                </tr>
                <tr>
                    <td colspan="2">1. You can place tiles by connecting them to the existing tiles on the board. In the first turn you can place tiles on the green placeholder.</td>
                </tr>
                <tr>
                    <td colspan="2">
                        <img src="Styles/images/TilePos_not_allowed.png"></img>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">2. Tiles can be connected with existing tiles only if they match by number or color.</td>
                </tr>
                <tr>
                    <td>
                        <img src="Styles/images/MatchColorOrNumber.png"></img>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">3. You can extend the tiles in horizontal or vertical direction only. This direction can be chosen by you.</td>
                </tr>
                <tr>
                    <td>
                        <img src="Styles/images/VerDirect.png"></img>
                        <img src="Styles/images/HorDirect.png"></img>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">4. The same tile can’t be placed more than once in a row or column.</td>
                </tr>
                <tr>
                    <td>
                        <img src="Styles/images/SameTile.png"></img>
                    </td>
                </tr>
                <tr>
                    <td colspan="2"><b>How to get points?</b></td>
                </tr>
                <tr>
                    <td colspan="2">1. You get one point for each tile placed on the board.</td>
                </tr>
                <tr>
                    <td colspan="2">2. You get one point for each tile you connect to. Example: If you place a tile that’s connected to 3 tiles then you get 3 + 1 = 4 points</td>
                </tr>
                <tr>
                    <td colspan="2">3. If you able to place all 6 colored tiles or all 6 numbered tiles in one line you get 6 bonus points.</td>
                </tr>
                <tr>
                    <td colspan="2">
                        <img src="Styles/images/Tiles_6_all_colors.png"></img>
                        <img src="Styles/images/Tiles_6_all_numbers1.png"></img>
                    </td>
                </tr>
            </tbody>
        </table>
        </div>
</asp:Content>
