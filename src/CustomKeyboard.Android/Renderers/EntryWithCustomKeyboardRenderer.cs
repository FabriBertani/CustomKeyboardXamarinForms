using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using CustomKeyboard;
using CustomKeyboard.Droid.Renderers;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.InputMethodServices.KeyboardView;

[assembly: ExportRenderer(typeof(EntryWithCustomKeyboard), typeof(EntryWithCustomKeyboardRenderer))]
namespace CustomKeyboard.Droid.Renderers
{
    public class EntryWithCustomKeyboardRenderer : EntryRenderer, IOnKeyboardActionListener
    {
        private Context context;

        private EntryWithCustomKeyboard entryWithCustomKeyboard;

        private Android.InputMethodServices.KeyboardView mKeyboardView;
        private Android.InputMethodServices.Keyboard mKeyboard;

        private InputTypes inputTypeToUse;

        private bool keyPressed, capsMode;

        public EntryWithCustomKeyboardRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            var newCustomEntryKeyboard = e.NewElement as EntryWithCustomKeyboard;
            var oldCustomEntryKeyboard = e.OldElement as EntryWithCustomKeyboard;

            if (newCustomEntryKeyboard == null && oldCustomEntryKeyboard == null)
                return;

            if (e.NewElement != null)
            {
                this.entryWithCustomKeyboard = newCustomEntryKeyboard;
                this.CreateCustomKeyboard();

                this.inputTypeToUse = this.entryWithCustomKeyboard.Keyboard.ToInputType() | InputTypes.TextFlagNoSuggestions;

                // Here we set the EditText event handlers
                this.EditText.FocusChange += Control_FocusChange;
                this.EditText.TextChanged += EditText_TextChanged;
                this.EditText.Click += EditText_Click;
                this.EditText.Touch += EditText_Touch;
            }

            // Dispose control
            if (e.OldElement != null)
            {
                this.EditText.FocusChange -= Control_FocusChange;
                this.EditText.TextChanged -= EditText_TextChanged;
                this.EditText.Click -= EditText_Click;
                this.EditText.Touch -= EditText_Touch;
            }
        }

        protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
        {
            e.Result = true;

            if (e.Focus)
                this.Control.RequestFocus();
            else
                this.Control.ClearFocus();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var keys = this.mKeyboard.Keys;

            foreach (var key in keys)
            {
                if (key.Codes[0] == (int)Keycode.Enter)
                {
                    var dr = (Drawable)this.context.GetDrawable(Resource.Drawable.enter_arrow_large);

                    dr.SetBounds(key.X, key.Y, key.X + key.Width, key.Y + key.Height);
                    dr.Draw(canvas);
                }
            }
        }

        #region EditText event handlers

        private void Control_FocusChange(object sender, FocusChangeEventArgs e)
        {
            // Workaround to avoid null reference exceptions in runtime
            if (this.EditText.Text == null)
                this.EditText.Text = string.Empty;

            if (e.HasFocus)
            {
                this.mKeyboardView.OnKeyboardActionListener = this;

                if (this.Element.Keyboard == Keyboard.Text)
                    this.CreateCustomKeyboard();

                this.ShowKeyboardWithAnimation();
            }
            else
            {
                // When the control looses focus, we set an empty listener to avoid crashes
                this.mKeyboardView.OnKeyboardActionListener = new NullListener();

                this.HideKeyboardView();
            }
        }

