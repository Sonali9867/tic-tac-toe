
package game

type Game struct {

	Board []int
	

	CurrentPlayer int
	

	Winner int
	

	Size int
}


func NewGame(size, currentPlayer int) *Game {
	n := size * size
	return &Game{
		Board:         make([]int, n),
		CurrentPlayer: currentPlayer,
		Winner:        0,
		Size:          size,
	}
}


func (g *Game) MakeMove(index int) bool {
	if index < 0 || index >= len(g.Board) || g.Board[index] != 0 || g.Winner != 0 {
		return false
	}
	g.Board[index] = g.CurrentPlayer

	if g.CheckWinner(index) {
		g.Winner = g.CurrentPlayer
		return true
	}
	if g.CheckDraw() {
		return true
	}
	g.CurrentPlayer *= -1
	return true
}


func (g *Game) CheckWinner(index int) bool {
	size := g.Size
	row := index / size
	col := index % size

	// Check row
	win := true
	for c := 0; c < size; c++ {
		if g.Board[row*size+c] != g.CurrentPlayer {
			win = false
			break
		}
	}
	if win {
		return true
	}

	// Check column
	win = true
	for r := 0; r < size; r++ {
		if g.Board[r*size+col] != g.CurrentPlayer {
			win = false
			break
		}
	}
	if win {
		return true
	}

	// Main diagonal
	if row == col {
		win = true
		for i := 0; i < size; i++ {
			if g.Board[i*size+i] != g.CurrentPlayer {
				win = false
				break
			}
		}
		if win {
			return true
		}
	}

	// Anti diagonal
	if row+col == size-1 {
		win = true
		for i := range size {
			if g.Board[i*size+(size-1-i)] != g.CurrentPlayer {
				win = false
				break
			}
		}
		if win {
			return true
		}
	}

	return false
}


func (g *Game) CheckDraw() bool {
	if g.Winner != 0 {
		return false
	}
	for _, cell := range g.Board {
		if cell == 0 {
			return false
		}
	}
	return true
}

