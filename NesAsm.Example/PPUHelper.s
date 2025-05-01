; Auto generated code using the NesAsm project
.scope PPUHelper

  PPU_CTRL = $2000
  PPU_MASK = $2001
  PPU_STATUS = $2002
  OAM_ADDR = $2003
  OAM_DATA = $2004
  OAM_DMA = $4014
  PPU_SCROLL = $2005
  PPU_ADDR = $2006
  PPU_DATA = $2007
  NAMETABLE_A = $2000
  NAMETABLE_B = $2400
  NAMETABLE_C = $2800
  NAMETABLE_D = $2c00
  ATTR_A = $23c0
  ATTR_B = $27c0
  ATTR_C = $2bc0
  ATTR_D = $2fc0
  .macro VramColRow col, row, nametable
    VramAddress (nametable + row * $20 + col)
  .endmacro

  .macro AttributeColRow col, row, attributeTable
    ; TODO C# only parameter checker
    VramAddress (attributeTable + row * $08 + col)
  .endmacro

  .macro VramAddress address
    bit $2002
    lda #.HIBYTE(address)
    sta $2006
    lda #.LOBYTE(address)
    sta $2006
  .endmacro

  .macro VramReset 
    bit $2002
    lda #0
    sta $2006
    sta $2006
  .endmacro

  .macro DrawNametableLine 

    ldx #0
  :
    sta $2007

    inx
    cpx #32
    bne :-
  .endmacro

.endscope

