using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace HDictInduction.Console
{
	public class Lightbox : Window {
		public static readonly DependencyProperty WindowIntroAnimationProperty = DependencyProperty.Register("WindowIntroAnimation", typeof(Storyboard), typeof(Lightbox));
		public static readonly DependencyProperty WindowOutroAnimationProperty = DependencyProperty.Register("WindowOutroAnimation", typeof(Storyboard), typeof(Lightbox));
		public static readonly DependencyProperty DialogIntroAnimationProperty = DependencyProperty.Register("DialogIntroAnimation", typeof(Storyboard), typeof(Lightbox));
		public static readonly DependencyProperty DialogOutroAnimationProperty = DependencyProperty.Register("DialogOutroAnimation", typeof(Storyboard), typeof(Lightbox));
		public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register("IsDraggable", typeof(bool), typeof(Lightbox), new PropertyMetadata(true));

		// Helper command, makes it easy to route a button click to close.
		public static readonly RoutedCommand CloseCommand = new RoutedCommand();

		private bool hasBeenActivated = false;
		private bool hasBeenClosed = false;

		// Count for #of storyboards to wait for to actually close the dialog
		private int storyboardWaitCount = 0;

		static Lightbox() {
			// Defaults for expected uses- transparent, borderless, not in taskbar.
			Lightbox.AllowsTransparencyProperty.OverrideMetadata(typeof(Lightbox), new FrameworkPropertyMetadata(true));
			Lightbox.WindowStyleProperty.OverrideMetadata(typeof(Lightbox), new FrameworkPropertyMetadata(WindowStyle.None));
			Lightbox.ShowInTaskbarProperty.OverrideMetadata(typeof(Lightbox), new FrameworkPropertyMetadata(false));
		}

		public Lightbox() {
			this.CommandBindings.Add(new CommandBinding(Lightbox.CloseCommand, this.HandleCloseCommand));

			this.Owner = Application.Current.MainWindow;
			this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		/// <summary>
		/// Animation run on owner window when this dialog is shown.
		/// </summary>
		public Storyboard WindowIntroAnimation {
			get { return (Storyboard)this.GetValue(Lightbox.WindowIntroAnimationProperty); }
			set { this.SetValue(Lightbox.WindowIntroAnimationProperty, value); }
		}

		/// <summary>
		/// Animation run on owner window when this dialog is closed.
		/// </summary>
		public Storyboard WindowOutroAnimation {
			get { return (Storyboard)this.GetValue(Lightbox.WindowOutroAnimationProperty); }
			set { this.SetValue(Lightbox.WindowOutroAnimationProperty, value); }
		}

		/// <summary>
		/// Animation run on this when this dialog is shown.
		/// </summary>
		public Storyboard DialogIntroAnimation {
			get { return (Storyboard)this.GetValue(Lightbox.DialogIntroAnimationProperty); }
			set { this.SetValue(Lightbox.DialogIntroAnimationProperty, value); }
		}

		/// <summary>
		/// Animation run on this when this dialog is closed.
		/// </summary>
		public Storyboard DialogOutroAnimation {
			get { return (Storyboard)this.GetValue(Lightbox.DialogOutroAnimationProperty); }
			set { this.SetValue(Lightbox.DialogOutroAnimationProperty, value); }
		}

		/// <summary>
		/// Automatic handling for MouseDrag.
		/// On by default since this doesn't have a titlebar.
		/// </summary>
		public bool IsDraggable {
			get { return (bool)this.GetValue(Lightbox.IsDraggableProperty); }
			set { this.SetValue(Lightbox.IsDraggableProperty, value); }
		}

		/// <summary>
		/// Called each time the window becomes the active window.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnActivated(System.EventArgs e) {
			base.OnActivated(e);

			// Since there is no Opened event, just trigger intro animations
			// on first activation.
			if (!this.hasBeenActivated) {
				this.hasBeenActivated = true;

				Storyboard windowAnim = this.WindowIntroAnimation;

				windowAnim.Begin(this.Host);

				Storyboard dialogAnim = this.DialogIntroAnimation;
				if (dialogAnim != null)
					dialogAnim.Begin(this);
			}
		}

		/// <summary>
		/// For auto-handling DragMove, for simplicity.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);

			if (this.IsDraggable)
				this.DragMove();
		}

		/// <summary>
		/// Handler for the close command.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HandleCloseCommand(object sender, ExecutedRoutedEventArgs e) {
			e.Handled = true;
			this.Close();
		}

		/// <summary>
		/// First time this is called the operation will be aborted to allow
		/// for the outro animations to be run. Once all outros are completed,
		/// the window is closed for real.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnClosing(CancelEventArgs e) {
             //Abort the first close to allow the outro animations to run.
            if (!this.hasBeenClosed) {
                e.Cancel = true;

                this.hasBeenClosed = true;

                Storyboard windowAnim = this.WindowOutroAnimation;
                if (windowAnim != null) {
                    Window host = this.Host;

                    // Add a handler so we know when the dialog can be closed.
                    windowAnim.Completed += this.HandleWindowCloseAnimationCompleted;

                    windowAnim.Begin(host);
                    ++this.storyboardWaitCount;
                }

                Storyboard dialogAnim = this.DialogOutroAnimation;
                if (dialogAnim != null) {

                    // Add a handler so we know when the dialog can be closed.
                    dialogAnim.Completed += this.HandleDialogCloseAnimationCompleted;
                    dialogAnim.Begin(this);

                    ++this.storyboardWaitCount;
                }

                if (this.storyboardWaitCount == 0)
                    e.Cancel = false;
            }
			base.OnClosing(e);
		}

		// Handler for the window close animation completion, this delays
		// actually closing the window until all outro animations have completed.
		private void HandleWindowCloseAnimationCompleted(object sender, EventArgs e) {
			this.WindowOutroAnimation.Completed -= this.HandleWindowCloseAnimationCompleted;
			--this.storyboardWaitCount;

			if (this.storyboardWaitCount == 0)
				this.Close();
		}

		// Handler for the window close animation completion, this delays
		// actually closing the window until all outro animations have completed.
		private void HandleDialogCloseAnimationCompleted(object sender, EventArgs e) {
			this.DialogOutroAnimation.Completed -= this.HandleDialogCloseAnimationCompleted;
			--this.storyboardWaitCount;

			if (this.storyboardWaitCount == 0)
				this.Close();
		}

		// Window this is attached to, will center on this window and will play
		// the Window Intro/Outro on it.
		private Window Host {
			get {
				if (this.Owner != null)
					return this.Owner;
				return Application.Current.MainWindow;
			}
		}

	}
}
