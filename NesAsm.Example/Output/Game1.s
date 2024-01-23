; Auto generated code using the NesAsm project
.segment "HEADER"
  .byte $4E, $45, $53, $1A  ; iNES header identifier
  .byte 2                   ; 2x 16KB PRG-ROM Banks
  .byte 1                   ; 1x  8KB CHR-ROM
  .byte $01                 ; mapper 0 - $01
  .byte $00                 ; $00 - vertical mirroring

.segment "VECTORS"
  .addr nmi
  .addr reset
  .addr 0

.segment "STARTUP"

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

@endless_loop:

  jsr readControllerOne

  jmp @endless_loop

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

.proc reset
  sei
  cld

  ldx #%01000000
  stx $4017

  ldx #$ff
  txs

  ldx #%00010000
  stx $2000
  stx $2001
  stx $4010

  bit $2002

@vblankWait1:
  bit $2002
  bpl @vblankWait1

@clearMemory:

  lda #$00

  sta $0000, x
  sta $0100, x
  sta $0200, x
  sta $0300, x
  sta $0400, x
  sta $0500, x
  sta $0600, x
  sta $0700, x

  inx
  bne @clearMemory

@vblankWait2:
  bit $2002
  bpl @vblankWait2

  jsr resetPalettes

  ; To Main

  ; Enable Sprites & Background
  lda #%00011000
  sta $2001

  ; Enable NMI
  lda #%10000000
  sta $2000

  jmp main

  rts
.endproc

.proc resetPalettes
  bit $2002

  lda #$3f
  sta $2006
  lda #$00
  sta $2006

  lda #$0F
  ldx #$20

@paletteLoadLoop:

  sta $2007
  dex
  bne @paletteLoadLoop

  rts
.endproc

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

.include "ReadController.s"
