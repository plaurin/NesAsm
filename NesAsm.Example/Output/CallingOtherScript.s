.segment "CODE"

.proc main
  jsr procB
  jsr myProcC

  rts
.endproc

.proc myProcC
  lda #30

  rts
.endproc

