; Auto generated code using the NesAsm project
.scope IterationStatementsScript

  .proc main
    ; for - using X or Y register

    ldx #0
  @loop1_on_X:
    lda data, x

    sta $2007

    inx
    cpx #10
    bne @loop1_on_X


    ldy #0
  @loop2_on_Y:
    lda data, y

    iny
    cpy #10
    bne @loop2_on_Y

    ; for - nested loop

    ldx #0
  @loop3_on_X:

    ldy #0
  @loop4_on_Y:
    lda data, x
    lda data, y

    iny
    cpy #4
    bne @loop4_on_Y

    inx
    cpx #4
    bne @loop3_on_X

    ; TODO while condition

    ; while true - infinite loop
  @endless_loop:
    lda #42
    jmp @endless_loop

    rts
  .endproc

.segment "CODE"

data:
  .byte 1
  .byte 2
  .byte 3
  .byte 4
  .byte 5
  .byte 6
  .byte 7
  .byte 8

  .byte 9
  .byte 10

  .proc invalidFor
    ; NA0013 - Should be X or Y only

    ; NA0015 - Should not declare new local variable X

    ; NA0014 - Should not mix X and Y

    ; NA0016 - Should not use the same register in nester loop

    ldx #0
  @loop1_on_X:

    ldx #0
  @loop2_on_X:

    inx
    cpx #10
    bne @loop2_on_X

    ; NA0008 - do is not supported

    rts
  .endproc

.endscope

.include "wrapper.s"
