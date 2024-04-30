; Auto generated code using the NesAsm project
.scope DataScript

  JOYPAD1 = $4016
  JOYPAD2 = $4017
  .proc main
    lda #1
    sta JOYPAD1
    lda #0
    sta JOYPAD1

    rts
  .endproc

.segment "CODE"

palettes:
  .byte 15
  .byte 32
  .byte 33
  .byte 34
  .byte 15
  .byte 0
  .byte 0
  .byte 0

  .byte 15
  .byte 0
  .byte 0
  .byte 0
  .byte 15
  .byte 0
  .byte 0
  .byte 0

  .byte 15
  .byte 32
  .byte 39
  .byte 49
  .byte 15
  .byte 0
  .byte 0
  .byte 0

  .byte 15
  .byte 0
  .byte 0
  .byte 0
  .byte 15
  .byte 0
  .byte 0
  .byte 0


.endscope

