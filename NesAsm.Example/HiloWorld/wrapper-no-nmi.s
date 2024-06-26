.segment "HEADER"
  .byte $4E, $45, $53, $1A  ; iNES header identifier
  .byte 2                   ; 2x 16KB PRG-ROM Banks
  .byte 1                   ; 1x  8KB CHR-ROM
  .byte $01, $00            ; mapper 0, vertical mirroring

.segment "VECTORS"
  .addr HiloWorld::nmi
  .addr reset
  .addr 0

.segment "STARTUP"

.segment "CHARS"

.segment "CODE"

.proc ResetPalettes
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

.proc reset
  sei
  cld
  ldx #%01000000
  stx $4017
  ldx #$ff
  txs
  ldx #0
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
  jsr ResetPalettes
start:
  jsr HiloWorld::main
  lda #%10000000	; Enable NMI
  sta $2000
  lda #%00011000  ; Enable Sprites & Background
  sta $2001
endlessLoop:
  jmp endlessLoop
.endproc
