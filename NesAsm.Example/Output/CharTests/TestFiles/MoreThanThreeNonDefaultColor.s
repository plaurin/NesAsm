; Auto generated code using the NesAsm project
.scope MoreThanThreeNonDefaultColor

.segment "CHARS"

  ; Sprite Tile 0
  .byte 0, 0, 0, 0, 0, 0, 0, 0
  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 1
  .byte %00001000
  .byte %00001100
  .byte %00001100
  .byte %11000000
  .byte %11000000
  .byte %00000000
  .byte %00000000
  .byte %00000000

  .byte %00000000
  .byte %00000000
  .byte %00000010
  .byte %11001111
  .byte %11001111
  .byte %00000010
  .byte %00000000
  .byte %00000000


.segment "CODE"

sprite_palettes:
  ; Sprite Palette 0
  .byte $0F, $05, $22, $29

  ; Sprite Palette 1
  .byte $00, $00, $00, $00

  ; Sprite Palette 2
  .byte $00, $00, $00, $00

  ; Sprite Palette 3
  .byte $00, $00, $00, $00

.endscope

