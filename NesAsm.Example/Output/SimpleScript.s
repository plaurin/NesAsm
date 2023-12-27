.segment "CODE"

.proc main
  ; Start by loading the value 25 into the X register
  ldx #25

  ; Increment the value of the X registrer
  inx

  rts
.endproc

.include "wrapper.s"
