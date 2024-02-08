; Auto generated code using the NesAsm project
.scope Controller

  JOYPAD1 = $4016
  JOYPAD2 = $4017
  BUTTON_A = %10000000
  BUTTON_B = %01000000
  BUTTON_SELECT = %00100000
  BUTTON_START = %00010000
  BUTTON_UP = %00001000
  BUTTON_DOWN = %00000100
  BUTTON_LEFT = %00000010
  BUTTON_RIGHT = %00000001
  Down = $20
  Pressed = $21
  .proc readControllerOne
    ; Initialize the output memory
    lda #1
    sta Down

    ; Send the latch pulse down to the 4021
    sta JOYPAD1
    lda #0
    sta JOYPAD1

    ; Read the buttons from the data line

  @read_loop:

    lda JOYPAD1
    lsr a
    rol Down
    bcc @read_loop

    ; Temp copy
    lda Down
    sta Pressed

    rts
  .endproc

.endscope

