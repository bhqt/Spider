﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/3/28 10:54:26.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Timers;
using Cdy.Spider;
using InSpiderDevelopWindow.ViewModel;

namespace InSpiderDevelopWindow
{
    public class MainViewModel : ViewModelBase, IProcessNotify
    {

        #region ... Variables  ...

        private ICommand mLoginCommand;

        private string mDatabase = string.Empty;

        private ICommand mSaveCommand;

        private ICommand mAddGroupCommand;
        
        private ICommand mRemoveGroupCommand;

        private ICommand mCancelCommand;

        private TreeItemViewModel mCurrentSelectTreeItem;

        private System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel> mTagGroup = new System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel>();

        private ViewModelBase mContentViewModel;

        private bool mIsCanOperate = true;

        private double mProcessNotify;

        private Visibility mNotifyVisiblity = Visibility.Hidden;

        private bool mIsLogin;

        private bool mIsDatabaseRunning;

        private System.Timers.Timer mCheckRunningTimer;

        private SpiderInfoViewModel infoModel;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        public MainViewModel()
        {
            ServiceLocator.Locator.Registor<IProcessNotify>(this);
            infoModel = new SpiderInfoViewModel();
            mContentViewModel = infoModel;

        }

        
        #endregion ...Constructor...

        #region ... Properties ...

        
        /// <summary>
        /// 
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if(mCancelCommand==null)
                {
                    mCancelCommand = new RelayCommand(() => { 
                        if(MessageBox.Show(Res.Get("canceltosavemsg"),"",MessageBoxButton.YesNo)== MessageBoxResult.Yes)
                        {
                           
                        }
                    },()=> { return !string.IsNullOrEmpty(mDatabase); });
                }
                return mCancelCommand;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public string MainwindowTitle
        {
            get
            {
                return string.IsNullOrEmpty(Database) ? Res.Get("MainwindowTitle"): Res.Get("MainwindowTitle")+"--"+this.Database;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Database
        {
            get
            {
                return mDatabase;
            }
            set
            {
                if (mDatabase != value)
                {
                    mDatabase = value;
                    OnPropertyChanged("Database");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public Visibility NotifyVisiblity
        {
            get
            {
                return mNotifyVisiblity;
            }
            set
            {
                if (mNotifyVisiblity != value)
                {
                    mNotifyVisiblity = value;
                    OnPropertyChanged("NotifyVisiblity");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public double ProcessNotify
        {
            get
            {
                return mProcessNotify;
            }
            set
            {
                if (mProcessNotify != value)
                {
                    mProcessNotify = value;
                    OnPropertyChanged("ProcessNotify");
                    OnPropertyChanged("ProcessNotifyPercent");
                }
            }
        }

        public double ProcessNotifyPercent
        {
            get
            {
                return mProcessNotify / 100;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsCanOperate
        {
            get
            {
                return mIsCanOperate;
            }
            set
            {
                if (mIsCanOperate != value)
                {
                    mIsCanOperate = value;
                    OnPropertyChanged("IsCanOperate");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModelBase ContentViewModel
        {
            get
            {
                return mContentViewModel;
            }
            set
            {
                if(mContentViewModel!=value)
                {
                    mContentViewModel = value;
                    OnPropertyChanged("ContentViewModel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel> TagGroup
        {
            get
            {
                return mTagGroup;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public TreeItemViewModel CurrentSelectGroup
        {
            get
            {
                return mCurrentSelectTreeItem;
            }
            set
            {
                if (mCurrentSelectTreeItem != value)
                {
                    mCurrentSelectTreeItem = value;
                   
                    OnPropertyChanged("CurrentSelectGroup");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand AddGroupCommand
        {
            get
            {
                if (mAddGroupCommand == null)
                {
                    mAddGroupCommand = new RelayCommand(() => {
                        (CurrentSelectGroup).AddCommand.Execute(null);
                        //  NewGroup();
                    },()=> { return mCurrentSelectTreeItem != null && mCurrentSelectTreeItem.CanAddChild(); });
                }
                return mAddGroupCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveGroupCommand
        {
            get
            {
                if (mRemoveGroupCommand == null)
                {
                    mRemoveGroupCommand = new RelayCommand(() => {
                        (CurrentSelectGroup).RemoveCommand.Execute(null);
                    },()=> { return CurrentSelectGroup != null && CurrentSelectGroup.CanRemove() ; });
                }
                return mRemoveGroupCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if(mSaveCommand==null)
                {
                    mSaveCommand = new RelayCommand(() => {

                        
                    }, () => { return string.IsNullOrEmpty(Database); });
                }
                return mSaveCommand;
            }
        }

        




        #endregion ...Properties...

        #region ... Methods    ...

        

        

       

        /// <summary>
        /// 
        /// </summary>
        public void BeginShowNotify()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {

                NotifyVisiblity = Visibility.Visible;
            }));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void ShowNotifyValue(double val)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {
                if (val > 100)
                {
                    ProcessNotify = 100;
                }
                else
                {
                    ProcessNotify = val;
                }
            }));
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndShowNotify()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {
                NotifyVisiblity = Visibility.Hidden;
                ProcessNotify = 0;
            }));
           
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public interface ITagGroupAdd
    //{
    //    bool AddGroup(string parent);
    //}

    /// <summary>
    /// 
    /// </summary>
    public interface IProcessNotify
    {
        /// <summary>
        /// 
        /// </summary>
        void BeginShowNotify();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        void ShowNotifyValue(double val);
        /// <summary>
        /// 
        /// </summary>
        void EndShowNotify();
    }


}
