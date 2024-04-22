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

.include "PPU.s"

.include "Game1Char.s"

.segment "CODE"

RightButtonPalette = $20A
LeftButtonPalette = $20E
DownButtonPalette = $212
UpButtonPalette = $216
StartButtonPalette = $21A
SelectButtonPalette = $21E
BButtonPalette = $222
AButtonPalette = $226
FaceX = $22B
FaceY = $228
.proc main
  ; Read the PPUSTATUS register $2002 PPU_STATUS
  lda PPU::PPU_STATUS

  ; Store the address $3f01 in the PPUADDR $2006 (begining of the background palette 0)
  lda #$3F
  ; 0x2006 PPU_ADDR
  sta PPU::PPU_ADDR
  lda #$00
  ; 0x2006 PPU_ADDR
  sta PPU::PPU_ADDR


  ldx #0
@loop1_on_X:
  lda game1Char.SpritePalettes, x

  ; Write palette data to PPUDATA $2007 PPU_DATA
  sta PPU::PPU_DATA

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
  stx RightButtonPalette
  stx LeftButtonPalette
  stx DownButtonPalette
  stx UpButtonPalette
  stx StartButtonPalette
  stx SelectButtonPalette
  stx BButtonPalette
  stx AButtonPalette

  ldx #1

  ; Check Button Right
  lda Controller::Down1
  ; if1_start
  and #Controller::BUTTON_RIGHT
  beq @if1_exit

  stx RightButtonPalette

@if1_exit:

  ; Check Button Left
  lda Controller::Down1
  ; if2_start
  and #Controller::BUTTON_LEFT
  beq @if2_exit

  stx LeftButtonPalette

@if2_exit:

  ; Check Button Down
  lda Controller::Down1
  ; if3_start
  and #Controller::BUTTON_DOWN
  beq @if3_exit

  stx DownButtonPalette

@if3_exit:

  ; Check Button Up
  lda Controller::Down1
  ; if4_start
  and #Controller::BUTTON_UP
  beq @if4_exit

  stx UpButtonPalette

@if4_exit:

  ; Check Button Start
  lda Controller::Down1
  ; if5_start
  and #Controller::BUTTON_START
  beq @if5_exit

  stx StartButtonPalette

@if5_exit:

  ; Check Button Select
  lda Controller::Down1
  ; if6_start
  and #Controller::BUTTON_SELECT
  beq @if6_exit

  stx SelectButtonPalette

@if6_exit:

  ; Check Button B
  lda Controller::Down1
  ; if7_start
  and #Controller::BUTTON_B
  beq @if7_exit

  stx BButtonPalette

@if7_exit:

  ; Check Button A
  lda Controller::Down1
  ; if8_start
  and #Controller::BUTTON_A
  beq @if8_exit

  stx AButtonPalette

@if8_exit:

  rts
.endproc

.proc moveFace
  ; Move right
  lda Controller::Down1
  ; if1_start
  and #Controller::BUTTON_RIGHT
  beq @if1_exit

  inc FaceX

@if1_exit:

  ; Move left
  lda Controller::Down1
  ; if2_start
  and #Controller::BUTTON_LEFT
  beq @if2_exit

  dec FaceX

@if2_exit:

  ; Move down
  lda Controller::Down1
  ; if3_start
  and #Controller::BUTTON_DOWN
  beq @if3_exit

  inc FaceY

@if3_exit:

  ; Move up
  lda Controller::Down1
  ; if4_start
  and #Controller::BUTTON_UP
  beq @if4_exit

  dec FaceY

@if4_exit:

  rts
.endproc

nmi:
; Transfer Sprites via OAM
lda #$00
; 0x2003 = OAM_ADDR
sta PPU::OAM_ADDR

lda #$02
; 0x4014 = OAM_DMA
sta PPU::OAM_DMA

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
  ; 0x2000 PPU_CTRL
  stx PPU::PPU_CTRL
  ; 0x2001 PPU_MASK
  stx PPU::PPU_MASK
  stx $4010

  ; 0x2002 PPU_STATUS
  bit PPU::PPU_STATUS

@vblankWait1:
  ; 0x2002 PPU_STATUS
  bit PPU::PPU_STATUS
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
  ; 0x2002 PPU_STATUS
  bit PPU::PPU_STATUS
  bpl @vblankWait2

  jsr resetPalettes

  ; To Main

  ; Enable Sprites & Background
  lda #%00011000
  ; 0x2002 PPU_STATUS
  sta PPU::PPU_MASK

  ; Enable NMI
  lda #%10000000
  ; 0x2000 PPU_CTRL
  sta PPU::PPU_CTRL

  jmp main

  rts
.endproc

.proc resetPalettes
  ; 0x2002 PPU_STATUS
  bit PPU::PPU_STATUS

  lda #$3f
  ; 0x2006 PPU_ADDR
  sta PPU::PPU_ADDR
  lda #$00
  ; 0x2006 PPU_ADDR
  sta PPU::PPU_ADDR

  lda #$0F
  ldx #$20

@paletteLoadLoop:

  ; 0x2007 PPU_DATA
  sta PPU::PPU_DATA
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


