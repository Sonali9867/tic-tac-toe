package websocket

import (
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
	"tictactoe-backend/internal/matchmaking"
	"tictactoe-backend/internal/models"
	"tictactoe-backend/internal/player"
	"tictactoe-backend/internal/room"

	"github.com/gorilla/websocket"
)

var (
	upgrader = websocket.Upgrader{
		CheckOrigin: func(r *http.Request) bool { return true },
	}

	connToPlayer = make(map[*websocket.Conn]*player.Player)

	connMu sync.RWMutex
)

func WebSocketHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println("Upgrade failed:", err)
		return
	}

	p := player.NewPlayer(conn)
	connMu.Lock()
	connToPlayer[conn] = p
	connMu.Unlock()

	matchmaking.AddPlayerToQueue(p)
	fmt.Println("Player connected:", p.ID, "| Queue size:", matchmaking.GetWaitingQueueSize())

	player1, player2 := matchmaking.TryMakeMatch()
	if player1 != nil && player2 != nil {
		rm := room.CreateRoomAndAddPlayers(player1, player2)
		sendGameStart(rm)
	} else {
		sendWaiting(p)
	}

	defer func() {
		connMu.Lock()
		delete(connToPlayer, conn)
		connMu.Unlock()
		matchmaking.RemovePlayerFromQueue(p.ID)
		handleDisconnect(p)
		conn.Close()
	}()

	for {
		_, msgByte, err := conn.ReadMessage()
		if err != nil {
			fmt.Println("Player disconnected:", p.ID)
			break
		}
		HandleMessage(msgByte, p)
	}
}

func sendWaiting(p *player.Player) {
	msg := models.ClientMessage{Type: "waiting"}
	data, _ := json.Marshal(msg)
	p.Conn.WriteMessage(websocket.TextMessage, data)
}

func sendGameStart(rm *room.Room) {
	turnPlayerID := room.SymbolToPlayerID(rm, 1)
	msg1 := models.GameStartMessage{
		Type:     "game_start",
		RoomID:   rm.RoomID,
		Symbol:   1,
		Turn:     turnPlayerID,
		PlayerId: rm.Player1.ID,
	}
	msg2 := models.GameStartMessage{
		Type:     "game_start",
		RoomID:   rm.RoomID,
		Symbol:   -1,
		Turn:     turnPlayerID,
		PlayerId: rm.Player2.ID,
	}
	data1, _ := json.Marshal(msg1)
	data2, _ := json.Marshal(msg2)
	rm.Player1.Conn.WriteMessage(websocket.TextMessage, data1)
	rm.Player2.Conn.WriteMessage(websocket.TextMessage, data2)
}

func handleDisconnect(p *player.Player) {
	rm, otherPlayer := room.RemovePlayerFromRoom(p.ID)
	if rm != nil && otherPlayer != nil {
		msg := models.MoveResponseMessage{
			Type:     "opponent_disconnected",
			Valid:    true,
			Winner:   otherPlayer.ID,
			PlayerId: otherPlayer.ID,
		}
		data, _ := json.Marshal(msg)
		otherPlayer.Conn.WriteMessage(websocket.TextMessage, data)
	}
}

func HandleMessage(msg []byte, p *player.Player) {
	var base struct {
		Type string `json:"type"`
	}
	if err := json.Unmarshal(msg, &base); err != nil {
		return
	}

	switch base.Type {
	case "move":
		var move models.MoveMessage
		if err := json.Unmarshal(msg, &move); err != nil {
			return
		}
		handleMove(p, move.Cell)

	case "cancel_matchmaking":
		handleCancelMatchmaking(p)
	}
}

func handleMove(p *player.Player, cellIndex int) {
	rm := room.GetRoomByPlayerID(p.ID)
	if rm == nil {
		sendMoveResponse(p.Conn, cellIndex, false, "", "", p.ID)
		return
	}

	playerSymbol := 1
	if rm.Player2.ID == p.ID {
		playerSymbol = -1
	}

	rm.LockGame()
	if rm.Game.CurrentPlayer != playerSymbol {
		turnPlayerID := room.SymbolToPlayerID(rm, rm.Game.CurrentPlayer)
		rm.UnlockGame()
		sendMoveResponse(p.Conn, cellIndex, false, turnPlayerID, "", p.ID)
		return
	}

	valid := rm.Game.MakeMove(cellIndex)
	turnPlayerID := room.SymbolToPlayerID(rm, rm.Game.CurrentPlayer)
	winner := ""
	if rm.Game.Winner != 0 {
		winner = room.SymbolToPlayerID(rm, rm.Game.Winner)
		turnPlayerID = ""
	} else if rm.Game.CheckDraw() {
		winner = "Draw"
		turnPlayerID = ""
	}
	rm.UnlockGame()

	if valid {
		moveSymbol := rm.Game.Board[cellIndex]
		sendMoveResponseToBoth(rm, cellIndex, true, turnPlayerID, winner, moveSymbol)
		if rm.Game.Winner != 0 || rm.Game.CheckDraw() {
			room.RemovePlayerFromRoom(rm.Player1.ID)
		}
	} else {
		sendMoveResponse(p.Conn, cellIndex, false, turnPlayerID, "", p.ID)
	}
}

func sendMoveResponse(conn *websocket.Conn, cell int, valid bool, turn, winner, playerID string) {
	msg := models.MoveResponseMessage{
		Type:     "move_response",
		Cell:     cell,
		Valid:    valid,
		Turn:     turn,
		Winner:   winner,
		PlayerId: playerID,
	}
	data, _ := json.Marshal(msg)
	conn.WriteMessage(websocket.TextMessage, data)
}

func sendMoveResponseToBoth(rm *room.Room, cell int, valid bool, turn, winner string, symbol int) {
	msg := models.MoveResponseMessage{
		Type:   "move_response",
		Cell:   cell,
		Valid:  valid,
		Turn:   turn,
		Winner: winner,
		Symbol: symbol,
	}
	data, _ := json.Marshal(msg)
	rm.Player1.Conn.WriteMessage(websocket.TextMessage, data)
	rm.Player2.Conn.WriteMessage(websocket.TextMessage, data)
}

func handleCancelMatchmaking(p *player.Player) {
	matchmaking.RemovePlayerFromQueue(p.ID)
	fmt.Println("Player cancelled matchmaking:", p.ID)

	msg := models.ClientMessage{
		Type: "matchmaking_cancelled",
	}
	data, _ := json.Marshal(msg)
	p.Conn.WriteMessage(websocket.TextMessage, data)
}
