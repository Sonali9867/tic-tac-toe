// package websocket

// import (
// 	"encoding/json"
// 	"fmt"
// 	"net/http"

// 	"github.com/gorilla/websocket"
// 	"tictactoe-backend/internal/models"
// 	"tictactoe-backend/internal/room"
// )

// var upgrader = websocket.Upgrader{
// 	CheckOrigin: func(r *http.Request) bool { return true },
// }

// var gameRoom = room.NewRoom("room-1")

// func WebSocketHandler(w http.ResponseWriter, r *http.Request) {
// 	conn, err := upgrader.Upgrade(w, r, nil)
// 	if err != nil {
// 		fmt.Println("Upgrade failed:", err)
// 		return
// 	}

// 	defer conn.Close()

// 	var mySymbol string

// 	for {
// 		_, msgBytes, err := conn.ReadMessage()
// 		if err != nil {
// 			fmt.Println("Client disconnected")
// 			return
// 		}

// 		var base struct {
// 			Type string `json:"type"`
// 		}
// 		json.Unmarshal(msgBytes, &base)

// 		switch base.Type {

// 		case "join":
// 			symbol, ok := gameRoom.AddPlayer(conn)
// 			if !ok {
// 				fmt.Println("Room full")
// 				return
// 			}
// 			mySymbol = symbol
// 			fmt.Println("Player joined as", symbol)

// 			// // 🔹 Send playerId ONLY to this client
// 			// joinAck := models.GameStateMessage{
// 			// 	Type:     "join",
// 			// 	PlayerId: mySymbol,
// 			// 	Board:    gameRoom.Game.Board,
// 			// 	Turn:     gameRoom.Game.CurrentTurn,
// 			// 	Winner:   gameRoom.Game.Winner,
// 			// }
// 			// data, _ := json.Marshal(joinAck)
// 			// conn.WriteMessage(websocket.TextMessage, data)

// 			broadcastState()

// 		case "move":
// 			var moveMsg models.MoveMessage
// 			json.Unmarshal(msgBytes, &moveMsg)

// 			gameRoom.Mu.Lock()

// 			if gameRoom.Game.CurrentTurn != mySymbol {
// 				fmt.Println("Wrong turn:", mySymbol, "Expected:", gameRoom.Game.CurrentTurn)
// 				continue
// 			}

// 			gameRoom.Game.MakeMove(moveMsg.CellIndex)
// 			fmt.Println("Move accepted from", mySymbol, "at", moveMsg.CellIndex)
// 			defer gameRoom.Mu.Unlock()
// 			broadcastState()

// 		}
// 	}
// }

// func broadcastState() {
// 	state := models.GameStateMessage{
// 		Type:   "state",
// 		Board:  gameRoom.Game.Board,
// 		Turn:   gameRoom.Game.CurrentTurn,
// 		Winner: gameRoom.Game.Winner,
// 	}

// 	data, _ := json.Marshal(state)

// 	for _, conn := range gameRoom.Players {
// 		conn.WriteMessage(websocket.TextMessage, data)
// 	}
// }






// package websocket

// import (
// 	"encoding/json"
// 	"fmt"
// 	"net/http"

// 	"tictactoe-backend/internal/models"
// 	"tictactoe-backend/internal/room"

// 	"github.com/gorilla/websocket"
// )

// var upgrader = websocket.Upgrader{
// 	CheckOrigin: func(r *http.Request) bool { return true },
// }

// var myRoom *room.Room
// var mySymbol string

// func WebSocketHandler(w http.ResponseWriter, r *http.Request) {
// 	conn, err := upgrader.Upgrade(w, r, nil)
// 	if err != nil {
// 		fmt.Println("Upgrade failed:", err)
// 		return
// 	}
// 	// defer conn.Close()

// 	var mySymbol string

// 	for {
// 		_, msgBytes, err := conn.ReadMessage()
// 		if err != nil {
// 			fmt.Println("Client disconnected")
// 			conn.Close()
// 			return
// 		}

// 		var base struct {
// 			Type string `json:"type"`
// 		}
// 		json.Unmarshal(msgBytes, &base)

// 		switch base.Type {

// 		// case "join":
// 		// 	symbol, ok := gameRoom.AddPlayer(conn)
// 		// 	if !ok {
// 		// 		// send room full message
// 		// 		resp := map[string]string{
// 		// 			"type":  "error",
// 		// 			"error": "room_full",
// 		// 		}
// 		// 		data, _ := json.Marshal(resp)
// 		// 		conn.WriteMessage(websocket.TextMessage, data)
// 		// 		return
// 		// 	}

// 		// 	mySymbol = symbol
// 		// 	fmt.Println("Player joined as", symbol)

// 		// 	// ✅ SEND JOIN ACK
// 		// 	joinAck := models.GameStateMessage{
// 		// 		Type:     "join",
// 		// 		PlayerId: mySymbol,
// 		// 		Board:    gameRoom.Game.Board,
// 		// 		Turn:     gameRoom.Game.CurrentTurn,
// 		// 		Winner:   gameRoom.Game.Winner,
// 		// 	}

// 		// 	data, _ := json.Marshal(joinAck)
// 		// 	conn.WriteMessage(websocket.TextMessage, data)

// 		// 	broadcastState()

// 		case "join":

// 			r, status := room.CreateRoom()
// 			myRoom = r

