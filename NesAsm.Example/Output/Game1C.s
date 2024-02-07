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

.include "Controller.s"

.segment "CODE"

.proc main
  ; Read the PPUSTATUS register $2002
  lda $2002

  ; Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
  lda #$3F
  sta $2006
  lda #$00
  sta $2006


  ldx #0
@loop1_on_X:
  lda palettes, x

  ; Write palette data to PPUDATA $2007
  sta $2007

  inx
  cpx #32
  bne @loop1_on_X

  ; Transfert sprite data into $200-$2ff memory range


  ldx #0
@loop2_on_X:
  lda controllerSprites, x
  sta $200, x

  inx
  cpx #48
  bne @loop2_on_X

  ; Main game loop
@endless_loop:
  jsr Controller::readControllerOne

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
  lda $21
  ; if1_start
  and #%00000001
  beq @if1_exit

  stx $20A

@if1_exit:

  ; Check Button Left
  lda $21
  ; if2_start
  and #%00000010
  beq @if2_exit

  stx $20E

@if2_exit:

  ; Check Button Down
  lda $21
  ; if3_start
  and #%00000100
  beq @if3_exit

  stx $212

@if3_exit:

  ; Check Button Up
  lda $21
  ; if4_start
  and #%00001000
  beq @if4_exit

  stx $216

@if4_exit:

  ; Check Button Start
  lda $21
  ; if5_start
  and #%00010000
  beq @if5_exit

  stx $21A

@if5_exit:

  ; Check Button Select
  lda $21
  ; if6_start
  and #%00100000
  beq @if6_exit

  stx $21E

@if6_exit:

  ; Check Button B
  lda $21
  ; if7_start
  and #%01000000
  beq @if7_exit

  stx $222

@if7_exit:

  ; Check Button A
  lda $21
  ; if8_start
  and #%10000000
  beq @if8_exit

  stx $226

@if8_exit:

  rts
.endproc

.proc moveFace
  ; Move right
  lda $21
  ; if1_start
  and #%00000001
  beq @if1_exit

  inc $22B

@if1_exit:

  ; Move right
  lda $21
  ; if2_start
  and #%00000010
  beq @if2_exit

  dec $22B

@if2_exit:

  ; Move down
  lda $21
  ; if3_start
  and #%00000100
  beq @if3_exit

  inc $228

@if3_exit:

  ; Move up
  lda $21
  ; if4_start
  and #%00001000
  beq @if4_exit

  dec $228

@if4_exit:

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

  ; Clear Memory - TODO Generate a loop that can go all the way from exactly 0 to 255 and don't stop before
  lda #$00

  ldx #0
@loop1_on_X:
  sta $0000, x
  sta $0100, x
  sta $0200, x
  sta $0300, x
  sta $0400, x
  sta $0500, x
  sta $0600, x
  sta $0700, x

  inx
  cpx #255
  bne @loop1_on_X

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


