var MovesManager = function () {
    this.MyMoves = [];
    this.GameMovesTimer = null;
    
    this.CalculatePoints = function (tile, row, col) {
        this.MyMoves.push({ Id: 0, Position: [row, col], Tile: tile });
        if (row === 8 && col === 8) {
            return 1;
        }
        var points = 0;
        var movs = gameManager.MM.MyMoves;
        var direct = inSameLine("hor", row) ? "hor" : "ver";
        var tiles = direct === "hor" ? getRowTiles("hor", row, col) : getRowTiles("ver", row, col);
        points = points + (tiles.length === 0 ? 0 : tiles.length + 1);
        var bonus = points === 6 ? 6 : 0;
        for (var i = 0; i < movs.length; i++) {
            tiles = getRowTiles((direct === "hor"? "ver":"hor"), movs[i].Position[0], movs[i].Position[1]);
            points = points + (tiles.length === 0 ? 0 : tiles.length + 1);
            if ((tiles.length + 1)===6) {
                bonus = bonus + 6;
            }
        }
        points = points + bonus;
        return points;
    };

    this.PlayMove = function () {
        if (!gameManager.GameStatus[gameManager.SelGameId].Player1.IsActive) {
            showMessage("Its not your turn!");
            return;
        }
        var points = $("#ppts").html();
        if (parseInt(points) > 0 && gameManager.MM.MyMoves && gameManager.MM.MyMoves.length > 0) {
            var moveCode = [];
            var moves = gameManager.MM.MyMoves;
            for (var i = 0; i < moves.length; i++) {
                moveCode.push(moves[i].Position[0] + "," + moves[i].Position[1] + "," + moves[i].Tile);
            }
            $("#btnPlay").hide();
            $.ajax({
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameManager.SelGameId, Points: points, MoveCode: moveCode.join("|") }),
                url: 'get/MovesService.svc/PlayMove',
                dataType: "json",
                cache: false,
                success: function(data) {
                    if (data != null && data.IsSucess && data.Move && data.Game) {
                        gameManager.GameStatus[data.Game.Id].Moves.push(data.Move);
                        gameManager.ParseAndRenderMyGames([data.Game], false);
                        if (data.Game.Status === "Finished") {
                            gameManager.ParseAndRenderMyGames([data.Game], false);
                            Session.TotalPoints = data.TotalPoints;
                            Session.Level = data.Level;
                            SaveSession();
                            gameManager.RenderUserProfile();
                        }
                    } else {
                        showMessage(data && data.ErrorMessage ? "PlayMove: " + data.ErrorMessage : "Faild to play moves! Please refresh.");
                    }
                    gameManager.RenderGameDetails();
                },
                error: function() {
                    showMessage('Unknown error please report.');
                }
            });
        } else {
            showMessage("You havnt played anything! Please place some tiles.");
        }
    };
    
    this.PassChance = function () {
        if (!gameManager.GameStatus[gameManager.SelGameId].Player1.IsActive) {
            showMessage("Its not your turn!");
            return;
        }
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameManager.SelGameId }),
            url: 'get/MovesService.svc/PassChance',
            dataType: "json",
            cache: false,
            success: function(data) {
                if (data != null && data.IsSucess && data.GameId) {
                    var gameIndex = findIndex(gameManager.MyGames, "Id", data.GameId);
                    gameManager.MyGames[gameIndex].Players[0].IsActive = false;
                    gameManager.MyGames[gameIndex].Players[1].IsActive = true;
                    gameManager.BindMyGames();
                    gameManager.RenderGameDetails();
                } else {
                    showMessage(data && data.ErrorMessage ? "PassChance: " + data.ErrorMessage : "Faild to play moves! Please refresh.");
                    gameManager.BindMyGames();
                    gameManager.RenderGameDetails();
                }
            },
            error: function() {
                showMessage('Unknown error please report.');
            }
        });
    };

    this.ResetGame = function () {
        $(".tileHolder [id*=tile_]").removeAttr("class");
        $(".tileHolder [id*=tile_]").removeAttr("style");
        gameManager.MM.ResetMyTiles();
        gameManager.MM.ResetGameMoves();
        $(".tileHolder [id*=tile_]").addClass("ui-draggable");
        $(".tileHolder [id*=tile_]").attr("style", "position: relative;");
        $("#ppts").html(0);
    };

    this.ResetMyTiles = function() {
        var tiles = gameManager.GameStatus[gameManager.SelGameId].Player1.Tiles;
        if (tiles != null) {
            for (var i = 0; i < 6; i++) {
                $("#tile_" + i).addClass(tiles[i]);
            }
        }
    };

    this.ResetGameMoves = function() {
        for (var j = 0; j < gameManager.MM.MyMoves.length; j++) {
            var move = gameManager.MM.MyMoves[j];
            $(".board #cell_" + move.Position[0] + "_" + move.Position[1]).removeClass(move.Tile);
        }
        gameManager.MM.MyMoves = [];
        gameManager.MM.RenderGameMoves();
    };

    this.RenderGameMoves = function() {
        var gameMoves = gameManager.MM.ParseGameMoves();
        for (var i = 0; i < gameMoves.length; i++) {
            if (gameMoves[i].Tile.length > 0) {
                $(".board #cell_" + gameMoves[i].Position[0] + "_" + gameMoves[i].Position[1]).addClass(gameMoves[i].Tile);
                if (gameMoves[i].IsLast) {
                    $(".board #cell_" + gameMoves[i].Position[0] + "_" + gameMoves[i].Position[1]).addClass("lastMove");
                }
            }
        }
        var moves = gameManager.GameStatus[gameManager.SelGameId].Moves;
        var lastMovePoints = 0;
        if (moves.length > 0) {
            lastMovePoints = moves[moves.length - 1].Points;
        } else {
            lastMovePoints = "NA";
        }
        $("#lastMovePoints").html(lastMovePoints===0?"Passed":lastMovePoints);
    };
    
    this.GetMyTiles = function () {
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameManager.SelGameId}),
            url: 'get/MovesService.svc/GetMyTiles',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess && data.GameId) {
                    gameManager.GameStatus[data.GameId].Tiles = data.Tiles;
                    gameManager.MM.ResetGame();
                } else {
                    showMessage(data && data.ErrorMessage ? "GetMyGames: " + data.ErrorMessage : "Faild to get your games! Unknown error.");
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    };

    this.GetNextMoves = function () {
        if (gameManager.SelGameId) {
            var index = findIndex(gameManager.MyGames, "Id", gameManager.SelGameId);
            if (gameManager.MyGames[index].Players.length < 2 || gameManager.MyGames[index].Status!="Started") {
                return;
            }
            var moves = gameManager.GameStatus[gameManager.SelGameId] && gameManager.GameStatus[gameManager.SelGameId].Moves ? gameManager.GameStatus[gameManager.SelGameId].Moves : [];
            var sendAll = moves.length == 0 ? true : false;
            $.ajax({
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ SessionId: Session.SessionId, GameId: gameManager.SelGameId, SendAll: sendAll}),
                url: 'get/MovesService.svc/GetGameMoves',
                dataType: "json",
                cache: false,
                success: function(data) {
                    if (data != null && data.IsSucess && data.Moves && data.GameId) {
                        var gameStatus = gameManager.GameStatus[data.GameId];
                        for (var i = 0; i < data.Moves.length; i++) {
                            var ix = findIndex(gameStatus.Moves, "Id", data.Moves[i].Id);
                            if (ix > -1) {
                                gameStatus.Moves[ix] = data.Moves[i];
                            } else {
                                gameStatus.Moves.push(data.Moves[i]);
                            }
                        }
                        if (data.Game) {
                            $("#btnPlay").show();
                            gameManager.ParseAndRenderMyGames([data.Game], false);
                        }
                        gameManager.RenderGameDetails();
                    } else {
                        showMessage(data && data.ErrorMessage ? "GetNextMoves: " + data.ErrorMessage : "Faild to get next moves! Please refresh");
                    }
                },
                error: function() {
                    showMessage('Unknown error please report.');
                }
            });
        }
    };

    this.DrawSuggestions = function(classList) {
        var tile = "";
        for (var k = 0; k < classList.length; k++) {
            if (classList[k].length <= 5) {
                tile = classList[k];
            }
        }
        var gameTiles = getGameTiles();
        if (gameManager.MM.MyMoves && gameManager.MM.MyMoves.length > 0) {
            gameTiles = [];
            var tiles = [];
            if (gameManager.MM.MyMoves.length == 1) {
                gameTiles = getRowTiles("hor", gameManager.MM.MyMoves[0].Position[0], gameManager.MM.MyMoves[0].Position[1], true);
                tiles = getRowTiles("ver", gameManager.MM.MyMoves[0].Position[0], gameManager.MM.MyMoves[0].Position[1], true);
            } else {
                var isHor = inSameLine("hor", gameManager.MM.MyMoves[0].Position[0]);
                tiles = isHor ? getRowTiles("hor", gameManager.MM.MyMoves[0].Position[0], gameManager.MM.MyMoves[0].Position[1], true)
                                : getRowTiles("ver", gameManager.MM.MyMoves[0].Position[0], gameManager.MM.MyMoves[0].Position[1], true);
            }
            tiles.push(gameManager.MM.MyMoves[0]);
            for (var l = 0; l < tiles.length; l++) {
                gameTiles.push(tiles[l]);
            }
        } 
        for (var i = 0; i < gameTiles.length; i++) {
            var blankTiles = getBlankTiles(gameTiles[i]);
            for (var j = 0; j < blankTiles.length; j++) {
                if (gameManager.MM.IsValidMove(tile, blankTiles[j].Row, blankTiles[j].Col, false)) {
                    $("#cell_" + blankTiles[j].Row + "_" + blankTiles[j].Col).addClass('seggestCell');
                }
            }
        }
    };

    function getBlankTiles(tilePos) {
        var blackTiles = [];
        var classList = $("#cell_" + (tilePos.Position[0] - 1) + "_" + tilePos.Position[1]).attr('class');
        if (classList && classList.split(' ').length === 1) {
            blackTiles.push({ Row: (tilePos.Position[0]-1), Col: tilePos.Position[1]});
        }
        classList = $("#cell_" + (tilePos.Position[0] +1) + "_" + tilePos.Position[1]).attr('class');
        if (classList && classList.split(' ').length === 1) {
            blackTiles.push({ Row: (tilePos.Position[0] + 1), Col: tilePos.Position[1] });
        }
        classList = $("#cell_" + tilePos.Position[0] + "_" + (tilePos.Position[1] -1)).attr('class');
        if (classList && classList.split(' ').length === 1) {
            blackTiles.push({ Row: tilePos.Position[0], Col: (tilePos.Position[1]-1) });
        }
        classList = $("#cell_" + tilePos.Position[0] + "_" + (tilePos.Position[1] + 1)).attr('class');
        if (classList && classList.split(' ').length === 1) {
            blackTiles.push({ Row: tilePos.Position[0], Col: (tilePos.Position[1]+1) });
        }
        return blackTiles;
    }
    
    function getGameTiles() {
        var gameTiles = [];
        if (gameManager.GameStatus[gameManager.SelGameId].Moves) {
            var moves = gameManager.GameStatus[gameManager.SelGameId].Moves;
            for (var i = 0; i < moves.length; i++) {
                var parts = moves[i].MoveCode.split('|');
                for (var j = 0; j < parts.length; j++) {
                    var m = parts[j].split(',');
                    gameTiles.push({Position: [parseInt(m[0]), parseInt(m[1])], Tile: m[2]});
                }
            }
        }
        return gameTiles;
    }
    
    this.ParseGameMoves = function () {
        if (gameManager.GameStatus[gameManager.SelGameId]) {
            var moves = gameManager.GameStatus[gameManager.SelGameId].Moves;
            var gameMoves = [];
            for (var i = 0; i < moves.length; i++) {
                if (moves[i].Points === 0)
                    continue;
                var parts = moves[i].MoveCode.split("|");
                for (var j = 0; j < parts.length; j++) {
                    var m = parts[j].split(",");
                    var mov = {
                        Id: moves[i].Id,
                        Position: [parseInt(m[0]), parseInt(m[1])],
                        Tile: m[2],
                        IsLast: false,
                    };
                    if (i == (moves.length - 1)) {
                        mov.IsLast = true;
                    }
                    gameMoves.push(mov);
                }
            }
            return gameMoves;
        }
        return [];
    };
    
    this.IsValidMove = function (tile, row, col, showMsg) {
        if (showMsg===undefined)
            showMsg = true;
        if (row == 8 && col == 8) {
            return true;
        }
        if (isOverTileMove(gameManager.MM.MyMoves, row, col) || isOverTileMove(gameManager.MM.ParseGameMoves(), row, col)) {
            if (showMsg)
            showMessage("Invalid move! Tile cant be placed on some other tile.");
            return false;
        }
        var horTiles = getRowTiles("hor", row, col);
        var verTiles = getRowTiles("ver", row, col);
        if (horTiles.length == 0 && verTiles.length == 0) {
            if (showMsg)
            showMessage("Invalid Move! Tile should be placed near some other tile.");
            return false;
        }
        if (sameTileExistInArr(horTiles, tile)) {
            if (showMsg)
            showMessage("Invalid move! Same tile exist in the horizontal row.");
            return false;
        }
        if (sameTileExistInArr(verTiles, tile)) {
            if (showMsg)
            showMessage("Invalid move! Same tile exist in the verticle row.");
            return false;
        }
        var isValidRow = isValidTiles(horTiles, tile);
        var isValidCol = isValidTiles(verTiles, tile);
        if (!isValidRow || !isValidCol) {
            if (showMsg)
            showMessage("Invalid move! " + (isValidCol ? "Horizontal" : "Verticle") + " tiles doesnt match.");
            return false;
        }
        var inSameRow = inSameLine("hor", row);
        var inSameCol = inSameLine("ver", col);
        if (!inSameRow && !inSameCol) {
            if (showMsg)
            showMessage("Invalid Move direction!");
            return false;
        }
        for (var i = 0; i < gameManager.MM.MyMoves.length; i++) {
            if (horTiles.indexOf(gameManager.MM.MyMoves[i].Tile) === -1 && verTiles.indexOf(gameManager.MM.MyMoves[i].Tile) === -1) {
                if (showMsg)
                showMessage("Invalid Move! Tile should be placed near other tiles you placed in this move");
                return false;
            }        
        }
        return true;
    };

    function sameTileExistInArr(arr, tile) {
        var isExist = false;
        for (var i = 0; i < arr.length; i++) {
            for (var j = 0; j < arr.length; j++) {
                if (i != j && arr[i]===arr[j]) {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
                break;
        }
        if (!isExist && arr.indexOf(tile) > -1) {
            isExist = true;
        }
        return isExist;
    }

    function isOverTileMove(arr, row, col) {
        var isOverTile = false;
        for (var i = 0; i < arr.length; i++) {
            if (arr[i].Position[0] === row && arr[i].Position[1] === col) {
                isOverTile = true;
                break;
            }
        }
        return isOverTile;
    }
    
    function inSameLine(direct, num) {
        var index = direct === "hor" ? 0 : 1;
        var inLine = true;
        for (var i = 0; i < gameManager.MM.MyMoves.length; i++) {
            if (gameManager.MM.MyMoves[i].Position[index] !== num) {
                inLine = false;
                break;
            }
        }
        return inLine;
    }

    function isValidTiles(tiles, tile) {
        var tileAtt = tile.split("_");
        var colors = getTileAttr("color", tiles);
        var symbols = getTileAttr("symbols", tiles);
        var isValidColor = true;
        for (var i = 0; i < colors.length; i++) {
            if (colors[i] != tileAtt[0]) {
                isValidColor = false;
                break;
            }
        }
        var isValidSym = true;
        for (var j = 0; j < symbols.length; j++) {
            if (symbols[j] != tileAtt[1]) {
                isValidSym = false;
                break;
            }
        }
        if (!isValidSym && !isValidColor) {
            return false;
        }
        return true;
    }

    function getTileAttr(attr, tiles) {
        var index = attr === "color" ? 0 : 1;
        var attArr = [];
        for (var i = 0; i < tiles.length; i++) {
            attArr.push(tiles[i].split("_")[index]);
        }
        return attArr;
    }
    
    function getRowTiles(direct, row, col, returnPos) {
        if (returnPos == undefined) {
            returnPos = false;
        }
        var tiles = [];
        var d = direct == "hor" ? col : row;
        for (var i = 1; i < 17; i++) {
            if (d + i >= 17)
                break;
            var cellId = direct == "hor" ? "cell_" + row + "_" + (col + i) : "cell_" + (row + i) + "_" + col;
            var t = $("#" + cellId).attr("class").split(" ");
            if (t.length > 1) {
                for (var k = 0; k < t.length; k++) {
                    if (t[k].length <= 4) {
                        if (returnPos) {
                            var mv = { Position: [], Tile: t[k] };
                            mv.Position[0] = direct == "hor" ? row : (row + i);
                            mv.Position[1] = direct == "hor" ? (col + i) : col;
                            tiles.push(mv);
                        } else
                            tiles.push(t[k]);
                    }
                }
            } else {
                break;
            }
        }
        for (var j = 1; j < 17; j++) {
            if (d - j <= -1)
                break;
            var id = direct == "hor" ? "cell_" + row + "_" + (col - j) : "cell_" + (row - j) + "_" + col;
            var t1 = $("#" + id).attr("class").split(" ");
            if (t1.length > 1) {
                for (var l = 0; l < t1.length; l++) {
                    if (t1[l].length <= 4) {
                        if (returnPos) {
                            var mv1 = { Position: [], Tile: t1[l] };
                            mv1.Position[0] = direct == "hor" ? row : (row - j);
                            mv1.Position[1] = direct == "hor" ? (col - j) : col;
                            tiles.push(mv1);
                        } else
                            tiles.push(t1[l]);
                    }
                }
            } else {
                break;
            }
        }
        return tiles;
    }

    this.PollGameMoves = function() {
        if (gameManager.SelGameId && gameManager.GameStatus && gameManager.GameStatus[gameManager.SelGameId] && gameManager.GameStatus[gameManager.SelGameId].Status != "Finished"
            && gameManager.GameStatus[gameManager.SelGameId].Player1 && gameManager.GameStatus[gameManager.SelGameId].Player1.IsActive == false) {
            gameManager.MM.GetNextMoves();
        }
    };

    this.StartMovesPolling = function() {
        if (!gameManager.MM.GameMovesTimer) {
            gameManager.MM.GameMovesTimer = setInterval("gameManager.MM.PollGameMoves();", 10000);
        }
    };
};