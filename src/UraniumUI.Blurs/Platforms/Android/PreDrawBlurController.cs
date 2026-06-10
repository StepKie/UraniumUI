using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using View = Android.Views.View;
using Color = Android.Graphics.Color;

namespace UraniumUI.Blurs;

public class PreDrawBlurController : IBlurController
{
    public const int TRANSPARENT = 0;

    private float blurRadius = BlurViewDefaults.BLUR_RADIUS;

    private readonly IBlurAlgorithm blurAlgorithm;
    private BlurViewCanvas internalCanvas;
    private Bitmap internalBitmap;

    View blurView;
    private Color overlayColor;
    private ViewGroup rootView;
    private int[] rootLocation = new int[2];
    private int[] blurViewLocation = new int[2];

    private bool blurEnabled = true;
    private bool initialized;
    private bool destroyed;

    private Drawable frameClearDrawable;

    private readonly OnPreDrawListener drawListener;
    public PreDrawBlurController(View blurView, ViewGroup rootView, Color overlayColor, IBlurAlgorithm algorithm)
    {
        drawListener = new OnPreDrawListener(() =>
        {
            UpdateBlur();
        });

        this.rootView = rootView;
        this.blurView = blurView;
        this.overlayColor = overlayColor;
        this.blurAlgorithm = algorithm;

        if (algorithm is RenderEffectBlur renderEffectBlur) {
            renderEffectBlur.SetContext(blurView.Context);
        }

        int measuredWidth = blurView.MeasuredWidth;
        int measuredHeight = blurView.MeasuredHeight;

        Init(measuredWidth, measuredHeight);
    }

    void Init(int measuredWidth, int measuredHeight)
    {
        if (destroyed)
        {
            return;
        }

        SetBlurAutoUpdate(true);
        SizeScaler sizeScaler = new SizeScaler(blurAlgorithm.ScaleFactor);
        if (sizeScaler.IsZeroSized(measuredWidth, measuredHeight))
        {
            // Will be initialized later when the View reports a size change
            initialized = false;
            blurView.SetWillNotDraw(true);
            ReleaseBitmap();
            return;
        }

        blurView.SetWillNotDraw(false);
        SizeScaler.Size bitmapSize = sizeScaler.scale(measuredWidth, measuredHeight);

        if (initialized && internalBitmap != null && !internalBitmap.IsRecycled &&
            internalBitmap.Width == bitmapSize.width && internalBitmap.Height == bitmapSize.height)
        {
            UpdateBlur();
            return;
        }

        ReleaseBitmap();
        internalBitmap = Bitmap.CreateBitmap(bitmapSize.width, bitmapSize.height, blurAlgorithm.SupportedBitmapConfig);
        internalCanvas = new BlurViewCanvas(internalBitmap);
        initialized = true;
        // Usually it's not needed, because `onPreDraw` updates the blur anyway.
        // But it handles cases when the PreDraw listener is attached to a different Window, for example
        // when the BlurView is in a Dialog window, but the root is in the Activity.
        // Previously it was done in `draw`, but it was causing potential side effects and Jetpack Compose crashes
        UpdateBlur();
    }

    void UpdateBlur()
    {
        if (destroyed || !blurEnabled || !initialized || internalBitmap == null || internalCanvas == null || rootView == null)
        {
            return;
        }

        if (frameClearDrawable == null)
        {
            internalBitmap.EraseColor(global::Android.Graphics.Color.Transparent);
        }
        else
        {
            frameClearDrawable.Draw(internalCanvas);
        }

        internalCanvas.Save();
        SetupInternalCanvasMatrix();
        rootView.Draw(internalCanvas);
        internalCanvas.Restore();

        BlurAndSave();
    }
    /**
    * Set up matrix to draw starting from blurView's position
    */
    private void SetupInternalCanvasMatrix()
    {
        rootView.GetLocationOnScreen(rootLocation);
        blurView.GetLocationOnScreen(blurViewLocation);

        int left = blurViewLocation[0] - rootLocation[0];
        int top = blurViewLocation[1] - rootLocation[1];

        // https://github.com/Dimezis/BlurView/issues/128
        float scaleFactorH = (float)blurView.Height / internalBitmap.Height;
        float scaleFactorW = (float)blurView.Width / internalBitmap.Width;

        float scaledLeftPosition = -left / scaleFactorW;
        float scaledTopPosition = -top / scaleFactorH;

        internalCanvas.Translate(scaledLeftPosition, scaledTopPosition);
        internalCanvas.Scale(1 / scaleFactorW, 1 / scaleFactorH);
    }

