// Author: Kelly Rey Wilson kelly@MolecularJig.com
//
// Copyright (c) 2014, NightPen, LLC and MolecularJig
//
// All rights reserved.
//
// when() statement Patent Pending
//
// While the source to JigScript is copyrighted, any JigScript
// add on function libraries you create or any JigScript script
// files you create are yours to do with as you please! If you
// develop a really cool game or function library, we would
// love to see it. You can contact us at MolecularJig.com

namespace NightPen.JigScript
{
    public enum OpCode : int
    {
        GROUPM = 0xff00, //Mask for extracting out only the group.
        CODEM  = 0x00ff, //Mash for extracting out only the code.

        GROUP1 = 0x0100, //Start of no operand opcodes.
        NOP    = 0x0101, //No operation, place holder
        CPS    = 0x0102, //Create a parameter list for a user defined function call.
        PUSHA  = 0x0103, //Push AX
        POPA   = 0x0104, //Pop AX
        PUSHB  = 0x0105, //Push BX
        POPB   = 0x0106, //Pop BX
        POP    = 0x0107, //remove extra values
        END    = 0x0108, //IC = Program.Count(), Stop running the program.
        RET    = 0x0109, //POP IC
        PUSHP  = 0x010a, //PUSHP Pushe a parameter for a user defined function.
        POPI   = 0x010b, //POPI pops the value off the stack into I. I is set to 0 after being read by GetValue which sets VALUE.index to the value in IRegister.I
        INT    = 0x010c, //INT places a request into the currently running problem to to a JR call to a when as soon as possible.
        RTI    = 0x010d, //RTI return from a when function.
        YIELD  = 0x010e, //Yield till end of frame.
        POPC   = 0x0110, //Remove top of stack, convert to int and put into CX.
        PRC    = 0x0111, //Pushes the repeat count onto the value stack as if it were an integer variable.
        RFU    = 0x0112, //Return from update.
        DNR    = 0x0113, //Duplicate top of stack change to non-reference used for math and bitwise assignments.

        GROUP2 = 0x0210, //Start of arithmetic codes, all codes do automatic (POPB, POPA, opcode, PUSHA)
        ADD    = 0x0211, //AX = AX + BX
        SUB    = 0x0212, //AX = AX - BX
        MUL    = 0x0213, //AX = AX * BX
        MOD    = 0x0214, //AX = AX % BX
        DIV    = 0x0215, //AX = AX / BX
        AND    = 0x0216, //AX = AX & BX
        OR     = 0x0217, //AX = AX | BX
        XOR    = 0x0218, //AX = AX ^ BX
        SHR    = 0x0219, //AX = AX >> BX
        SHL    = 0x021a, //AX = AX << BX
        
        LOGICS = 0x021b, //Start of logic codes needed due to array[0] operations that or the results.
        LT     = 0x021b, //AX = AX < BX 
        GT     = 0x021c, //AX = AX > BX
        LTE    = 0x021d, //AX = AX <= BX 
        GTE    = 0x021e, //AX = AX >= BX 
        EQ     = 0x021f, //AX = AX == BX
        NEQ    = 0x0220, //AX = AX != BX
        LAND   = 0x0221, //AX = AX && BX
        LOR    = 0x0222, //AX = AX || BX
        LOGICE = 0x0222, //End of logic codes needed due to array[0] operations that or the results.
        
        NEG    = 0x0223, //AX = !AX BX is ignored.
        
        GROUP3 = 0x0430, //Start unary codes
        INC    = 0x0431, //Increment value[operand 1][arrayIndex]
        DEC    = 0x0432, //Decrement value[operand 1][arrayIndex]
        PUSHV  = 0x0434, //PUSH variable
        PUSHI  = 0x0435, //Push the value in the instruction
        

        GROUP4 = 0x0840, //Start of jump codes.
        JA     = 0x0841, //IC = address
        JF     = 0x0842, //IC = address if (AX is false)
        JR     = 0x0843, //PUSH IC; IC = address
        CALL   = 0x0844, //CALL user function, execution pauses if the function was added with wait set to true. The opCode address contains the index of the function.
        JCX    = 0x0855, //Decrement CX and if not 0 jump to the address.
        
        GROUP5 = 0x1050, //Start of move register memory codes.
        MOVA   = 0x1051, //MOV AX, [BX][IX]
        MOVM   = 0x1052, //MOV [BX][IX], AX
        LDA    = 0x1053, //LDA VALUE
        LDB    = 0x1054, //LDB VALUE
        LDI    = 0x1055, //LDI VALUE

        GROUP6 = 0x2050  //Start of next group.
    }
    ;
}