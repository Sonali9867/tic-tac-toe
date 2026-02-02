package room

import (
	"fmt"
	"sync"
	"tictactoe-backend/internal/game"
	"tictactoe-backend/internal/player"

	"github.com/google/uuid"
)

var (
	rooms      = make(map[string]*Room)
	playerRoom = make(map[string]*Room)
	mu         sync.RWMutex
)

type Room struct {
	RoomID  string
	Player1 *player.Player
	Player2 *player.Player
	Game    *game.Game
	Size    int
	gameMu  sync.Mutex
}

func CreateRoomAndAddPlayers(p1, p2 *player.Player) *Room {
	roomID := uuid.New().String()
	g := game.NewGame(3, 1) // 3x3 board, player 1 starts
	room := &Room{
		RoomID:  roomID,
		Player1: p1,
		Player2: p2,
		Game:    g,
		Size:    3,
	}
	mu.Lock()
	rooms[roomID] = room
	playerRoom[p1.ID] = room
	playerRoom[p2.ID] = room
	mu.Unlock()
	fmt.Println("Room created:", roomID, "with 2 players")
	return room
}

func GetRoomByPlayerID(playerID string) *Room {
	mu.RLock()
	defer mu.RUnlock()
	return playerRoom[playerID]
}

func GetRoomByID(roomID string) *Room {
	mu.RLock()
	defer mu.RUnlock()
	return rooms[roomID]
}

func RemovePlayerFromRoom(playerID string) (*Room, *player.Player) {
	mu.Lock()
	defer mu.Unlock()
	room := playerRoom[playerID]
	if room == nil {
		return nil, nil
	}
	var otherPlayer *player.Player
	if room.Player1.ID == playerID {
		otherPlayer = room.Player2
	} else {
		otherPlayer = room.Player1
	}
	delete(playerRoom, room.Player1.ID)
	delete(playerRoom, room.Player2.ID)
	delete(rooms, room.RoomID)
	return room, otherPlayer
}

func SymbolToPlayerID(r *Room, symbol int) string {
	if symbol == 1 {
		return r.Player1.ID
	}
	return r.Player2.ID
}

func (r *Room) LockGame() {
	r.gameMu.Lock()
}

func (r *Room) UnlockGame() {
	r.gameMu.Unlock()
}
