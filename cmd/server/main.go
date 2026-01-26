// package main

// import (
// 	"fmt"
// 	"net/http"
// 	"tictactoe-backend/internal/websocket"
// )

// func main() {
// 	http.HandleFunc("/ws", websocket.WebSocketHandler)
// 	fmt.Println("Server running on ws://localhost:9090/ws")
// 	http.ListenAndServe(":9090", nil)
// }


package main

import (
	"fmt"
	"net/http"
	"os"
	"tictactoe-backend/internal/websocket"
)

func main() {
	http.HandleFunc("/ws", websocket.WebSocketHandler)

	// Health check endpoint for Railway
	http.HandleFunc("/healthz", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		w.Write([]byte("OK"))
	})

	// Use dynamic port
	port := os.Getenv("PORT")
	if port == "" {
		port = "9090" // fallback for local testing
	}

	fmt.Println("Server running on port", port)
	err := http.ListenAndServe(":"+port, nil)
	if err != nil {
		fmt.Println("Server failed:", err)
	}
}




