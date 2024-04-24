; Auto generated code using the NesAsm project
.scope MoreThanFourPalettes

.segment "CHARS"

  ; Sprite Tile 0
  .byte 0, 0, 0, 0, 0, 0, 0, 0
  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 1
  .byte %00001000
  .byte %00001100
  .byte %00001110
  .byte %11111111
  .byte %11111111
  .byte %00001110
  .byte %00001100
  .byte %00001000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 2
  .byte %00010000
  .byte %00110000
  .byte %01110000
  .byte %11111111
  .byte %11111111
  .byte %01110000
  .byte %00110000
  .byte %00010000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 3
  .byte %00011000
  .byte %00011000
  .byte %00011000
  .byte %00011000
  .byte %11111111
  .byte %01111110
  .byte %00111100
  .byte %00011000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 4
  .byte %00011000
  .byte %00111100
  .byte %01111110
  .byte %11111111
  .byte %00011000
  .byte %00011000
  .byte %00011000
  .byte %00011000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 5
  .byte %00000000
  .byte %11100000
  .byte %10001110
  .byte %11000100
  .byte %01100100
  .byte %00100100
  .byte %11100100
  .byte %00000000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 6
  .byte %00000000
  .byte %11100000
  .byte %10001110
  .byte %11001000
  .byte %01101110
  .byte %00101000
  .byte %11101110
  .byte %00000000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 7
  .byte %11111000
  .byte %11111100
  .byte %11000010
  .byte %11111100
  .byte %11111100
  .byte %11000010
  .byte %11111100
  .byte %11111000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 8
  .byte %00111100
  .byte %00111100
  .byte %11100111
  .byte %11000011
  .byte %11111111
  .byte %11111111
  .byte %11000011
  .byte %11000011

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 9
  .byte %00111100
  .byte %01111110
  .byte %11011011
  .byte %11011011
  .byte %11111111
  .byte %11000011
  .byte %01111110
  .byte %00111100

  .byte 0, 0, 0, 0, 0, 0, 0, 0

  ; Sprite Tile 10
  .byte %00000000
  .byte %01100110
  .byte %01111110
  .byte %11111111
  .byte %11111111
  .byte %01111110
  .byte %00111100
  .byte %00011000

  .byte 0, 0, 0, 0, 0, 0, 0, 0

.segment "CODE"

sprite_palettes:
  ; Sprite Palette 0
  .byte $0F, $01, $0F, $0F

  ; Sprite Palette 1
  .byte $0F, $12, $0F, $0F

  ; Sprite Palette 2
  .byte $0F, $03, $0F, $0F

  ; Sprite Palette 3
  .byte $0F, $23, $0F, $0F

  ; Sprite Palette 4
  .byte $0F, $20, $0F, $0F

  ; Sprite Palette 5
  .byte $0F, $19, $0F, $0F

  ; Sprite Palette 6
  .byte $0F, $39, $0F, $0F

  ; Sprite Palette 7
  .byte $0F, $11, $0F, $0F

  ; Sprite Palette 8
  .byte $0F, $15, $0F, $0F

.endscope

