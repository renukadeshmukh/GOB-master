<%@ Page Title="" Language="C#" MasterPageFile="~/Master/Site.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="GetOnBoard.Portal.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="Styles/Tiles.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="Styles/BoardLayout.css" />
    <script src="Scripts/GameManager.js"></script>
    <script src="Scripts/MovesManager.js"></script>
    <script src="Scripts/Home.js" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="actionArea left">
        <div class="boxContainer" id="accountContainer">
            
        </div>
        <div class="boxContainer">
            <table width="100%" class="box">
                <tr>
                    <th  align="left">Create new game</th>
                </tr>
                <tr class="actionButton">
                    <td align="center"  id="btnRandom">Random player</td>
                </tr>
                <%--<tr  class="actionButton">
                    <td align="center" id="btnChallenge">Challenge Friend</td>
                </tr>--%>
            </table>
        </div>
        <div class="boxContainer">
            <table width="100%" class="box">
                <tr>
                    <th width="70%" align="left">Active Games(<b id="actionGameCnt"></b>)</th>
                    <th width="30%"><a href="#" class="actionLink" id="lnkRefreshGames">Refresh</a></th>
                </tr>
            </table>
            <div id="activeGamesContainer">
                
            </div>
        </div>
        <div class="boxContainer">
            <div id="topPlayersCont">
            </div>
        </div>
        <div class="boxContainer">
            <div id="historyGamesCont">
                
            </div>
        </div>
    </div>
    <div class="playArea right">
        <div id="boardContainer">
        </div>
    </div>
    <!---------------------------Templates---------------------------->
    <div style="display: none">
        <textarea id="accountTemplate">
            <table width="100%" class="box">
                <tr>
                    <th width="50%" align="center" colspan="2">{$T.UserName}</th>
                </tr>
                <tr>
                    <td width="50%" align="center" colspan="2"><div id="progressbar">Level: {$T.Level}</div></td>
                </tr>
                <tr>
                    <td width="50%" align="center" colspan="2">Points: {$T.TotalPoints}</td>
                </tr>
            </table>
        </textarea>
        <textarea id="boardTemplate">
            <div class="actionPanel">
                <table width="100%" class="action">
                    <tr>
                        <td width="25%" align="center">{$T.Player1.UserName}(<span class="points" id="totalPoints">{$T.Player1.Points}</span>)</td>
                        <td width="55%" align="center">
                            <table class="tileHolder">
                                 <tr id="tileRow">
                                    <td align="center" class="tilesRemaining">{$T.TilesRemaining}</td>
                                    {#foreach $T.TileIds as id}
                                    <td><div id="tile_{$T.id}"></div></td>
                                    {#/for}
                                 </tr>
                            </table>
                        </td>
                        <td width="25%" align="center">{$T.Player2.UserName}(<span class="points">{$T.Player2.Points}</span>)</td>
                    </tr>
                    <tr>
                        <td width="25%"  align="center" class="smallHeader" >Current Move(<span class="points" id="ppts"></span>)</td>
                        <td width="55%" height="30px" align="center">
                            <a href="#" class="actionLink" id="lnkRefreshStatus">Refresh</a>
                            <a href="#" class="actionLink" id="btnPass">Pass</a>
                            <a href="#" class="actionLink" id="btnReset">Reset</a>
                            <a href="#" class="actionLink" id="btnPlay" {#if $T.Player1.IsActive==false || $T.Status!='Started'}style="display:none"{#/if}>Play</a>
                        </td>
                        <td width="25%" align="center" class="smallHeader">Last Move(<span class="points" id="lastMovePoints"></span>)</td>
                    </tr>
                    {#if $T.Status==='Finished'}
                    <tr>
                        <td width="80%" align="center" colspan="3">
                            <div style="margin:5px auto; width:45%">
                                {#if $T.IsWinner}
                                <span class="winner">You Won</span>
                                {#else}
                                <span class="loser">You Lost</span>
                                {#/if}
                                {#if $T.ByPoints===0}
                                    by {$T.Player2.Tiles.length-$T.Player1.Tiles.length} tiles.
                                {#else}
                                    by {$T.ByPoints} points.
                                {#/if}
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td width="80%" align="center" colspan="3">
                            <div style="width:75%; margin:0px auto">
                                {#if $T.TilesRemaining==0}
                                        {$T.TileDiff} points added because opponant havnt played all tiles.
                                {#else}
                                        {$T.TileDiff} points subtracted because you havnt played all tiles.
                                {#/if}
                            </div>
                        </td>
                    </tr>
                    {#/if}
                    {#if $T.Status==='Resigned'}
                    <tr>
                        <td width="80%" align="center" colspan="3">
                            {#if $T.Winner===$T.Player1.Id}
                                <span class="winner">You Won! Opponant resigned.</span>
                            {#else}
                                <span class="loser">You resigned!</span>
                            {#/if}
                        </td>
                    </tr>
                    {#/if}
                    {#if $T.Status==='Cancelled'}
                    <tr>
                        <td width="80%" align="center" colspan="3">
                            <span class="loser">Game deleted!</span>
                        </td>
                    </tr>
                    {#/if}
                    <tr>
                        <td width="80%" alight="center" colspan="3">
                            <div style="width:87%; margin:20px auto;">
                                <table class="board">
                                 {#foreach $T.Board.Rows as row}
                                 <tr>
                                    {#foreach $T.Board.Cols as col}
                                    <td>
                                        <div id="cell_{$T.row}_{$T.col}" style="width: 20px;  height: 20px;
                                        {#if $T.row==8 && $T.col==8}background-color:green;{#/if}" </div>
                                    </td>
                                    {#/for}
                                 </tr>
                                 {#/for}
                                </table>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
        </textarea>
        <textarea id="activeGamesTemplate">
            <table width="100%">
                {#foreach $T.UserGames as game}
                    <tr id="game_{$T.game.Id}" class="actionButton {#if $T.game.Players[0].IsActive && $T.game.Opponant!=''}activeButton{#/if}">
                        <td width="30%">{#if $T.game.Opponant==''}Searching...{#else}{$T.game.Opponant}{#/if}</td>
                        <td width="10%">
                            {#if $T.game.Status==='Started'}
                                {#if $T.game.Players[0].IsActive}Played{#else}Playing{#/if}
                            {#else}
                                {$T.game.Status}
                            {#/if}
                        </td>
                        <td width="30%">&nbsp{#if $T.game.LastActivityTime}{$T.game.LastActivityTime}{#/if}</td>
                        <td width="30%" align="center">
                            {#if $T.game.Status==="Invited" && $T.game.Players[0].IsActive}
                                <a href="#" id="lnkAccept_{$T.game.Id}" title="Accept invite" >Accept</a>
                                <a href="#" id="lnkReject_{$T.game.Id}" title="Reject invite" >Reject</a> 
                            {#else}
                                {#if $T.game.Status!='Resigned' && $T.game.Status!='Cancelled'}
                                <a href="#" id="lnkResign_{$T.game.Id}" title="Resign game" {#if $T.game.Players.length==1}style="display:none"{#/if} >[R]</a>&nbsp;&nbsp;
                                <a href="#" id="lnkDelete_{$T.game.Id}" title="Delete game" >[D]</a> 
                                {#/if}
                            {#/if}
                        </td>
                    </tr>
                {#/for}
            </table>
        </textarea>
        <textarea id="topPlayersTemplate">
            <table width="100%" class="box">
                <tr>
                    <th width="10%" align="center">#</th>
                    <th width="50%">Top players</th>
                    <th width="20%" align="center">Points</th>
                    <th width="20%" align="center">Level</th>
                </tr>
            </table>
            <table width="100%">
                {#foreach $T.TopPlayers as player}
                    <tr>
                        <td width="10%" align="center">{$T.player.Num}</td>
                        <td width="50%">{$T.player.UserName}</td>
                        <td width="20%" align="center">{$T.player.Points}</td>
                        <td width="20%" align="center">{$T.player.Level}</td>
                    </tr>
                {#/for}
            </table>
        </textarea>
        <textarea id="gameHistoryTemplate">
            <table width="100%" class="box">
                <tr>
                    <th align="left">Played Games({$T.AllGames.length})</th>
                </tr>
            </table>
            <table width="100%">
                {#foreach $T.AllGames as hGame}
                    <tr id="hgame_{$T.hGame.Id}" class="actionButton">
                        <td width="40%">You Vs {#if $T.hGame.Opponant==''}Searching...{#else}{$T.hGame.Opponant}{#/if}</td>
                        <td width="20%">
                            {#if $T.hGame.IsWinner}You Won{#else}You Lost{#/if}
                        </td>
                        <td width="30%">&nbsp{$T.hGame.LastActivityTime}</td>
                        <td width="10%" align="center">
                            <a href="#" id="lnkRematch_{$T.hGame.Players[1].Id}">Rematch</a>
                        </td>
                    </tr>
                {#/for}
            </table>
        </textarea>
    </div>
</asp:Content>
