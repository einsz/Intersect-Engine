using Eto.Forms;
using Eto.Drawing;

namespace Intersect.Editor.Forms.Controls;

public partial class AutoDragPanel : Panel
{
    private UITimer mDragTimer;
    private int mMaxDragChange = 2;
    private bool mIsDragging;
    private PointF mLastMousePosition;

    public AutoDragPanel()
    {
        mDragTimer = new UITimer
        {
            Interval = 0.001
        };

        mDragTimer.Elapsed += DragTimer_Tick;

        MouseDown += AutoDragPanel_MouseDown;
        MouseUp += AutoDragPanel_MouseUp;
        MouseMove += AutoDragPanel_MouseMove;
    }

    public int MaxDragChange
    {
        get => mMaxDragChange;
        set => mMaxDragChange = value;
    }

    private void AutoDragPanel_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Primary)
        {
            mIsDragging = true;
            mLastMousePosition = e.Location;
            mDragTimer.Start();
        }
    }

    private void AutoDragPanel_MouseUp(object sender, MouseEventArgs e)
    {
        mIsDragging = false;
        mDragTimer.Stop();
    }

    private void AutoDragPanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (mIsDragging)
        {
            mLastMousePosition = e.Location;
        }
    }

    private void DragTimer_Tick(object sender, EventArgs e)
    {
        if (!mIsDragging)
        {
            return;
        }

        var pos = mLastMousePosition;

        var right = (float)Width;
        var bottom = (float)Height;

        var scrollable = this as IScrollable;
        if (scrollable != null)
        {
            var scrollPosition = scrollable.ScrollPosition;

            // Vertical scroll
            if (pos.Y < 0)
            {
                var difference = (int)(-pos.Y);
                if (mMaxDragChange > 0 && difference > mMaxDragChange)
                {
                    difference = mMaxDragChange;
                }

                scrollPosition = new Point(scrollPosition.X, Math.Max(0, scrollPosition.Y - difference));
            }
            else if (pos.Y > bottom)
            {
                var difference = (int)(pos.Y - bottom);
                if (mMaxDragChange > 0 && difference > mMaxDragChange)
                {
                    difference = mMaxDragChange;
                }

                scrollPosition = new Point(scrollPosition.X, scrollPosition.Y + difference);
            }

            // Horizontal scroll
            if (pos.X < 0)
            {
                var difference = (int)(-pos.X);
                if (mMaxDragChange > 0 && difference > mMaxDragChange)
                {
                    difference = mMaxDragChange;
                }

                scrollPosition = new Point(Math.Max(0, scrollPosition.X - difference), scrollPosition.Y);
            }
            else if (pos.X > right)
            {
                var difference = (int)(pos.X - right);
                if (mMaxDragChange > 0 && difference > mMaxDragChange)
                {
                    difference = mMaxDragChange;
                }

                scrollPosition = new Point(scrollPosition.X + difference, scrollPosition.Y);
            }

            scrollable.ScrollPosition = scrollPosition;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            mDragTimer?.Stop();
            mDragTimer?.Dispose();
        }

        base.Dispose(disposing);
    }
}