    public bool Draw(Canvas canvas)
    {
        if (destroyed || !blurEnabled || !initialized || internalBitmap == null)
        {
            return true;
        }
        // Not blurring itself or other BlurViews to not cause recursive draw calls
        // Related: https://github.com/Dimezis/BlurView/issues/110
        if (canvas is BlurViewCanvas) {
            return false;
        }

        // https://github.com/Dimezis/BlurView/issues/128
        float scaleFactorH = (float)blurView.Height / internalBitmap.Height;
        float scaleFactorW = (float)blurView.Width / internalBitmap.Width;

        canvas.Save();
        canvas.Scale(scaleFactorW, scaleFactorH);
        blurAlgorithm.Render(canvas, internalBitmap);
        canvas.Restore();
        if (overlayColor != TRANSPARENT)
        {
            canvas.DrawColor(overlayColor);
        }
        return true;
    }

    private void BlurAndSave()
    {
        var blurredBitmap = blurAlgorithm.Blur(internalBitmap, blurRadius);
        if (blurredBitmap != internalBitmap)
        {
            var previousBitmap = internalBitmap;
            internalBitmap = blurredBitmap;

            if (previousBitmap != null && !previousBitmap.IsRecycled)
            {
                previousBitmap.Recycle();
            }

            previousBitmap?.Dispose();
        }

        if (!blurAlgorithm.CanModifyBitmap)
        {
            internalCanvas.SetBitmap(internalBitmap);
        }
    }

    public void UpdateBlurViewSize()
    {
        int measuredWidth = blurView.MeasuredWidth;
        int measuredHeight = blurView.MeasuredHeight;

        Init(measuredWidth, measuredHeight);
    }

    public void Destroy()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;
        SetBlurAutoUpdate(false);
        blurAlgorithm.Destroy();
        ReleaseBitmap();
        frameClearDrawable = null;
        rootView = null;
        initialized = false;
        blurView.SetWillNotDraw(true);
    }

    public IBlurViewFacade SetBlurRadius(float radius)
    {
        this.blurRadius = radius;
        return this;
    }

    public IBlurViewFacade SetFrameClearDrawable(Drawable frameClearDrawable)
    {
        this.frameClearDrawable = frameClearDrawable;
        return this;
    }

    public IBlurViewFacade SetBlurEnabled(bool enabled)
    {
        this.blurEnabled = enabled;
        SetBlurAutoUpdate(enabled);
        blurView.Invalidate();
        return this;
    }

    public IBlurViewFacade SetBlurAutoUpdate(bool enabled)
    {
        var viewTreeObserver = rootView?.ViewTreeObserver;
        if (viewTreeObserver == null || !viewTreeObserver.IsAlive)
        {
            return this;
        }

        viewTreeObserver.RemoveOnPreDrawListener(drawListener);
        if (enabled)
        {
            viewTreeObserver.AddOnPreDrawListener(drawListener);
        }
        return this;
    }

    public IBlurViewFacade SetOverlayColor(Color overlayColor)
    {
        if (this.overlayColor != overlayColor)
        {
            this.overlayColor = overlayColor;
            blurView.Invalidate();
        }
        return this;
    }

    private void ReleaseBitmap()
    {
        internalCanvas?.Dispose();
        internalCanvas = null;

        if (internalBitmap != null && !internalBitmap.IsRecycled)
        {
            internalBitmap.Recycle();
        }

        internalBitmap?.Dispose();
        internalBitmap = null;
    }
}
