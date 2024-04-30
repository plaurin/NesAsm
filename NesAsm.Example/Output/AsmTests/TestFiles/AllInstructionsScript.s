; Auto generated code using the NesAsm project
.scope AllInstructionsScript

  .proc allInstructions
    lda #2
    ldx #3
    ldy #4
    lda #%00011000
    lda #$10

    ; LDAi(5); // Enable Sprites & Background - Inline comment not supported yet

    lda $3F
    lda $2002
    lda $2002

    lda data, x
    lda data, y
    sta $20, x
    sta $200, y

    sta $2006
    sta $2000
    sta $2001
    sta $2002
    sta $2006
    sta $2007

    inc $200
    dec $200
    inx
    dex

    lsr a
    rol $20
    sei
    cld
    txs
    bit $2002

    and #%01010010

    cmp $30

  @label:
    beq @label
    bne @label
    bcc @label
    bpl @label

    rts
  .endproc

.segment "CODE"

data:
  .byte 0
  .byte 1
  .byte 2
  .byte 3
  .byte 4

.endscope

