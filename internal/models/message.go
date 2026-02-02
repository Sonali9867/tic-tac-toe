package models

type ClientMessage struct {
	Type string `json:"type"`
}

type ServerMessage struct {
	Type   string `json:"type"`
	Symbol int    `json:"symbol,omitempty"`
	Turn   string `json:"turn,omitempty"`
	Winner string `json:"winner,omitempty"`
}

type MoveMessage struct {
	Type string `json:"type"`
	Cell int    `json:"cell"`
}

type MoveResponseMessage struct {
	Type     string `json:"type"`
	Cell     int    `json:"cell"`
	Valid    bool   `json:"valid"`
	Turn     string `json:"turn"`
	Winner   string `json:"winner,omitempty"`
	PlayerId string `json:"playerId,omitempty"`
	Symbol   int    `json:"symbol,omitempty"`
}

type GameStartMessage struct {
	Type     string `json:"type"`
	RoomID   string `json:"roomId"`
	Symbol   int    `json:"symbol"`
	Turn     string `json:"turn"`
	PlayerId string `json:"playerId"`
}

type GameStateMessage struct {
	Type     string   `json:"type"`
	Board    []int    `json:"board"`
	Turn     string   `json:"turn"`
	PlayerId string   `json:"playerId"`
	Winner   string   `json:"winner,omitempty"`
}
