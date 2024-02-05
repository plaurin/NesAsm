; Auto generated code using the NesAsm project
.segment "CODE"

.proc main
  ldx #12
  ldy #12

  ;if1_start
  cpx #12
  bne @if1_exit

  lda #1

@if1_exit:

  ;if2_start
  cpy #15
  bpl @if2_exit

  lda #2

@if2_exit:

  ;if3_start
  and #%00000001
  beq @if3_exit

  lda #3

@if3_exit:

  rts
.endproc

.proc invalidIf

  rts
.endproc

.include "wrapper.s"
