; Auto generated code using the NesAsm project
.include "MultiProcScript.s"

.include "../UpFolderScript.s"

.include "SubFolder/SubFolderScript.s"

.scope CallingOtherScript

  .proc start
    jsr MultiProcScript::procB

    jsr myProcC

    jmp MultiProcScript::procC

    rts
  .endproc

  .proc myProcC
    lda MultiProcScript::Data
    lda MultiProcScript::sprite_palettes, x

    jsr UpFolderScript::procU
    jsr SubFolderScript::procS

    rts
  .endproc

.endscope

