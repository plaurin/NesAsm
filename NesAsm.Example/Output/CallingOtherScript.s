; Auto generated code using the NesAsm project
.include "MultiProcScript.s"

.scope CallingOtherScript

  .proc main
    jsr MultiProcScript::procB

    jsr myProcC

    jmp MultiProcScript::procC

    rts
  .endproc

  .proc myProcC
    lda MultiProcScript::Data

    rts
  .endproc

.endscope

