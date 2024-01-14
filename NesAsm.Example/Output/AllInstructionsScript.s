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

@data:
  .byte %00000000
  .byte %00000001
  .byte %00000010
  .byte %00000011
  .byte %00000100
