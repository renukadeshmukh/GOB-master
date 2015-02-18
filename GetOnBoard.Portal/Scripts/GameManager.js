var GameManager = function () {
    this.MyGames = [];
    this.AllGames = [];
    this.GameStatus = [];
    this.GameStatusTimer = null;
    this.SelGameId = null;
    this.MM = new MovesManager();
    this.CreateGame = function (method, playWith, gameId) {
        var gameCount = 0;
        for (var i = 0; i < gameManager.MyGames.length; i++) {
            if (gameManager.MyGames[i].Status === "Started" || gameManager.MyGames[i].Status === "Invited" || gameManager.MyGames[i].Status === "Waiting") {
                gameCount++;
            }
        }
        if (gameCount < Session.MaxGameLimit) {
            $.ajax({
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ SessionId: Session.SessionId, Method: method, PlayWith: playWith, GameId: gameId }),
                url: 'get/GameService.svc/StartGame',
                dataType: "json",
                cache: false,
                success: function(data) {
                    if (data != null && data.IsSucess) {
                        gameManager.ParseAndRenderMyGames([data.Game], true);
                        if (gameId) {
                            gameManager.RenderGameDetails();
                        }
                    } else {
                        showMessage(data && data.ErrorMessage ? "CreateGame:" + data.ErrorMessage : "Faild to get your games! Unknown error.");
                    }
                },
                error: function() {
                    showMessage('Unknown error please report.');
                }
            });
        } else {
            showMessage("You can play only "+Session.MaxGameLimit+" games at a time!");
        }
    };

    this.GetMyGames = function () {
        EnableLoading = true;
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({SessionId: Session.SessionId}),
            url: 'get/GameService.svc/GetMyGames',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    gameManager.MyGames = [];
                    gameManager.ParseAndRenderMyGames(data.UserGames, true);
                } else {
                    showMessage(data && data.ErrorMessage ? "GetMyGames: " + data.ErrorMessage : "Faild to get your games! Unknown error.");
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    };
    
    this.GetPreviousGames = function () {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId }),
            url: 'get/GameService.svc/GetPreviousGames',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    gameManager.AllGames = data.UserGames;
                    gameManager.RenderPreviousGames();
                } else {
                    showMessage(data && data.ErrorMessage ? "GetPreviousGames: " + data.ErrorMessage : "Faild to get your previous games! Unknown error.");
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    };

    function arrangePlayers(game) {
        if (game) {
            var index = findIndex(game.Players, "Id", Session.Id);
            if (index != 0) {
                var players = [];
                players.push(game.Players[1]);
                players.push(game.Players[0]);
                game.Players = players;
            }
        }
    }

    this.ParseAndRenderMyGames = function (games, isNew) {
        if (!games)
            return;
        for (var i = 0; i < games.length; i++) {
            var opp = "";
            if (games[i].Players) {
                for (var p = 0; p < games[i].Players.length; p++) {
                    if (games[i].Players[p].Id != Session.Id) {
                        opp = games[i].Players[p].UserName;
                        break;
                    }
                }
            }
            games[i].Opponant = opp;
            arrangePlayers(games[i]);
            var index = findIndex(gameManager.MyGames, 'Id', games[i].Id);
            if (index != -1) {
                gameManager.MyGames[index] = games[i];
            } else {
                gameManager.MyGames.push(games[i]);
            }
        }
        gameManager.BindMyGames();
    };
    
    this.RenderPreviousGames = function () {
        if (!gameManager.AllGames)
            return;
        for (var i = 0; i < gameManager.AllGames.length; i++) {
            var opp = "";
            var isWinner = true;
            if (gameManager.AllGames[i].Players) {
                for (var p = 0; p < gameManager.AllGames[i].Players.length; p++) {
                    if (gameManager.AllGames[i].Players[p].Id != Session.Id) {
                        opp = gameManager.AllGames[i].Players[p].UserName;
                        if (gameManager.AllGames[i].Players[p].Id === gameManager.AllGames[i].Winner) {
                            isWinner = false;
                        }
                        break;
                    }
                }
                gameManager.AllGames[i].IsWinner = isWinner;
            }
            gameManager.AllGames[i].Opponant = opp;
            arrangePlayers(gameManager.AllGames[i]);
        }
        $("#historyGamesCont").setTemplate($("#gameHistoryTemplate").val());
        $("#historyGamesCont").processTemplate({ AllGames: gameManager.AllGames });
        $("[id*=lnkRematch_]").click(function () {
            EnableLoading = true;
            var playerId = this.id.split('_');
            gameManager.CreateGame("PlayWith", playerId[1], '');
        });
    };
    
    this.RenderGameDetails = function() {
        var index = findIndex(gameManager.MyGames, "Id", gameManager.SelGameId);
        if (index != -1) {
            if (gameManager.MyGames[index].Players.length < 2) {
                return;
            }
            var game = gameManager.MyGames[index];
            var status = gameManager.GameStatus[gameManager.SelGameId];
            if (!status) {
                status = {
                    Board: {
                        Rows: [],
                        Cols: []
                    },
                    TileIds: [0,1,2,3,4,5],
                    Moves: [],
                    Status: game.Status,
                    IsWinner: false,
                    ByPoints: 0
                };
                for (var i = 0; i < 17; i++) {
                    status.Board.Rows.push(i);
                    status.Board.Cols.push(i);
                }
                gameManager.MM.GetNextMoves();
                gameManager.GameStatus[gameManager.SelGameId] = status;
            }
            gameManager.MM.MyMoves = [];
            status.Player1 = game.Players[0];
            status.Player2 = game.Players[1];
            status.TilesRemaining = game.Players[0].TilesRemaining;
            status.Status = game.Status;
            if (game.Status === "Resigned" || game.Status === "Cancelled") {
                status.Player1.IsActive = false;
                game.Players[0].IsActive = false;
            }
            if (game.Status === "Finished") {
                status.IsWinner = status.Player1.Points >= status.Player2.Points;
                status.TileDiff = status.TilesRemaining == 0 ? status.Player2.TilesRemaining : status.Player1.TilesRemaining;
                status.ByPoints = status.IsWinner ? status.Player1.Points - status.Player2.Points : status.Player2.Points - status.Player1.Points;
            }
            status.Winner = game.Winner;
            $("#boardContainer").setTemplate($("#boardTemplate").val());
            $("#boardContainer").processTemplate(status);
            for (var j = 0; j < status.Player1.Tiles.length; j++) {
                $("#tile_" + j).addClass(status.Player1.Tiles[j]);
            }
            gameManager.MM.RenderGameMoves();
            bindGameEvents();
        } else {
            showMessage("Selected game not found. Please refresh the page.");
        }
    };

    function bindGameEvents() {
        $(".tileHolder [id*=tile_]").draggable({
            //start: function(event, ui) {
            //    gameManager.MM.DrawSuggestions(this.classList);
            //},
            revert: function (event, ui) {
                $(".board [id*=cell_]").removeClass("seggestCell");
                $(this).data("draggable").originalPosition = {
                    top: 0,
                    left: 0
                };
                return !event;
            }
        });
        $(".board [id*=cell_]").droppable({
            hoverClass: "activeCell",
            drop: function (event, ui) {
                $(".board [id*=cell_]").removeClass("seggestCell");
                var position = this.id.split("_");
                var row = parseInt(position[1]);
                var col = parseInt(position[2]);
                var tile = ui.draggable[0].classList[0].length <= 4 ? ui.draggable[0].classList[0] : ui.draggable[0].classList[1];
                if (gameManager.MM.IsValidMove(tile, row, col)) {
                    var points = gameManager.MM.CalculatePoints(tile, row, col);
                    $("#ppts").html(points);
                    $("#" + this.id).addClass(tile);
                    $("#" + ui.draggable[0].id).removeClass(tile);
                } else {
                    var pos = ui.draggable.data("draggable").originalPosition;
                    if (pos) {
                        $("#" + ui.draggable[0].id).animate({
                            left: pos.left,
                            top: pos.top
                        }, 500, function () {
                            $("#" + ui.draggable[0].id).data("originalPosition", null);
                        });
                    }
                }
                return true;
            }
        });
        $("#btnPlay").click(function () {
            gameManager.MM.PlayMove();
        });
        $("#btnPass").click(function () {
            if (confirm("Are you sure you want to pass your turn to opponent?")) {
                gameManager.MM.PassChance();
            }
        });
        $("#btnReset").click(function () {
            gameManager.MM.ResetGame();
        });
        $("#lnkRefreshStatus").click(function () {
            EnableLoading = true;
            gameManager.GetGameStatus([gameManager.SelGameId]);
            gameManager.MM.GetNextMoves();
        });
        gameManager.MM.StartMovesPolling();
    }
    
    this.RenderUserProfile = function () {
        $("#accountContainer").setTemplate($("#accountTemplate").val());
        $("#accountContainer").processTemplate(Session);
        var levelProgress = Session.TotalPoints == 0 ? 0 : (((Session.TotalPoints % 500) / 500) * 100);
        $("#progressbar").progressbar({
            value: levelProgress
        });
    };

    this.GetTopPlayers = function () {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, Count: 5 }),
            url: 'get/GameService.svc/GetTopPlayers',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess && data.TopPlayers) {
                    for (var i = 0; i < data.TopPlayers.length; i++) {
                        data.TopPlayers[i].Num = (i + 1);
                    }
                    $("#topPlayersCont").setTemplate($("#topPlayersTemplate").val());
                    $("#topPlayersCont").processTemplate(data);
                }
            },
            error: function () {
            }
        });
    };

    this.DeleteGame = function (gameid) {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameid }),
            url: 'get/GameService.svc/DeleteGame',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    gameManager.GetGameStatus([data.GameId]);
                } else {
                    showMessage(data && data.ErrorMessage ? "DeleteGame: " + data.ErrorMessage : "Faild to delete game! Unknown error.");
                }
            },
            error: function () {
            }
        });
    };

    this.ResignGame = function (gameid) {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameid }),
            url: 'get/GameService.svc/ResignGame',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    gameManager.GetGameStatus([data.GameId]);
                } else {
                    showMessage(data && data.ErrorMessage ? "ResignGame: " + data.ErrorMessage : "Faild to resign game! Unknown error.");
                }
            },
            error: function () {
            }
        });
    };
    
    this.BindMyGames = function () {
        $("#actionGameCnt").html(gameManager.MyGames.length);
        $("#activeGamesContainer").setTemplate($("#activeGamesTemplate").val());
        $("#activeGamesContainer").processTemplate({ UserGames: gameManager.MyGames });
        if (!gameManager.GameStatusTimer) {
            gameManager.GameStatusTimer = setInterval("gameManager.PullGameStatus();", 60000);
        }
        if (!gameManager.SelGameId && gameManager.MyGames.length!=0) {
            gameManager.SelGameId = gameManager.MyGames[0].Id;
            gameManager.RenderGameDetails();
        }
        $("#activeGamesContainer #game_" + gameManager.SelGameId).addClass("activeGame");
        $("[id*=game_]").click(function () {
            if (gameManager.SelGameId) {
                $("#activeGamesContainer #game_" + gameManager.SelGameId).removeClass("activeGame");
            }
            var id = this.id.split('_');
            var gIndex = findIndex(gameManager.MyGames, "Id", id[1]);
            if (gIndex != -1) {
                if (gameManager.MyGames[gIndex].Players.length == 2) {
                    gameManager.SelGameId = id[1];
                } else {
                    showMessage("Waiting for game to start..", "success_message");
                }
            }
            $("#activeGamesContainer #game_" + gameManager.SelGameId).addClass("activeGame");
            gameManager.MM.GetNextMoves();
            gameManager.RenderGameDetails();
        });

        $("[id*=lnkResign_]").click(function () {
            if (confirm("You will lose game if you resign. Are you sure you want resign game? ")) {
                var id = this.id.split('_');
                gameManager.ResignGame(id[1]);
            }
        });
        
        $("[id*=lnkDelete_]").click(function () {
            if (confirm("Are you sure you want delete game?")) {
                var id = this.id.split('_');
                gameManager.DeleteGame(id[1]);
            }
        });
        
        $("[id*=lnkAccept_]").click(function () {
            var id = this.id.split('_');
            EnableLoading = true;
            gameManager.CreateGame("PlayWith", '', id[1]);
        });
        
        $("[id*=lnkReject_]").click(function () {
            var id = this.id.split('_');
            gameManager.DeleteGame(id[1]);
        });
    };
    
    this.PullGameStatus = function () {
        if (gameManager.MyGames) {
            var gameIds = [];
            for (var i = 0; i < gameManager.MyGames.length; i++) {
                if (gameManager.MyGames[i].Status === "Started" || gameManager.MyGames[i].Status === "Waiting" || gameManager.MyGames[i].Status === "Invited") {
                    var gameStatus = gameManager.GameStatus[gameManager.MyGames[i].Id];
                    if (gameStatus == null || !gameStatus.Player1.IsActive || gameStatus.Status === "Waiting" || gameStatus.Status === "Invited")
                        gameIds.push(gameManager.MyGames[i].Id);
                }
            }
            gameManager.GetGameStatus(gameIds);
        }
    };

    this.GetGameStatus = function (gameIds) {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, GameIds: gameIds }),
            url: 'get/GameService.svc/GetGameStatus',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    for (var i = 0; i < data.Games.length; i++) {
                        if (data.Games[i].Status === "Resigned" && Session.Id === data.Games[i].Winner) {
                            var player = data.Games[i].Players[0].Id === Session.Id ? data.Games[i].Players[0] : data.Games[i].Players[1];
                            Session.TotalPoints = Session.TotalPoints + player.Points;
                            SaveSession();
                            gameManager.RenderUserProfile();
                        }
                    }
                    gameManager.ParseAndRenderMyGames(data.Games, false);
                }
            },
            error: function () {
            }
        });
    };
};