        private void EditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            // Ensure no key is pressed to clear focus
            if (this.EditText.Text.Length != 0 && !this.keyPressed)
            {
                this.EditText.ClearFocus();
                return;
            }
        }

        private void EditText_Click(object sender, System.EventArgs e)
        {
            ShowKeyboardWithAnimation();
        }

        private void EditText_Touch(object sender, TouchEventArgs e)
        {
            this.EditText.InputType = InputTypes.Null;

            this.EditText.OnTouchEvent(e.Event);

            this.EditText.InputType = this.inputTypeToUse;

            e.Handled = true;
        }

        #endregion

        #region keyboard related

        private void CreateCustomKeyboard()
        {
            var activity = (Activity)this.context;

            var rootView = activity.Window.DecorView.FindViewById(Android.Resource.Id.Content);
            var activityRootView = (ViewGroup)((ViewGroup)rootView).GetChildAt(0);

            this.mKeyboardView = activityRootView.FindViewById<Android.InputMethodServices.KeyboardView>(Resource.Id.customKeyboard);

            // If the previous line fails, it means the keyboard needs to be created and added
            if (this.mKeyboardView == null)
            {
                this.mKeyboardView = (Android.InputMethodServices.KeyboardView)activity.LayoutInflater.Inflate(Resource.Layout.CustomKeyboard, null);
                this.mKeyboardView.Id = Resource.Id.customKeyboard;
                this.mKeyboardView.Focusable = true;
                this.mKeyboardView.FocusableInTouchMode = true;

                this.mKeyboardView.Release += (sender, e) => { };

                var layoutParams = new Android.Widget.RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
                layoutParams.AddRule(LayoutRules.AlignParentBottom);
                activityRootView.AddView(this.mKeyboardView, layoutParams);
            }

            this.HideKeyboardView();

            this.mKeyboard = new Android.InputMethodServices.Keyboard(this.context, Resource.Xml.special_keyboard);

            this.SetCurrentKeyboard();
        }

        private void SetCurrentKeyboard()
        {
            this.mKeyboardView.Keyboard = this.mKeyboard;
        }

        // Method to show our custom keyboard
        private void ShowKeyboardWithAnimation()
        {
            // First we must ensure that custom keyboard is hidden to
            // prevent showing it multiple times
            if (this.mKeyboardView.Visibility == ViewStates.Gone)
            {
                // Ensure native keyboard is hidden
                var imm = (InputMethodManager)this.context.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(this.EditText.WindowToken, 0);

                this.EditText.InputType = InputTypes.Null;

                var animation = AnimationUtils.LoadAnimation(this.context, Resource.Animation.slide_in_bottom);
                this.mKeyboardView.Animation = animation;

                this.mKeyboardView.Enabled = true;

                // Show custom keyboard with animation
                this.mKeyboardView.Visibility = ViewStates.Visible;
            }
        }

        // Method to hide our custom keyboard
        private void HideKeyboardView()
        {
            this.mKeyboardView.Visibility = ViewStates.Gone;
            this.mKeyboardView.Enabled = false;

            this.EditText.InputType = InputTypes.Null;
        }

        private void ChangeKeyboardView(int keyboardView)
        {
            this.HideKeyboardView();

            this.mKeyboard = new Android.InputMethodServices.Keyboard(this.context, keyboardView);

            this.SetCurrentKeyboard();

            this.ShowKeyboardWithAnimation();
        }

        #endregion

        // Implementing IOnKeyboardActionListener interface
        public void OnKey([GeneratedEnum] Keycode primaryCode, [GeneratedEnum] Keycode[] keyCodes)
        {
            if (!this.EditText.IsFocused)
                return;

            // Ensure key is pressed to avoid removing focus
            this.keyPressed = true;

            // Create event for key press
            long eventTime = JavaSystem.CurrentTimeMillis();

            var ev = new KeyEvent(eventTime, eventTime, KeyEventActions.Down, primaryCode, 0, 0, 0, 0,
                                  KeyEventFlags.SoftKeyboard | KeyEventFlags.KeepTouchMode);

            // Ensure native keyboard is hidden
            var imm = (InputMethodManager)this.context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(this.EditText.WindowToken, HideSoftInputFlags.None);

            this.EditText.InputType = this.inputTypeToUse;

            switch(ev.KeyCode)
            {
                case Keycode.ShiftLeft:
                case Keycode.ShiftRight:
                    this.capsMode = !this.capsMode;

                    this.mKeyboard = new Android.InputMethodServices.Keyboard(this.context, Resource.Xml.advance_keyboard);

                    this.mKeyboardView.SetShifted(this.capsMode);
                    this.mKeyboardView.InvalidateAllKeys();
                    this.SetCurrentKeyboard();
                    break;
                case Keycode.ButtonY:
                    this.ChangeKeyboardView(Resource.Xml.advance_keyboard);                    
                    break;
                case Keycode.ButtonZ:
                    this.ChangeKeyboardView(Resource.Xml.special_keyboard);
                    break;
                case Keycode.Enter:
                    // Sometimes EditText takes long to update the HasFocus status
                    if (this.EditText.HasFocus)
                    {
                        // Close the keyboard, remove focus and launch command asociated action
                        this.HideKeyboardView();

                        this.ClearFocus();

                        this.entryWithCustomKeyboard.EnterCommand?.Execute(null);
                    }

                    break;
            }

            // Set the cursor at the end of the text
            this.EditText.SetSelection(this.EditText.Text.Length);

            if (this.EditText.HasFocus)
            {
                if (this.capsMode)
                    this.EditText.SetFilters(new IInputFilter[] { new InputFilterAllCaps() });
                else
                {
                    this.EditText.SetFilters(new IInputFilter[] { });
                    //this.EditText.InputType = this.inputTypeToUse;
                }

                this.DispatchKeyEvent(ev);

                this.keyPressed = false;
            }
        }        

        public void OnPress([GeneratedEnum] Keycode primaryCode)
        {
        }

        public void OnRelease([GeneratedEnum] Keycode primaryCode)
        {
        }

        public void OnText(ICharSequence text)
        {
        }

        public void SwipeDown()
        {
        }

        public void SwipeLeft()
        {
        }

        public void SwipeRight()
        {
        }

        public void SwipeUp()
        {
        }

        private class NullListener : Object, IOnKeyboardActionListener
        {
            public void OnKey([GeneratedEnum] Keycode primaryCode, [GeneratedEnum] Keycode[] keyCodes)
            {
            }

            public void OnPress([GeneratedEnum] Keycode primaryCode)
            {
            }

            public void OnRelease([GeneratedEnum] Keycode primaryCode)
            {
            }

            public void OnText(ICharSequence text)
            {
            }

            public void SwipeDown()
            {
            }

            public void SwipeLeft()
            {
            }

            public void SwipeRight()
            {
            }

            public void SwipeUp()
            {
            }
        }
    }
}