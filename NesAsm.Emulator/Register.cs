namespace NesAsm.Emulator;

public class Register
{
    public byte Value { get; set; }

    public static implicit operator Register(byte value) => new() { Value = value };
    public static implicit operator byte(Register register) => register.Value;
}