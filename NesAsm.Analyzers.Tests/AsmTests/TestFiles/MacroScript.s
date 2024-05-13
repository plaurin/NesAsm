; Auto generated code using the NesAsm project
.scope MacroScript

  .macro MacroNoParam 
    macroWithParams 5, 10, $4000
  .endmacro

  .macro MacroWithParams a, b, c
    macroOneParam (c + b * $20 + a)
  .endmacro

  .macro MacroOneParam a
    bit $2002
    lda #.HIBYTE(a)
    sta $2006
    lda #.LOBYTE(a)
    sta $2006
  .endmacro

.endscope

