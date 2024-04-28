; Auto generated code using the NesAsm project
.scope GeneratorDataScript

  .proc main

    ldx #0
  @loop1_on_X:
    lda sprite_data, x
    sta $200, x

    inx
    cpx #20
    bne @loop1_on_X

    rts
  .endproc

.segment "CODE"

sprite_data:
  .byte 48
  .byte 0
  .byte 2
  .byte 47

.segment "CODE"

named_argument:
  .byte 51
  .byte 2
  .byte 1
  .byte 50

.segment "CODE"

mix_ordered_and_named_argument:
  .byte 35
  .byte 8
  .byte 1
  .byte 36

.segment "CODE"

changed_argument_order:
  .byte 62
  .byte 4
  .byte 3
  .byte 63

.segment "CODE"

base_name:
  .byte 75
  .byte 6
  .byte 0
  .byte 74

.endscope

