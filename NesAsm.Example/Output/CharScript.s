.segment "CODE"

.proc main

  rts
.endproc

.segment "CHARS"

  .byte %01000001
  .byte %11000010
  .byte %01000100
  .byte %01001000
  .byte %00010000
  .byte %00100000
  .byte %01000000
  .byte %10000000

  .byte %00000001
  .byte %00000010
  .byte %00000100
  .byte %00001000
  .byte %00010110
  .byte %00100001
  .byte %01000010
  .byte %10000111

