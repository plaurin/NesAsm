; Auto generated code using the NesAsm project
.segment "CODE"

.proc allInstructions
  lda #2
  ldx #3
  ldy #4

  lda $3F
  lda $2002

  lda data, x
  lda data, y

  sta $2006

  rts
.endproc

.segment "CODE"

data:
  .byte 0
  .byte 1
  .byte 2
  .byte 3
  .byte 4
