; Auto generated code using the NesAsm project
.scope SubroutineResultsScript

  .proc main
    ; Argument of the method could not be used after calling a subroutine with return values
    jsr procB

    lda 0
    sta $40

    ; LDAa(b);
    ; STA(0x41);

    lda 2
    sta $42

    rts
  .endproc

  .proc procB
    lda #250
    sta 0
    lda #56
    sta 1
    lda #4
    sta 2
    lda #1
    sta 3

    rts
  .endproc

.endscope

