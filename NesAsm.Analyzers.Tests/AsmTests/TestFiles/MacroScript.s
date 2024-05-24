; Auto generated code using the NesAsm project
.scope MacroScript

  ADDR = $2000
  .macro MacroNoParam 
    MacroWithParams 5, 10, $4000
  .endmacro

  .macro MacroWithParams a, b, c
    MacroOneParam (c + b * $20 + a)

    ; TODO Fix arg usage in macro should use the arg name directly?
    lda 0
    ldx 1
  .endmacro

  .macro MacroOneParam a
    bit $2002
    lda #.HIBYTE(a)
    sta $2006
    lda #.LOBYTE(a)
    sta $2006
  .endmacro

  .macro MacroWithLoops 

    ldx #0
  :

    ldy #0
  :
    lda #25

    iny
    cpy #4
    bne :-

    inx
    cpx #4
    bne :--


    ldx #0
  :

    ldy #0
  :
    lda #52

    iny
    cpy #2
    bne :-

    ldy #0
  :
    lda #2

    iny
    cpy #6
    bne :-

    inx
    cpx #2
    bne :---
  .endmacro

.endscope

