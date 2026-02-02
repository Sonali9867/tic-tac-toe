package matchmaking

import (
	"sync"

	player "tictactoe-backend/internal/player"
)

var (
	waitingQueue []*player.Player
	queueMu      sync.Mutex
)

func GetWaitingQueueSize() int {
	queueMu.Lock()
	defer queueMu.Unlock()
	return len(waitingQueue)
}

func AddPlayerToQueue(p *player.Player) {
	queueMu.Lock()
	defer queueMu.Unlock()
	waitingQueue = append(waitingQueue, p)
}

func RemovePlayerFromQueue(playerID string) {
	queueMu.Lock()
	defer queueMu.Unlock()
	for i, p := range waitingQueue {
		if playerID == p.ID {
			waitingQueue = append(waitingQueue[:i], waitingQueue[i+1:]...)
			return
		}
	}
}

func TryMakeMatch() (*player.Player, *player.Player) {
	queueMu.Lock()
	defer queueMu.Unlock()
	if len(waitingQueue) < 2 {
		return nil, nil
	}
	p1 := waitingQueue[0]
	p2 := waitingQueue[1]
	waitingQueue = waitingQueue[2:]
	return p1, p2
}

