// Smile:
// 0001110000111000
// 0001110000111000
// 0001110000111000
// 0000000000000000
// 0000000000000000
// 0110000000000110
// 0011100000011100
// 0000111111110000

// R0 == top*32 + left/16

@SCREEN // setup start pos
D = A
@R0
M = D + M

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

(END)
	@END
	0;JMP
