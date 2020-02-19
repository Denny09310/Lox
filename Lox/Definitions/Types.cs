﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{

    public enum TokenType
    {
        //Single character tokens
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
        QUESTION_MARK, COLON,

        //One OR two character tokens
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
        PLUS_PLUS, MINUS_MINUS,

        //Literals
        IDENTIFIER, STRING, NUMBER,

        //Keywords
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        EOF
    }

    public enum FunctionType
    {
        FUNCTION,
        INITIALIZER,
        METHOD,
        NONE
    }

    public enum ClassType
    {
        CLASS,
        SUBCLASS,
        NONE
    }

}
