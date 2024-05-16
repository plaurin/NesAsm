; Auto generated code using the NesAsm project
.scope BranchScript

  .proc start
    ldx #0

  @loop:

    stx 1
    inx

    cpx #4

    bne @loop

    ; Memory location 0x01 should be equals to 3

    rts
  .endproc

.endscope

