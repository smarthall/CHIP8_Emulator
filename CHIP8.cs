using System;

namespace CHIP8_Emulator
{
	public class CHIP8
	{
		// Current opcode
		private UInt16 opcode;
		
		// 4K of RAM
		private byte[] memory = new byte[4096];
		
		// 16 registers
		private byte[] V = new byte[16];
		
		// Index Register
		private UInt16 i;
		
		// Program Counter
		private UInt16 pc;
		
		// The screen
		private byte[] gfx = new byte[64 * 32];
		
		// Timer Registers
		private byte delay_timer;
		private byte sound_timer;
		
		// Stack
		private byte[] stack = new byte[16];
		private byte sp;
		
		// Key Inputs
		private byte[] key = new byte[16];
		
		public CHIP8 ()
		{
		}
	}
}

