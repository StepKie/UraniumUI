﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UraniumApp.Pages.DataGrids;
public class SelectableDataGridPageViewModel : CustomDataGridPageViewModel
{
    public ObservableCollection<Student> SelectedItems { get; set; } = new ObservableCollection<Student>();

    public ICommand RemoveSelectedCommand { get; set; }

    public SelectableDataGridPageViewModel() : base()
    {
        RemoveSelectedCommand = new Command(() =>
        {
            foreach (var item in SelectedItems)
            {
                Items.Remove(item);
            }
        });
    }

    protected override bool SimulateNetwork => false;
}
