// package game

// type Game struct {
// 	Board       []string
// 	CurrentTurn string
// 	Winner      string
// }

// func NewGame() *Game {
// 	return &Game{
// 		Board:       []string{"", "", "", "", "", "", "", "", ""},
// 		CurrentTurn: "O",
// 		Winner:      "",
// 	}
// }

// func (g *Game) MakeMove(cell int) bool {
// 	if cell < 0 || cell > 8 {
// 		return false
// 	}

// 	if g.Board[cell] != "" || g.Winner != "" {
// 		return false
// 	}

// 	g.Board[cell] = g.CurrentTurn
// 	g.checkWinner()

// 	if g.Winner == "" {
// 		g.switchTurn()
// 	}

// 	return true
// }

// func (g *Game) switchTurn() {
// 	if g.CurrentTurn == "X" {
// 		g.CurrentTurn = "O"
// 	} else {
// 		g.CurrentTurn = "X"
// 	}
// }

// func (g *Game) checkWinner() {
// 	winPatterns := [8][3]int{
// 		{0, 1, 2}, {3, 4, 5}, {6, 7, 8},
// 		{0, 3, 6}, {1, 4, 7}, {2, 5, 8},
// 		{0, 4, 8}, {2, 4, 6},
// 	}

// 	for _, p := range winPatterns {
// 		a, b, c := p[0], p[1], p[2]
// 		if g.Board[a] != "" &&
// 			g.Board[a] == g.Board[b] &&
// 			g.Board[b] == g.Board[c] {
// 			g.Winner = g.Board[a]
// 			return
// 		}
// 	}

// 	for _, cell := range g.Board {
// 		if cell == "" {
// 			return
// 		}
// 	}

// 	g.Winner = "Draw"
// }



package game

type Game struct {
	Board       []string
	CurrentTurn string
	Winner      string
}

func NewGame() *Game {
	return &Game{
		Board:       []string{"", "", "", "", "", "", "", "", ""},
		CurrentTurn: "O",
		Winner:      "",
	}
}

func (g *Game) MakeMove(cell int) bool {
	if cell < 0 || cell > 8 || g.Board[cell] != "" || g.Winner != "" {
		return false
	}

	g.Board[cell] = g.CurrentTurn
	g.checkWinner()
	if g.Winner == "" {
		g.switchTurn()
	}
	return true
}

func (g *Game) switchTurn() {
	if g.CurrentTurn == "X" {
		g.CurrentTurn = "O"
	} else {
		g.CurrentTurn = "X"
	}
}

func (g *Game) checkWinner() {
	winPatterns := [8][3]int{
		{0, 1, 2}, {3, 4, 5}, {6, 7, 8},
		{0, 3, 6}, {1, 4, 7}, {2, 5, 8},
		{0, 4, 8}, {2, 4, 6},
	}

	for _, p := range winPatterns {
		a, b, c := p[0], p[1], p[2]
		if g.Board[a] != "" && g.Board[a] == g.Board[b] && g.Board[b] == g.Board[c] {
			g.Winner = g.Board[a]
			return
		}
	}

	// Draw check
	for _, cell := range g.Board {
		if cell == "" {
			return
		}
	}
	g.Winner = "Draw"
}
