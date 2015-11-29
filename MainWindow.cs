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
	private const int SCREEN_ZOOM   = 4;
	
	// Display
	DrawingArea da;
	Bitmap screen = new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT);
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		
		// Setup Emulator
		emulator = new CHIP8_Emulator.Chip8();
		emulator.Load("../../ROMS/INVADERS");
		
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
			g.DrawImage((System.Drawing.Image)screen, new Rectangle(0, 0, 320,160));
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
	
	protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
	{
		return base.OnKeyPressEvent (evnt);
	}

	protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
	{
		return base.OnKeyReleaseEvent (evnt);
	}
}
