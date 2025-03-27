@SCREEN
D = A
@pointer
M = D

@DRAWROW
0;JMP

(DRAWLINE)
	@lineCounter
	M = M + 1

	@pointer
	A = M
	M = M - 1

	@pointer
	M = M + 1
	M = M + 1
	
	@16
	D = A
	@lineCounter
	D = D - M

	@DRAWLINE
	D;JNE
	@returnLine
	0;JMP

(DRAWROW)
	@rowCounter
	M = M + 1

	@lineCounter
	M = 0
	@DRAWLINE
	0;JMP
	(returnLine)

	@16
	D = A
	@rowCounter
	D = D - M

	@DRAWROW
	D;JNE
	@returnRow
	0;JMP

@offset
M = 0
@mainCounter
M = 0
(MAIN)
	@mainCounter
	M = M + 1
	
	@rowCounter
	M = 0
	@DRAWROW
	0;JMP
	(returnRow)

	@offset
	D = M
	@if
	D;JEQ
	
	@pointer
	M = M - 1
	@offset
	M = M - 1
	@endIf
	0;JMP

	(if)
	@pointer
	M = M + 1
	@offset
	M = M + 1

	(endIf)
	
	@15
	D = A
	@mainCounter
	D = D - M
	@MAIN
	D;JNE
	@END
	0;JMP

(END)
	@END
	0;JMP
