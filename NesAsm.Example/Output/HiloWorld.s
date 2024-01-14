.segment "CODE"

.proc main
  ; Read the PPUSTATUS register $2002
  lda $2002

  ; Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
  lda #$3F
  sta $2006
  lda #$00
  sta $2006

@loop:

  ; TODO lda palettes, x
  lda palettes, x

  ; Write palette data to PPUDATA $2007
  sta $2007
  inx

  cpx #$20

  bne @loop

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

.segment "CHARS"

  .byte 195
  .byte 195
  .byte 195
  .byte 255
  .byte 255
  .byte 195
  .byte 195
  .byte 195

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 255
  .byte 255
  .byte 24
  .byte 24
  .byte 24
  .byte 24
  .byte 255
  .byte 255

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 192
  .byte 192
  .byte 192
  .byte 192
  .byte 192
  .byte 192
  .byte 255
  .byte 255

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 24
  .byte 60
  .byte 102
  .byte 195
  .byte 195
  .byte 102
  .byte 60
  .byte 24

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 195
  .byte 195
  .byte 195
  .byte 195
  .byte 102
  .byte 126
  .byte 102
  .byte 102

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 248
  .byte 254
  .byte 195
  .byte 207
  .byte 248
  .byte 204
  .byte 198
  .byte 195

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 248
  .byte 252
  .byte 198
  .byte 195
  .byte 195
  .byte 198
  .byte 252
  .byte 248

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

.include "wrapper.s"
