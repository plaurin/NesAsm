; Auto generated code using the NesAsm project
.scope MultiProcScript

  Data = $FF
.segment "CODE"

sprite_palettes:
  .byte 15
  .byte 32
  .byte 15
  .byte 15
  .byte 15
  .byte 17
  .byte 15
  .byte 15

  .byte 15
  .byte 21
  .byte 15
  .byte 15

  .proc procA
    lda #10

    rts
  .endproc

  .proc procB
    lda #20

    rts
  .endproc

  .proc procC
    lda #30

    rts
  .endproc

.endscope

