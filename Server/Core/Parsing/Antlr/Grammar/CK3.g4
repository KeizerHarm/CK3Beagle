grammar CK3;

file : script? EOF;
scriptElement
    : namedBlock
    | anonymousBlock
    | binaryExpression
    | comment
    | anonymousToken
    ;
script: scriptElement+;

namedBlock : identifier=tokenChain SCOPER '{' script? '}';
anonymousBlock : '{' script? '}';
binaryExpression : key=tokenChain SCOPER value=tokenChain;
anonymousToken : identifier=tokenChain;
comment: COMMENTLINE+;
tokenChain : token (('.' | '|') token)* ;
token      : QUOTED_TOKEN | CALCULATED_VAR | LINK | DATE | NUMERAL | BARE_TOKEN ;

SCOPER : ('=' | '?=' | '<' | '<=' | '>' | '>=' | '!=' | '==');
COMMENTLINE : '#' ~[\r\n]* '\r'?  ( '\n' | EOF );
QUOTED_TOKEN : '"' ~'"'* '"';
CALCULATED_VAR : '@' '[' ~']'* ']';
LINK : BARE_TOKEN ':' BARE_TOKEN;

NUMERAL : '-'? DIGIT+ ( '.' DIGIT+ )? '%'?;
DATE : DIGIT DIGIT? DIGIT? DIGIT? DIGIT? '.' DIGIT DIGIT? '.' DIGIT DIGIT?;
DIGIT : NONZERODIGIT | '0';
NONZERODIGIT : ('1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9');

BARE_TOKEN : '@'? AnyIdentifierCharacter+;

fragment AnyIdentifierCharacter : LetterOrDigit | '-' | '/' | '&' | '\'' | '%';

fragment LetterOrDigit: Letter | [0-9];

fragment Letter:
    [a-zA-Z$_]                        // these are the "java letters" below 0x7F
    | ~[\u0000-\u007F\uD800-\uDBFF]   // covers all characters above 0x7F which are not a surrogate
    | [\uD800-\uDBFF] [\uDC00-\uDFFF] // covers UTF-16 surrogate pairs encodings for U+10000 to U+10FFFF
;

WS: [ \t\r\n]+ -> skip;