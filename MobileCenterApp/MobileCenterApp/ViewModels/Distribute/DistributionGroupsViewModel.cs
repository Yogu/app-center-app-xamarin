﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileCenterApp
{
	public class DistributionGroupsViewModel : BaseViewModel
	{
		public DistributionGroupsViewModel()
		{
			Title = "Distribution";
			Icon = Images.DistributePageIcon;
			DeleteCommand = new Command(async (obj) =>
			{
				try
				{
					var distribution = ((MenuItem)obj).CommandParameter as DistributionGroup;
					var success = await SyncManager.Shared.Delete(distribution);
					if (!success)
						await App.Current.MainPage.DisplayActionSheet("Error deleting the app. Please try again", "Ok", null);
					else
						Items.ResfreshData();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
			});
		}

		SimpleDatabaseSource<DistributionGroup> items = new SimpleDatabaseSource<DistributionGroup>(Database.Main);
		public SimpleDatabaseSource<DistributionGroup> Items
		{
			get { return items; }
			set { ProcPropertyChanged(ref items, value); }
		}

		public AppClass CurrentApp { get; set; }

		public override async Task OnRefresh()
		{
			var currentId = Settings.CurrentApp;
			if (string.IsNullOrWhiteSpace(currentId))
				return;
			CurrentApp = Database.Main.GetObject<AppClass>(currentId);
			SetGroupInfo();
			await Task.Delay(1999);
			await SyncManager.Shared.SyncDistributionGroups(CurrentApp);
			SetGroupInfo();

		}
		void SetGroupInfo()
		{
			var groupInfo = Database.Main.GetGroupInfo<DistributionGroup>().Clone();
			groupInfo.OrderByDesc = true;
			groupInfo.Filter = $"AppId = ?";
			groupInfo.Params = CurrentApp?.Id;
			Items.GroupInfo = groupInfo;
		}

		public ICommand CreateCommand { get; private set; } = new Command(async (obj) =>
		{
			try
			{
				await NavigationService.PushModalAsync(new CreateDistributionGroupViewModel());
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		});

		public ICommand DeleteCommand { get; private set; }

	}
}
