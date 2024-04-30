; Auto generated code using the NesAsm project
.scope ParametersScript

  .proc main
    lda #25
    sta 0
    lda #232
    sta 1
    lda #3
    sta 2
    lda #1
    sta 3
    jsr procB

    rts
  .endproc

  .proc procB
    lda 0
    sta $40

    ; LDXa(b);
    ; STA(0x41);

    ldy 2
    sta $42

    rts
  .endproc

  .proc invalidParamType

    rts
  .endproc

  .proc invalidOpCodeUsingParameter

    rts
  .endproc

  .proc lDAa

    rts
  .endproc

.endscope

