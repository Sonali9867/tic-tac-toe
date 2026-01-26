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
	"tictactoe-backend/internal/websocket"
)

func main() {
	// Correct path with leading slash
	http.HandleFunc("/ws", websocket.WebSocketHandler)

	fmt.Println("Server running on ws://localhost:9090/ws")

	err := http.ListenAndServe(":9090", nil)
	if err != nil {
		fmt.Println("Server failed:", err)
	}
}




