using System;
using Windows.UI.Xaml.Controls;

namespace BandCentral.Uwp.Common
{
	public class FrameService
	{
		readonly Frame frame;

		public FrameService(Frame frame)
		{
			this.frame = frame;
		}

		public void GoBack()
		{
			frame.GoBack();
		}

		public void GoForward()
		{
			frame.GoForward();
		}

		public bool Navigate<T>(object parameter = null)
		{
			var type = typeof(T);

			return Navigate(type, parameter);
		}

		public bool Navigate(Type source, object parameter = null)
		{
			return frame.Navigate(source, parameter);
		}
	}

}
