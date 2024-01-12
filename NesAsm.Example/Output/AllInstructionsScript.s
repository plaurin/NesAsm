.segment "CODE"

.proc allInstructions
  lda $3F
  lda $2002

  sta $2006

  rts
.endproc

