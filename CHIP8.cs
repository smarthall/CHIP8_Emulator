using System;

namespace CHIP8_Emulator
{
	public class CHIP8
	{
		// Constants
		private const UInt16 LOAD_ADDR = 0x200;

		// Current opcode
		private UInt16 opcode;
		
		// 4K of RAM
		private byte[] memory = new byte[4096];
		
		// 16 registers
		private byte[] V = new byte[16];
		
		// Index Register
		private UInt16 I;
		
		// Program Counter
		private UInt16 pc;
		
		// The screen
		private byte[] gfx = new byte[64 * 32];
		
		// Timer Registers
		private byte delay_timer;
		private byte sound_timer;
		
		// Stack
		private UInt16[] stack = new UInt16[16];
		private byte sp;
		
		// Key Inputs
		private byte[] key = new byte[16];
		
		// Fonts
		private byte[] fontset = new byte[] {
		  0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
		  0x20, 0x60, 0x20, 0x20, 0x70, // 1
		  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
		  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
		  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
		  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
		  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
		  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
		  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
		  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
		  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
		  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
		  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
		  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
		  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
		  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};

		public CHIP8 ()
		{
			initialize();
		}

		public void initialize ()
		{
			// Init core registers
			pc      = LOAD_ADDR; // this is where CHIP8 loads programs
			opcode  = 0;
			I       = 0;
			sp      = 0;

			// Clear Display
			for (int i = 0; i < gfx.Length; i++)
			{
				gfx[i] = 0;
			}

			// Clear Stack
			for (int i = 0; i < stack.Length; i++)
			{
				stack[i] = 0;
			}

			// Clear Registers
			for (int i = 0; i < V.Length; i++)
			{
				V[i] = 0;
			}

			// Clear Memory
			for (int i = 0; i < memory.Length; i++)
			{
				memory[i] = 0;
			}

			// Load Fonts
			for (int i = 0; i < fontset.Length; i++)
			{
				memory[i] = fontset[i];
			}

			// Reset Timers
			delay_timer = 0;
			sound_timer = 0;
		}
		
		public void load(string filename)
		{
			// Open the file
			System.IO.BinaryReader file = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open));
			
			// Start loading at LOAD_ADDR
			UInt16 offset = LOAD_ADDR;
			
			// Load Byte for Byte
			while (true)
			{
				try
				{
					memory[offset++] = file.ReadByte();
				}
				catch (System.IO.EndOfStreamException)
				{
					break;
				}
			}
		}
		
		public void cycle()
		{
			// Fetch the opcode
			opcode = (UInt16)(memory[pc] << 8 | memory[pc + 1]);
			
			switch (opcode & 0xF000)
			{

				// 1NNN: Jumps to address NNN.
				case 0x1000:
					pc = (UInt16)(opcode & 0x0FFF);
					break;
				
				// 2NNN: Calls subroutine at NNN.
				case 0x2000:
					stack[sp++] = pc;
					pc = (UInt16)(opcode & 0x0FFF);
					break;
				
				// ANNN: Sets I to the address NNN.
				case 0xA000:
					I = (UInt16)(opcode & 0x0FFF);
					pc += 2;
					break;
				
				// Unknown Opcode
				default:
      				Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
					break;
			}
			
			// Update timers
			if (delay_timer > 0) delay_timer--;
			if (sound_timer > 0) sound_timer--;
		}
	}
}

