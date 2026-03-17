using Eto.Forms;
using Intersect.Editor.Content;
using Intersect.Editor.Forms;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Intersect.Framework.Core;
using Intersect.Utilities;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Core;


public static partial class Main
{

    private static long sAnimationTimer = Timing.Global.MillisecondsUtc;

    private static int sFps;

    private static int sFpsCount;

    private static long sFpsTime;

    private static Thread sMapThread;

    private static dynamic? sMyForm;

    private static FrmProgress? sProgressForm;

    private static long sWaterfallTimer = Timing.Global.MillisecondsUtc;

    private static UITimer sRenderTimer;

    public static bool LoopStarted = false;

    public static void StartLoop()
    {
        if (LoopStarted)
        {
            return;
        }
        LoopStarted = true;

        Console.WriteLine("Main.StartLoop() called");

        // Initialize MonoGame graphics device for rendering
        try
        {
            Core.Graphics.InitMonogame();
            Console.WriteLine("Graphics.InitMonogame() completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Graphics.InitMonogame() failed: {ex.Message}");
        }

        AppDomain.CurrentDomain.UnhandledException += Program.CurrentDomain_UnhandledException;
        Globals.MainForm.Visible = true;
        Globals.MainForm.EnterMap(Globals.CurrentMap == null ? Guid.Empty : Globals.CurrentMap.Id);
        sMyForm = Globals.MainForm;
        Console.WriteLine($"Map entered: {Globals.CurrentMap?.Id ?? Guid.Empty}");

        if (sMapThread == null)
        {
            sMapThread = new Thread(UpdateMaps);
            sMapThread.Start();

            // Use Eto UITimer for the render loop instead of blocking while loop
            sRenderTimer = new UITimer();
            sRenderTimer.Interval = 1.0 / 60.0; // 60 FPS
            sRenderTimer.Elapsed += (sender, e) =>
            {
                if (sMyForm != null && sMyForm.Visible)
                {
                    RunFrame();
                }
                else
                {
                    sRenderTimer.Stop();
                }
            };
            sRenderTimer.Start();
        }
    }

    public static void DrawFrame()
    {
        if (Globals.MapGrid == null)
        {
            return;
        }

        lock (Globals.MapGrid.GetMapGridLock())
        {
            Graphics.Render();
        }
    }

    public static void RunFrame()
    {
        //Shooting for 30fps
        var startTime = Timing.Global.MillisecondsUtc;
        sMyForm?.Update();

        if (sWaterfallTimer < Timing.Global.MillisecondsUtc)
        {
            Globals.WaterfallFrame++;
            if (Globals.WaterfallFrame == 3)
            {
                Globals.WaterfallFrame = 0;
            }

            sWaterfallTimer = Timing.Global.MillisecondsUtc + 500;
        }

        if (sAnimationTimer < Timing.Global.MillisecondsUtc)
        {
            Globals.AutotileFrame++;
            if (Globals.AutotileFrame == 3)
            {
                Globals.AutotileFrame = 0;
            }

            sAnimationTimer = Timing.Global.MillisecondsUtc + 600;
        }

        DrawFrame();

        GameContentManager.Update();
        Networking.Network.Update();

        sFpsCount++;
        if (sFpsTime < Timing.Global.MillisecondsUtc)
        {
            sFps = sFpsCount;
            sMyForm?.UpdateFpsLabel(sFps);
            sFpsCount = 0;
            sFpsTime = Timing.Global.MillisecondsUtc + 1000;
        }
    }

    private static void UpdateMaps()
    {
        while (!Globals.ClosingEditor)
        {
            if (Globals.MapsToScreenshot.Count > 0 &&
                Globals.FetchingMapPreviews == false)
            {
                if (sProgressForm == null || !sProgressForm.Visible)
                {
                    sProgressForm = new FrmProgress();

                    sProgressForm.SetTitle(Strings.MapCacheProgress.title);
                    Application.Instance.Invoke(() => sProgressForm.Show());

                    while (Globals.MapsToScreenshot.Count > 0)
                    {
                        try
                        {
                            var maps = MapInstance.Lookup.ValueList.ToArray();
                            foreach (MapInstance map in maps)
                            {
                                if (sProgressForm != null && sProgressForm.Visible)
                                {
                                    Application.Instance.Invoke(() =>
                                        sProgressForm.SetProgress(
                                            Strings.MapCacheProgress.remaining.ToString(
                                                Globals.MapsToScreenshot.Count
                                            ), -1, false
                                        )
                                    );
                                }

                                if (map != null)
                                {
                                    map.Update();
                                }

                                Networking.Network.Update();
                            }
                        }
                        catch (Exception exception)
                        {
                            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(
                                exception,
                                "JC's Solution for UpdateMaps collection was modified bug did not work!"
                            );
                        }

                        Thread.Sleep(50);
                    }

                    Globals.MapGrid.ResetForm();
                    Application.Instance.Invoke(() => sProgressForm?.Close());
                }
            }

            Thread.Sleep(100);
        }
    }

}
