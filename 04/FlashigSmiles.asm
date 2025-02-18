@lastDrawPos
M = 0
@lastPress
M = 0

(LOOP) // while 1
	@lastPress // lastPress - KBD = D
	D = M
	@KBD
	D = D - M

	// if D != 0: pressedKeyChange
	@pressedKeyChange 
	D;JNE
	(returnKeyChange)
	
	@LOOP
	0;JMP

(pressedKeyChange)
	@KBD // lastPress = KBD
	D = M
	@lastPress
	M = D

	@ClearScreen
	0;JMP
	(returnClear)

	@130 // if left
	D = A
	@lastPress
	D = D - M // 130 - lastPress
	@left
	D;JEQ

	@131 // elif up
	D = A
	@lastPress
	D = D - M // 131 - lastPress
	@up
	D;JEQ

	@132 // elif right
	D = A
	@lastPress
	D = D - M // 132 - lastPress
	@right
	D;JEQ

	@133 // elif down
	D = A
	@lastPress
	D = D - M // 133 - lastPress
	@down
	D;JEQ

	// else return
	@returnKeyChange 
	0;JMP

	(returnDirectionFork)

	@DrawSmile
	0;JMP
	(returnDraw)

	@returnKeyChange
	0;JMP

(left)
	@3978 // 160, 124
	D = A
	@R0
	M = D

	@returnDirectionFork
	0;JMP

(up)
	@2640 // 256, 82
	D = A
	@R0
	M = D

	@returnDirectionFork
	0;JMP


(right)
	@3989 // 336, 124
	D = A
	@R0
	M = D

	@returnDirectionFork
	0;JMP

(down)
	@5296 // 256, 165
	D = A
	@R0
	M = D

	@returnDirectionFork
	0;JMP


(ClearScreen)
	@lastDrawPos // setup start pos
	D = M
	@R0
	M = D
	
	@R0
	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M
	
	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A
	@R0
	M = D + M

	A = M
	M = 0
	@32
	D=A

	@returnClear
	0;JMP


(DrawSmile)
	@SCREEN // setup start pos
	D = A
	@R0
	M = D + M
	D = M
	@lastDrawPos
	M = D

	@7224 // 1 line
	D = A
	@R0
	A = M
	M = D

	@32 // move pointer down
	D = A
	@R0
	M = D + M

	@7224 // 2 line
	D = A
	@R0
	A = M
	M = D

	@32 // move pointer down
	D = A
	@R0
	M = D + M

	@7224 // 3 line
	D = A
	@R0
	A = M
	M = D

	@32 // move pointer 3 down
	D = A
	@R0
	M = D + M
	M = D + M
	M = D + M

	@24582 // 6 line
	D = A
	@R0
	A = M
	M = D

	@32 // move pointer down
	D = A
	@R0
	M = D + M

	@14364 // 7 line
	D = A
	@R0
	A = M
	M = D

	@32 // move pointer down
	D = A
	@R0
	M = D + M

	@4080 // 8 line
	D = A
	@R0
	A = M
	M = D

	@returnDraw
	0;JMP
