@300
D=A
@LCL
M=D
@256
D=A
@SP
M=D

// push constant 10
@10
D=A
@SP
M=M+1
A=M-1
M=D

//pop local 0
@LCL
D=M
@0
D=D+A
@13
M=D

@SP
M=M-1
A=M
D=M

@13
A=M
M=D


