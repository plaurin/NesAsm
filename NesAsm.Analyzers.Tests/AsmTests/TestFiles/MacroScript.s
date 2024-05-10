; Auto generated code using the NesAsm project
.scope MacroScript

  .macro MacroNoParam 
    MacroWithParams 5, 10, 0x4000)
  .endmacro

  .macro MacroWithParams a, b, c
  .endmacro

  .macro MacroOneParam a
    bit $2002
    lda #.HIBYTE(a)
    sta $2006
    lda #.LOBYTE(a)
    sta $2006
  .endmacro

.endscope

