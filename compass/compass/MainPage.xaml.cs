using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace compass
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
	    }

	    protected override void OnAppearing()
	    {
	        base.OnAppearing();
	        ((CompassViewModel)BindingContext).StartCommand.Execute(null);
	    }

	    protected override void OnDisappearing()
	    {
	        base.OnDisappearing();
	        ((CompassViewModel)BindingContext).StopCommand.Execute(null);
	    }
    }
}
