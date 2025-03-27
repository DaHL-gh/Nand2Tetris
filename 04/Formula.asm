// RAM[4] = (RAM[0] * 3 + (RAM[1] | RAM[2])) & !RAM[3] + 11
@1
D = M
@2
D = D | M
@0
D = D + M
D = D + M
D = D + M
@4
M = D
@3
D = !M
@4
D = D & M
@11
D = D + A
@4
M = D

