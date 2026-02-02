package main

import (
	"fmt"
	"net/http"
	"os"
	"tictactoe-backend/internal/websocket"
)

func main() {
	http.HandleFunc("/ws", websocket.WebSocketHandler)

	port := os.Getenv("PORT")
	if port == "" {
		port = "9090"
	}
	addr := ":" + port
	fmt.Println("Server running on port", port)

	err := http.ListenAndServe(addr, nil)
	if err != nil {
		fmt.Println("Server failed:", err)
	}
}




