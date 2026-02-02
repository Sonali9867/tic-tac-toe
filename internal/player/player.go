package player

import (
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
)

type Player struct {
	ID   string
	Conn *websocket.Conn
}

func NewPlayer(conn *websocket.Conn) *Player {
	return &Player{
		ID:   uuid.New().String(),
		Conn: conn,
	}
}