// 			symbol, ok := myRoom.AddPlayer(conn)
// 			if !ok {
// 				conn.WriteJSON(map[string]string{
// 					"type": "error",
// 					"msg":  "room_full",
// 				})
// 				return
// 			}

// 			mySymbol = symbol
// 			fmt.Println("Player joined room", myRoom.RoomID, "as", mySymbol)

// 			// Send join ack
// 			conn.WriteJSON(models.GameStateMessage{
// 				Type:     "join",
// 				PlayerId: mySymbol,
// 				Board:    myRoom.Game.Board,
// 				Turn:     myRoom.Game.CurrentTurn,
// 				Winner:   myRoom.Game.Winner,
// 			})

// 			// If waiting, tell client
// 			if status == "waiting" {
// 				conn.WriteJSON(map[string]string{
// 					"type": "waiting",
// 				})
// 				continue
// 			}

// 			if status == "matched" {
// 				for _, conn := range myRoom.Players {
// 					conn.WriteJSON(map[string]string{
// 						"type": "matched",
// 					})
// 				}

// 				broadcastState(myRoom)
// 			}

// 		case "move":
// 			var moveMsg models.MoveMessage
// 			json.Unmarshal(msgBytes, &moveMsg)

// 			myRoom.Mu.Lock()

// 			if myRoom.Game.CurrentTurn != mySymbol {
// 				fmt.Println("Wrong turn:", mySymbol, "Expected:", myRoom.Game.CurrentTurn)
// 				myRoom.Mu.Unlock()
// 				continue
// 			}

// 			myRoom.Game.MakeMove(moveMsg.CellIndex)
// 			fmt.Println("Move accepted from", mySymbol, "at", moveMsg.CellIndex)

// 			myRoom.Mu.Unlock()
// 			broadcastState(myRoom)
// 		}
// 	}
// }

// // func broadcastState() {
// // 	state := models.GameStateMessage{
// // 		Type:   "state",
// // 		Board:  gameRoom.Game.Board,
// // 		Turn:   gameRoom.Game.CurrentTurn,
// // 		Winner: gameRoom.Game.Winner,
// // 	}

// // 	data, _ := json.Marshal(state)

// // 	for _, conn := range gameRoom.Players {
// // 		conn.WriteMessage(websocket.TextMessage, data)
// // 	}

// func broadcastState(r *room.Room) {
// 	state := models.GameStateMessage{
// 		Type:   "state",
// 		Board:  r.Game.Board,
// 		Turn:   r.Game.CurrentTurn,
// 		Winner: r.Game.Winner,
// 	}

// 	data, _ := json.Marshal(state)

// 	for _, conn := range r.Players {
// 		conn.WriteMessage(websocket.TextMessage, data)
// 	}
// }



















package websocket

import (
	"encoding/json"
	"fmt"
	"net/http"
	"tictactoe-backend/internal/models"
	"tictactoe-backend/internal/room"
	"github.com/gorilla/websocket"
)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool { return true },
}

func WebSocketHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println("Upgrade failed:", err)
		return
	}

	var myRoom *room.Room
	var mySymbol string

	for {
		_, msgBytes, err := conn.ReadMessage()
		if err != nil {
			fmt.Println("Client disconnected")
			conn.Close()
			return
		}

		var base struct { Type string `json:"type"` }
		json.Unmarshal(msgBytes, &base)

		switch base.Type {
		case "join":
			r, status := room.CreateRoom()
			myRoom = r

			symbol, ok := myRoom.AddPlayer(conn)
			if !ok {
				conn.WriteJSON(map[string]string{"type": "error", "msg": "room_full"})
				return
			}

			mySymbol = symbol
			fmt.Println("Player joined room", myRoom.RoomID, "as", mySymbol)

			conn.WriteJSON(models.GameStateMessage{
				Type:     "join",
				PlayerId: mySymbol,
				Board:    myRoom.Game.Board,
				Turn:     myRoom.Game.CurrentTurn,
				Winner:   myRoom.Game.Winner,
			})

			if status == "waiting" {
				conn.WriteJSON(map[string]string{"type": "waiting"})
				continue
			}

			if status == "matched" {
				myRoom.Mu.Lock()
				for _, c := range myRoom.Players {
					c.WriteJSON(map[string]string{"type": "matched"})
				}
				myRoom.Mu.Unlock()
				broadcastState(myRoom)
			}

		case "move":
			if myRoom == nil {
				fmt.Println("Move received before join")
				continue
			}

			var moveMsg models.MoveMessage
			json.Unmarshal(msgBytes, &moveMsg)

			myRoom.Mu.Lock()
			if myRoom.Game.CurrentTurn != mySymbol {
				fmt.Println("Wrong turn:", mySymbol)
				myRoom.Mu.Unlock()
				continue
			}

			myRoom.Game.MakeMove(moveMsg.Cell) // ✅ use Cell (not CellIndex)
			fmt.Println("Move accepted from", mySymbol, "at", moveMsg.Cell)
			myRoom.Mu.Unlock()

			broadcastState(myRoom)
		}
	}
}

func broadcastState(r *room.Room) {
	state := models.GameStateMessage{
		Type:   "state",
		Board:  r.Game.Board,
		Turn:   r.Game.CurrentTurn,
		Winner: r.Game.Winner,
	}

	data, _ := json.Marshal(state)

	r.Mu.Lock()
	defer r.Mu.Unlock()
	for _, conn := range r.Players {
		conn.WriteMessage(websocket.TextMessage, data)
	}
}
