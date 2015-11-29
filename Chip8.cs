using System;

namespace CHIP8_Emulator
{
	public class Chip8
	{
		// Constants
		private const ushort LOAD_ADDR = 0x200;
		private const ushort CARRY_REGISTER = 0xF;

		// Current opcode
		private ushort opcode;
		
		// 4K of RAM
		private byte[] memory = new byte[4096];
		
		// 16 registers
		private byte[] V = new byte[16];
		
		// Index Register
		private ushort I;
		
		// Program Counter
		private ushort pc;
		
		// The screen
		private byte[] gfx = new byte[64 * 32];
		private bool gfx_updated = true;
		
		// Timer Registers
		private byte delay_timer;
		private byte sound_timer;
		
		// Stack
		private ushort[] stack = new ushort[16];
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
		
		// Random numbers
		private Random rng = new Random();

		public Chip8 ()
		{
			Initialize();
		}
		
		public void ClearScreen()
		{
			// Clear Display
			for (int i = 0; i < gfx.Length; i++)
			{
				gfx[i] = 0;
			}
			
			gfx_updated = true;
		}

		public void Initialize ()
		{
			// Init core registers
			pc      = LOAD_ADDR; // this is where CHIP8 loads programs
			opcode  = 0;
			I       = 0;
			sp      = 0;

			// Clear Display
			ClearScreen ();

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
		
		public void Load(string filename)
		{
			// Open the file
			System.IO.BinaryReader file = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open));
			
			// Start loading at LOAD_ADDR
			ushort offset = LOAD_ADDR;
			
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
		
		public bool DrawRequired()
		{
			return gfx_updated;
		}
		
		public void KeyDown(byte key_id)
		{
			key[key_id] = 1;
		}
		
		public void KeyUp(byte key_id)
		{
			key[key_id] = 0;
		}
		
		public byte[] GetDisplay() {
			gfx_updated = false;
			return gfx;
		}
		
