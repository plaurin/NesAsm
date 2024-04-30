; Auto generated code using the NesAsm project
.scope SubroutineResultScript

  .proc main
    ; Argument of the method could not be used after calling a subroutine with return values
    jsr procB

    lda 0
    sta $40

    rts
  .endproc

  .proc procB
    lda #178
    sta 0

    rts
  .endproc

.endscope

