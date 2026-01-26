package models

type JoinMessage struct {
	Type string `json:"type"`
}

type MoveMessage struct {
	Type string `json:"type"`
	Cell int    `json:"cell"`  // ✅ match Unity C#
}

type GameStateMessage struct {
	Type     string   `json:"type"`
	Board    []string `json:"board"`
	Turn     string   `json:"turn"`
	PlayerId string   `json:"playerId"`
	Winner   string   `json:"winner"`
}