		public void Cycle()
		{
			byte x, y;
			ushort n;
			int res;
			
			// Fetch the opcode
			opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
			
			switch (opcode & 0xF000)
			{
				case 0x0000:
					switch (opcode)
					{
						// 00E0: Clears the screen.
						case 0x00E0:
							ClearScreen ();
							break;

						// 00EE: Returns from a subroutine.
						case 0x00EE:
							sp--;
							pc = (ushort)(stack[sp] + 2); // Go back to the instruction AFTER the one we came from
							break;
					
						// 0NNN: Calls RCA 1802 program at address NNN. Not necessary for most ROMs.
						default:
							Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
							break;
					}
					break;
				
				// 1NNN: Jumps to address NNN.
				case 0x1000:
					pc = (ushort)(opcode & 0x0FFF);
					break;
				
				// 2NNN: Calls subroutine at NNN.
				case 0x2000:
					stack[sp++] = pc;
					pc = (ushort)(opcode & 0x0FFF);
					break;
				
				// 3XNN: Skips the next instruction if VX equals NN.
				case 0x3000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (ushort)(opcode & 0x00FF);
					if (V[x] == n) {
						pc += 4;
					} else {
						pc += 2;
					}
					break;
				
				// 4XNN: Skips the next instruction if VX doesn't equal NN.
				case 0x4000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (ushort)(opcode & 0x00FF);
					if (V[x] != n) {
						pc += 4;
					} else {
						pc += 2;
					}
					break;
				
				// 5XY0: Skips the next instruction if VX equals VY.
				case 0x5000:
					x = (byte)((opcode & 0x0F00) >> 8);
					y = (byte)((opcode & 0x00F0) >> 4);
					if (V[x] == V[y]) {
						pc += 4;
					} else {
						pc += 2;
					}
					break;
				
				// 6XNN: Sets VX to NN.
				case 0x6000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (ushort)(opcode & 0x00FF);
					V[x] = (byte)n;
					pc += 2;
					break;
				
				// 7XNN: Adds NN to VX.
				case 0x7000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (ushort)(opcode & 0x00FF);
					res = V[x] + n;
				
					if (res > byte.MaxValue) {
						V[x] = (byte)(res - byte.MaxValue);
						V[CARRY_REGISTER] = 1;
					} else {
						V[x] = (byte)(res);
						V[CARRY_REGISTER] = 0;
					}
					pc += 2;
					break;
				
				// 8XXX: Various Register operations
				case 0x8000:
					// Get the two registers
					x = (byte)((opcode & 0x0F00) >> 8);
					y = (byte)((opcode & 0x00F0) >> 4);

					switch (opcode & 0x000F)
					{
						// 8XY0: Sets VX to the value of VY.
						case 0x0:
							V[x] = V[y];
							pc += 2;
							break;
					
						// 8XY1: Sets VX to VX or VY.
						case 0x1:
							V[x] = (byte)(V[x] | V[y]);
							pc += 2;
							break;
					
						// 8XY2: Sets VX to VX and VY.
						case 0x2:
							V[x] = (byte)(V[x] & V[y]);
							pc += 2;
							break;
					
						// 8XY3: Sets VX to VX xor VY.
						case 0x3:
							V[x] = (byte)(V[x] ^ V[y]);
							pc += 2;
							break;
					
						// 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
						case 0x4:
							res = V[x] + V[y];
							if (res > byte.MaxValue) {
								V[CARRY_REGISTER] = 1;
							} else {
								V[CARRY_REGISTER] = 0;
							}
							V[x] = (byte)(res);
							pc += 2;
							break;
					
						// 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
						case 0x5:
							res = V[x] - V[y];
							if (res < byte.MinValue) {
								V[CARRY_REGISTER] = 0;
							} else {
								V[CARRY_REGISTER] = 1;
							}
							V[x] = (byte)(res);
							pc += 2;
							break;
					
						// 8XY6: Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
						case 0x6:
							V[CARRY_REGISTER] = (byte)(V[x] & 0x01);
							V[x] = (byte)(V[x] << 1);
							pc += 2;
							break;
					
						// 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
						case 0x7:
							res = V[y] - V[x];
							if (res < byte.MinValue) {
								V[x] = (byte)(res + byte.MaxValue);
								V[CARRY_REGISTER] = 0;
							} else {
								V[x] = (byte)(res);
								V[CARRY_REGISTER] = 1;
							}
							pc += 2;
							break;
					
						// 8XYE: Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
						case 0xE:
							V[CARRY_REGISTER] = (byte)((V[x] & 0x80) >> 8);
							V[x] = (byte)(V[x] >> 1);
							pc += 2;
							break;
					
						default:
							Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
							break;
					}
					break;
				
				// 9XY0: Skips the next instruction if VX doesn't equal VY.
				case 0x9000:
					x = (byte)((opcode & 0x0F00) >> 8);
					y = (byte)((opcode & 0x00F0) >> 4);
					if (V[x] == V[y]) {
						pc += 4;
					} else {
						pc += 2;
					}
					break;
				
				// ANNN: Sets I to the address NNN.
				case 0xA000:
					I = (ushort)(opcode & 0x0FFF);
					pc += 2;
					break;
				
				// BNNN: Jumps to the address NNN plus V0.
				case 0xB000:
					pc = (ushort)((opcode & 0x0FFF) + V[0]);
					break;
				
				// CXNN: Sets VX to the result of a bitwise and operation on a random number and NN.
				case 0xC000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (byte)(opcode & 0x00FF);
					V[x] = (byte)(rng.Next() & n);
					pc += 2;
					break;
				
				// DXYN: Sprites stored in memory at location in index register (I), 8bits wide.
				//       Wraps around the screen.
				//       If when drawn, clears a pixel, register VF is set to 1 otherwise it is zero.
				//       All drawing is XOR drawing (i.e. it toggles the screen pixels).
				//       Sprites are drawn starting at position VX, VY.
				//       N is the number of 8bit rows that need to be drawn.
				//       If N is greater than 1, second line continues at position VX, VY+1, and so on.
				case 0xD000:
					x = (byte)((opcode & 0x0F00) >> 8);
					y = (byte)((opcode & 0x00F0) >> 4);
					n = (byte)(opcode & 0x000F);

					byte pixel;
				
					V[CARRY_REGISTER] = 0;
					for (int yline = 0; yline < n; yline++)
					{
						pixel = memory[I + yline];
						for(int xline = 0; xline < 8; xline++)
						{
 							if((pixel & (0x80 >> xline)) != 0)
							{
								if(gfx[(V[x] + xline + ((V[y] + yline) * 64))] == 1) V[CARRY_REGISTER] = 1;                                 
								gfx[V[x] + xline + ((V[y] + yline) * 64)] ^= 1;
							}
						}
					}
 					
					gfx_updated = true;
					pc += 2;
				
					break;
				
				// EXXX: Some key dependent routines
				case 0XE000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (byte)(opcode & 0x00FF);
				
					switch (n) {
						// EX9E: Skips the next instruction if the key stored in VX is pressed.
						case 0x9E:
							if (key[V[x]] != 0) {
								pc += 4;
							} else {
								pc += 2;
							}
							break;
				
						// EXA1: Skips the next instruction if the key stored in VX isn't pressed.
						case 0xA1:
							if (key[V[x]] == 0) {
								pc += 4;
							} else {
								pc += 2;
							}
							break;
					
						default:
							Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
							break;
					}
					break;
				
				// FXXX: IO Routines
				case 0xF000:
					x = (byte)((opcode & 0x0F00) >> 8);
					n = (byte)(opcode & 0x00FF);
				
					switch (n) {
						// FX07: Sets VX to the value of the delay timer.
						case 0x07:
							V[x] = delay_timer;
							pc += 2;
							break;
				
						// FX0A: A key press is awaited, and then stored in VX.
						case 0x0A:
							bool pressed = false;
					
							for (byte i = 0; i < 0xF; i++) {
								if (key[i] != 0) {
									pressed = true;
									V[x] = i;
									break;
								}
							}
							
							if (pressed) {
								pc += 2;
							} else {
								// Dont advance counter/timers until key is pressed.
								return;
							}
					
							break;
				
						// FX15: Sets the delay timer to VX.
						case 0x15:
							delay_timer = V[x];
							pc += 2;
							break;
				
						// FX18: Sets the sound timer to VX.
						case 0x18:
							sound_timer = V[x];
							pc += 2;
							break;
					
						// FX1E: Adds VX to I.
						case 0x1E:
							res = I + V[x];
							if (res > 0xFFF) {
								V[CARRY_REGISTER] = 1;
							} else {
								V[CARRY_REGISTER] = 0;
							}
							I = (ushort)res;
							pc += 2;
							break;
				
						// FX29: Sets I to the location of the sprite for the character in VX.
						//       Characters 0-F (in hexadecimal) are represented by a 4x5 font.
						case 0x29:
							I = (ushort)(V[x] * 5);
							pc += 2;
							break;
				
						// FX33: Stores the Binary-coded decimal representation of VX,
						//       with the most significant of three digits at the address in I,
						//       the middle digit at I plus 1, and the least significant digit at I plus 2.
						case 0x33:
							memory[I]     = (byte)(V[x] / 100);
							memory[I + 1] = (byte)(V[x] % 100 / 10);
							memory[I + 2] = (byte)(V[x] % 10);
							pc += 2;
							break;

						// FX55: Stores V0 to VX in memory starting at address I.
						case 0x55:
							for (int i = 0; i <= x; i++) {
								memory[I + i] = V[i];
							}
							pc += 2;
							break;
				
						// FX65: Fills V0 to VX with values from memory starting at address I.
						case 0x65:
							for (int i = 0; i <= x; i++) {
								V[i] = memory[I + i];
							}
							pc += 2;
							break;
					
						default:
							Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
							break;
					}
					break;
				
				// Unknown Opcode
				default:
      				Console.WriteLine("Unknown opcode: 0x{0:X}", opcode);
					break;
			}
			
			// Update timers
			if (delay_timer > 0) delay_timer--;
			if (sound_timer > 0) {
				if (sound_timer == 1) Console.Beep();
				sound_timer--;
			}
		}
	}
}

