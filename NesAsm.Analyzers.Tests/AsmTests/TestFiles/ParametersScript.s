; Auto generated code using the NesAsm project
.scope ParametersScript

  .proc start
    lda #25
    sta 0
    lda #232
    sta 1
    lda #3
    sta 2
    lda #1
    sta 3
    jsr procB
    lda #32
    sta 0
    lda #0
    sta 1
    lda #32
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
    jsr proc

    rts
  .endproc

  .proc invalidOpCodeUsingParameter

    rts
  .endproc

  .proc proc

    rts
  .endproc

.endscope

