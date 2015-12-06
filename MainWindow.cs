using System;
using System.Drawing;
using Gtk;
using MicroLibrary;

public partial class MainWindow: Gtk.Window
{
	// The Emulator Core
	CHIP8_Emulator.Chip8 emulator;
	
	// The Timer for the emulator
	MicroTimer hiResTimer = new MicroTimer((long)(1000000.0f / 60.0f)); //60 Hz
	
	// Display settings
	private const int SCREEN_WIDTH  = 64;
	private const int SCREEN_HEIGHT = 32;
	private const int SCREEN_ZOOM   = 8;
	
	// Display
	DrawingArea da;
	Bitmap screen = new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT);

	/*
	 * 1 2 3 4     1 2 3 C
	 * Q W E R __\ 4 5 6 D
	 * A S D F   / 7 8 9 E
	 * Z X C V     A 0 B F
	 */
	
	private uint[] keymap = {
		120, 49, 50, 51,
		113, 119, 101, 97,
		115, 100, 122, 99,
		52, 114, 102, 118,
	};

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		
		// Setup Emulator
		emulator = new CHIP8_Emulator.Chip8();
		emulator.Load("../../ROMS/PONG2");
		
		// Register our timer function
		hiResTimer.MicroTimerElapsed += new MicroTimer.MicroTimerElapsedEventHandler(hiResTick);
		hiResTimer.Enabled = true;
		
		// Install drawing area
		da = new DrawingArea();
		Add (da);
		da.Show();
		
		// Setup drawing function
		da.ExposeEvent += ScreenExposeEvent;
	}

	void ScreenExposeEvent (object o, ExposeEventArgs args)
	{
		Gdk.EventExpose ev = args.Event;
		Gdk.Window window = ev.Window;
		
		using (Graphics g = Gtk.DotNet.Graphics.FromDrawable(window)) {
			g.DrawImage((System.Drawing.Image)screen, new Rectangle(0, 0, SCREEN_WIDTH * SCREEN_ZOOM, SCREEN_HEIGHT * SCREEN_ZOOM));
		}
	}
	
	private void hiResTick(object sender, MicroTimerEventArgs timerEventArgs)
	{
		// Emulate a cycle
		emulator.Cycle();
		
		// If the screen was updated, draw it
		if (emulator.DrawRequired()) {
			byte[] screen_data = emulator.GetDisplay();

            for (int y = 0; y < SCREEN_HEIGHT; ++y)
            {
                for (int x = 0; x < SCREEN_WIDTH; ++x)
                {
                    if (screen_data[(y * SCREEN_WIDTH) + x] == 0)
                        screen.SetPixel(x, y, Color.Black);
                    else
                        screen.SetPixel(x, y, Color.White);
                }
            }
			
			QueueDraw();
		}
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		hiResTimer.Enabled = false;
		Application.Quit ();
		a.RetVal = true;
	}
	
	private byte GetKeyFromMap(uint KeyValue)
	{
		for (byte i = 0; i < keymap.Length; i++) {
			if (KeyValue == keymap[i]) {
				return i;
			}
		}
		
		return 255;
	}

	protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
	{
		byte KeyValue = GetKeyFromMap(evnt.KeyValue);
		
		if (KeyValue != 255) emulator.KeyDown(KeyValue);

		return base.OnKeyPressEvent (evnt);
	}

	protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
	{
		byte KeyValue = GetKeyFromMap(evnt.KeyValue);
		
		if (KeyValue != 255) emulator.KeyUp(KeyValue);

		return base.OnKeyReleaseEvent (evnt);
	}
}
