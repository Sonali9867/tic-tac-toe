// package room

// import (
// 	"sync"

// 	"github.com/gorilla/websocket"
// 	"tictactoe-backend/internal/game"
// )

// type Room struct {
// 	RoomID  string
// 	Players map[string]*websocket.Conn // X or O → conn
// 	Game    *game.Game
// 	Mu      sync.Mutex
// }

// func NewRoom(roomID string) *Room {
// 	return &Room{
// 		RoomID:  roomID,
// 		Players: make(map[string]*websocket.Conn),
// 		Game:    game.NewGame(),
// 	}
// }

// var (
// 	waitingRoom *Room
// 	roomMutex   sync.Mutex
// )



// func CreateRoom() (*Room, string) {
// 	roomMutex.Lock()
// 	defer roomMutex.Unlock()

// 	// No waiting room → create one and wait
// 	if waitingRoom == nil {
// 		waitingRoom = NewRoom("room-temp")
// 		return waitingRoom, "waiting"
// 	}

// 	// Second player joins → matchmaking success
// 	room := waitingRoom
// 	waitingRoom = nil
// 	return room, "matched"
// }




// func (r *Room) AddPlayer(conn *websocket.Conn) (string, bool) {
// 	r.Mu.Lock()
// 	defer r.Mu.Unlock()

// 	if len(r.Players) >= 2 {
// 		return "", false
// 	}

// 	// ✅ O joins first
// 	if _, ok := r.Players["O"]; !ok {
// 		r.Players["O"] = conn
// 		return "O", true
// 	}

// 	r.Players["X"] = conn
// 	return "X", true
// }


package room

import (
	"sync"

	"github.com/gorilla/websocket"
	"tictactoe-backend/internal/game"
)

type Room struct {
	RoomID  string
	Players map[string]*websocket.Conn // X or O → conn
	Game    *game.Game
	Mu      sync.Mutex
}

func NewRoom(roomID string) *Room {
	return &Room{
		RoomID:  roomID,
		Players: make(map[string]*websocket.Conn),
		Game:    game.NewGame(),
	}
}

var (
	waitingRoom *Room
	roomMutex   sync.Mutex
)

func CreateRoom() (*Room, string) {
	roomMutex.Lock()
	defer roomMutex.Unlock()

	if waitingRoom == nil {
		waitingRoom = NewRoom("room-temp")
		return waitingRoom, "waiting"
	}

	room := waitingRoom
	waitingRoom = nil
	return room, "matched"
}

func (r *Room) AddPlayer(conn *websocket.Conn) (string, bool) {
	r.Mu.Lock()
	defer r.Mu.Unlock()

	if len(r.Players) >= 2 {
		return "", false
	}

	if _, ok := r.Players["O"]; !ok {
		r.Players["O"] = conn
		return "O", true
	}

	r.Players["X"] = conn
	return "X", true
}
