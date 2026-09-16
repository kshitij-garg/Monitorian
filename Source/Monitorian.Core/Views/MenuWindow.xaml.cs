using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Monitorian.Core.Models;
using Monitorian.Core.ViewModels;
using Monitorian.Core.Views.Controls;
using ScreenFrame.Movers;

namespace Monitorian.Core.Views;

public partial class MenuWindow : Window
{
	private readonly Point _pivot;
	private readonly FloatWindowMover _mover;
	private readonly AppControllerCore _controller;
	public MenuWindowViewModel ViewModel => (MenuWindowViewModel)this.DataContext;

	public MenuWindow(AppControllerCore controller, Point pivot)
	{
		this._pivot = pivot;
		LanguageService.Switch();

		InitializeComponent();

		this._controller = controller;
		var viewModel = new MenuWindowViewModel(controller);
		viewModel.LanguageChanged += OnLanguageChanged;
		this.DataContext = viewModel;

		_mover = new FloatWindowMover(this, pivot);
		_mover.ForegroundWindowChanged += OnDeactivated;
		_mover.AppDeactivated += OnDeactivated;

		controller.WindowPainter.Add(this);
	}

	private async void OnLanguageChanged(object sender, EventArgs e)
	{
		if (_isClosing)
			return;

		_mover.ForegroundWindowChanged -= OnDeactivated;
		_mover.AppDeactivated -= OnDeactivated;

		await Task.Delay(50);

		if (_isClosing)
			return;

		this.Close();
		_controller.ShowMenuWindow(_pivot);
	}

	private void LanguageComboBox_DropDownOpened(object sender, EventArgs e)
	{
		DepartFromForeground();
	}

	private void LanguageComboBox_DropDownClosed(object sender, EventArgs e)
	{
		ReturnToForeground();
	}

	public UIElementCollection HeadSection => this.HeadItems.Children;
	public UIElementCollection MenuSectionTop => this.MenuItemsTop.Children;
	public UIElementCollection MenuSectionMiddle => this.MenuItemsMiddle.Children;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		FlowElement.EnsureFlowDirection(this);
	}

	private void InvertScrollDirection_Click(object sender, RoutedEventArgs e)
	{
		if (sender is ButtonBase button)
		{
			var topLeft = button.PointToScreen(new Point(0, 0));
			var bottomRight = button.PointToScreen(new Point(button.ActualWidth, button.ActualHeight));
			var pivot = new Rect(topLeft, bottomRight);

			DepartFromForeground();

			var scrollWindow = new ScrollWindow(_controller, pivot);
			scrollWindow.Closed += OnClosed;
			scrollWindow.Show();
		}

		void OnClosed(object sender, EventArgs e)
		{
			((Window)sender).Closed -= OnClosed;
			ReturnToForeground();
		}
	}

	#region Show/Close

	public void DepartFromForeground()
	{
		this.Topmost = false;
	}

	public async void ReturnToForeground()
	{
		// Wait for this window to be able to be activated.
		await Task.Delay(TimeSpan.FromMilliseconds(100));

		if (_isClosing)
			return;

		// Activate this window. This is necessary to assure this window is foreground.
		this.Activate();

		this.Topmost = true;
	}

	private bool _isClosing = false;

	private void OnDeactivated(object sender, EventArgs e)
	{
		if (!_isClosing && this.IsLoaded)
			this.Close();
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);

		if (!this.Topmost)
			return;

		if (!_isClosing)
			this.Close();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		if (!e.Cancel)
		{
			_isClosing = true;
			ViewModel.Dispose();
		}

		base.OnClosing(e);
	}

	#endregion
}