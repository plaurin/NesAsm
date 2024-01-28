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

  ; Transfert sprite data into $200-$2ff memory range
  ldx #$00

@spriteLoop:

  lda controllerSprites, x
  sta $200, x
  inx

  cpx #48
  bne @spriteLoop

  ; Main game loop
@endless_loop:

  jsr readControllerOne

  jsr updateController

  jsr moveFace

  ; Wait for VBlank
  lda $30

@WaitForVBlank:
  cmp $30
  beq @WaitForVBlank

  jmp @endless_loop

  rts
.endproc

.proc updateController
  ; ## Update button palette
  ; Init palette to zero
  ldx #0
  stx $20A
  stx $20E
  stx $212
  stx $216
  stx $21A
  stx $21E
  stx $222
  stx $226

  ldx #1

  ; Check Button Right
@CheckRight:
  lda $21
  and #%00000001
  beq @CheckLeft
  stx $20A

  ; Check Button Left
@CheckLeft:
  lda $21
  and #%00000010
  beq @CheckDown
  stx $20E

  ; Check Button Down
@CheckDown:
  lda $21
  and #%00000100
  beq @CheckUp
  stx $212

  ; Check Button Up
@CheckUp:
  lda $21
  and #%00001000
  beq @CheckStart
  stx $216

  ; Check Button Start
@CheckStart:
  lda $21
  and #%00010000
  beq @CheckSelect
  stx $21A

  ; Check Button Select
@CheckSelect:
  lda $21
  and #%00100000
  beq @CheckB
  stx $21E

  ; Check Button B
@CheckB:
  lda $21
  and #%01000000
  beq @CheckA
  stx $222

  ; Check Button A
@CheckA:
  lda $21
  and #%10000000
  beq @EndCheck
  stx $226

@EndCheck:
  ; Hack because it is not possible to finish a method with a label?!?
  lda #0

  rts
.endproc

.proc moveFace
  ; Move right
  lda $21
  and #%00000001
  beq @MoveLeft
  inc $22B

@MoveLeft:
  lda $21
  and #%00000010
  beq @MoveDown
  dec $22B

@MoveDown:
  lda $21
  and #%00000100
  beq @MoveUp
  inc $228

@MoveUp:
  lda $21
  and #%00001000
  beq @End
  dec $228

@End:
  lda 0

  rts
.endproc

nmi:
  ; Transfer Sprites via OAM
  lda #$00
  ; 0x2003 = OAM_ADDR
  sta $2003

  lda #$02
  ; 0x4014 = OAM_DMA
  sta $4014

  ; Increment frame counter
  inc $30
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

controllerSprites:
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
  .byte 50
  .byte 30
  .byte 2
  .byte 0
  .byte 30

  .byte 40
  .byte 3
  .byte 0
  .byte 40
  .byte 20
  .byte 4
  .byte 0
  .byte 40

  .byte 30
  .byte 5
  .byte 0
  .byte 70
  .byte 30
  .byte 6
  .byte 0
  .byte 60

  .byte 30
  .byte 7
  .byte 0
  .byte 80
  .byte 30
  .byte 8
  .byte 1
  .byte 90

  .byte 80
  .byte 9
  .byte 3
  .byte 80
  .byte 80
  .byte 10
  .byte 2
  .byte 100


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
  .byte 41
  .byte 0
  .byte 0

  .byte 15
  .byte 22
  .byte 0
  .byte 0
  .byte 15
  .byte 18
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

  .byte 8
  .byte 12
  .byte 14
  .byte 255
  .byte 255
  .byte 14
  .byte 12
  .byte 8

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 16
  .byte 48
  .byte 112
  .byte 255
  .byte 255
  .byte 112
  .byte 48
  .byte 16

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 24
  .byte 24
  .byte 24
  .byte 24
  .byte 255
  .byte 126
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

  .byte 24
  .byte 60
  .byte 126
  .byte 255
  .byte 24
  .byte 24
  .byte 24
  .byte 24

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 0
  .byte 224
  .byte 142
  .byte 196
  .byte 100
  .byte 36
  .byte 228
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
  .byte 224
  .byte 142
  .byte 200
  .byte 110
  .byte 40
  .byte 238
  .byte 0

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
  .byte 252
  .byte 252
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

  .byte 60
  .byte 60
  .byte 230
  .byte 195
  .byte 255
  .byte 255
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

  .byte 60
  .byte 126
  .byte 219
  .byte 219
  .byte 255
  .byte 195
  .byte 126
  .byte 60

  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0
  .byte 0

  .byte 0
  .byte 102
  .byte 126
  .byte 255
  .byte 255
  .byte 126
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


.include "ReadController.s"
