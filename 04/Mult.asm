// Программа должна умножать числа из R0 и R1 и сохранять результат в R2.
// while i - r0 != 0:
// 	r2 += r1
//  i += 1

// variables init
@R2
M = 0
@i
M = 0

(LOOP)
	// statement
	@i
	D = M
	@R0
	D = D - M
	@END // quiting loop
	D;JEQ

	// body
	@R1
	D = M
	@R2
	M = D + M
	@i
	M = M + 1

	@LOOP // next iteration
	0;JMP

(END)
	@END
	0;JMP

