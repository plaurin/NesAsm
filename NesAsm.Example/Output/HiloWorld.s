; Auto generated code using the NesAsm project
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

  lda palettes, x

  ; Write palette data to PPUDATA $2007
  sta $2007
  inx

  cpx #$20

  bne @loop

  rts
.endproc

nmi:
  ldx #$00
  stx $2003

@spriteLoop:

  lda hiloWorldSprites, x
  sta $2004
  inx

  cpx #44
  bne @spriteLoop
  rti

.segment "CODE"

hiloWorldSprites:
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 30
  .byte 1
  .byte 0
  .byte 30
  .byte 30
  .byte 2
  .byte 0
  .byte 39

  .byte 30
  .byte 3
  .byte 0
  .byte 48
  .byte 30
  .byte 4
  .byte 0
  .byte 57

  .byte 40
  .byte 5
  .byte 0
  .byte 75
  .byte 40
  .byte 4
  .byte 0
  .byte 84

  .byte 40
  .byte 6
  .byte 0
  .byte 93
  .byte 40
  .byte 3
  .byte 0
  .byte 102

  .byte 40
  .byte 7
  .byte 0
  .byte 111

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

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

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


.include "wrapper-no-nmi.s"
