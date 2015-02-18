var actManager = new ActivityManager();
var gameClient = new GameClient();
var gameManager = new GameManager();
var flashIndex = 0;
$(document).ready(function () {
    gameManager.RenderUserProfile();
    gameManager.GetMyGames();
    actManager.GetUnreadMessages();
    gameManager.GetPreviousGames();
    gameClient.Connect();
    $("#btnRandom").click(function () {
        EnableLoading = true;
        gameManager.CreateGame("Random");
    });
    
    $("#btnChallenge").click(function () {
        EnableLoading = true;
        gameManager.CreateGame("Challenge");
    });
    $("#lnkRefreshGames").click(function () {
        EnableLoading = true;
        gameManager.PullGameStatus();
    });
    $("#tabBoard").click(function () {
        $("[id*=tab]").removeClass("active");
        $("#" + this.id).addClass("active");
        gameManager.RenderGameDetails();
    });
    $("#tabChats").click(function () {
        $("[id*=tab]").removeClass("active");
        $("#" + this.id).addClass("active");
        $("#boardContainer").setTemplate($("#chatTemplate").val());
        $("#boardContainer").processTemplate({});
        gameManager.RenderChats();
    });
    gameManager.GetTopPlayers();
    setInterval("flashTitle('Boardplays')", 1000);
});

function flashTitle(pageTitle) {
    if (document.title === pageTitle) {
        var players = getWaitingPlayers();
        if (players.length > 0) {
            flashIndex = flashIndex % players.length;
            document.title = players[flashIndex] + " made move..";
            flashIndex++;
        }
    }
    else {
        document.title = pageTitle;
    }
}

function getWaitingPlayers() {
    var players = [];
    if (gameManager.MyGames && gameManager.MyGames.length > 0) {
        for (var i = 0; i < gameManager.MyGames.length; i++) {
            if (gameManager.MyGames[i].Players.length >= 2 && gameManager.MyGames[i].Players[0].IsActive) {
                players.push(gameManager.MyGames[i].Players[1].UserName);
            }
        }
    }
    return players;
}




