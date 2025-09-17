grammar CK3;

file : script EOF;
script : ( 
    namedBlock
    | anonymousBlock 
    | keyValuePair
    | anonymousToken
    | comment ) + ;

namedBlock : token SCOPER '{' script? '}';
anonymousBlock : '{' script? '}';
keyValuePair : token SCOPER token;
anonymousToken : token;
token : TOKEN ( ( '.' | '|' ) TOKEN)*;
comment: COMMENT;

SCOPER : ('=' | '?=' | '<' | '<=' | '>' | '>=' | '!=' | '==');
COMMENT : '#' ~[\r\n]* '\r'?  ( '\n' | EOF );
TOKEN :
    QUOTED_TOKEN
    | CALCULATED_VAR
    | LINK 
    | DATE
    | NUMERAL
    | BARE_TOKEN;
QUOTED_TOKEN : '"' ~'"'* '"';
CALCULATED_VAR : '@' '[' ~']'* ']';
LINK : BARE_TOKEN ':' BARE_TOKEN;

NONZERODIGIT : ('1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9');
DIGIT : NONZERODIGIT | '0';
NUMERAL : '-'? DIGIT+ ( '.' DIGIT+ )? '%'?;
DATE : DIGIT DIGIT? DIGIT? DIGIT? DIGIT? '.' DIGIT DIGIT? '.' DIGIT DIGIT?;

BARE_TOKEN : '@'? AnyIdentifierCharacter+;

fragment AnyIdentifierCharacter : LetterOrDigit | '-' | '/' | '&' | '\'' | '%';

fragment LetterOrDigit: Letter | [0-9];

fragment Letter:
    [a-zA-Z$_]                        // these are the "java letters" below 0x7F
    | ~[\u0000-\u007F\uD800-\uDBFF]   // covers all characters above 0x7F which are not a surrogate
    | [\uD800-\uDBFF] [\uDC00-\uDFFF] // covers UTF-16 surrogate pairs encodings for U+10000 to U+10FFFF
;

WS: [ \t\r\n]+ -> skip;