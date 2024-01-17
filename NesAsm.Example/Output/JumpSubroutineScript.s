; Auto generated code using the NesAsm project
.segment "CODE"

.proc procA
  jsr procB
  jsr procC

  rts
.endproc

.proc procB
  lda #1

  rts
.endproc

.proc procC
  lda #2

  rts
.endproc

