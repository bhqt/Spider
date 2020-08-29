﻿//==============================================================
//  Copyright (C) 2020 Chongdaoyang Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/25 20:16:01 .
//  Version 1.0
//  CDYWORK
//==============================================================

using Cdy.Spider;
using InSpiderDevelop;
using InSpiderDevelop.Device;
using InSpiderDevelopWindow.ViewModel;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Input;

namespace InSpiderDevelopWindow
{

    /// <summary>
    /// 
    /// </summary>
    public class DeviceGroupViewModel : HasChildrenTreeItemViewModel
    {

        #region ... Variables  ...
        
        private DeviceGroup mModel;

        /// <summary>
        /// 
        /// </summary>
        private DeviceDocument mDocument;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...



        /// <summary>
        /// 
        /// </summary>
        public virtual DeviceDocument Document
        {
            get
            {
                return mDocument;
            }
            set
            {
                mDocument = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceGroup Model
        {
            get
            {
                return mModel;
            }
            set
            {
                if (mModel != value)
                {
                    mModel = value;
                    mName = value.Name;
                    Init();
                    OnPropertyChanged("Model");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string FullName => mModel.FullName;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Init()
        {
            var vcount = mDocument.ListDevice(this.FullName).Count + mDocument.ListDeviceGroup(this.FullName).Count;
            if (vcount > 0)
                this.PreLoadChildForExpend(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddChild()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddGroup()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Remove()
        {
            mDocument.RemoveGroup(this.FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool OnRename(string oldName, string newName)
        {
            return mDocument.ChangeGroupName(this.FullName, newName);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadData()
        {
            foreach (var vv in mDocument.ListDeviceGroup(this.FullName))
            {
                var vmm = new DeviceGroupViewModel() {  Document = this.Document, Model = vv };
                Children.Add(vmm);
            }

            foreach (var vv in mDocument.ListDevice(this.FullName))
            {
                var vvv = new DeviceTreeViewModel() {  Document = this.Document, Model = vv };
                Children.Add(vvv);
            }
        }

       

        /// <summary>
        /// 
        /// </summary>
        public override void AddGroup()
        {
            string sname = mDocument.GetAvaiableGroupName("Group");
            var vgd = new DeviceGroup() { Parent = this.mModel, Name = sname };
            if(mDocument.AddDeviceGroup(this.Model,vgd))
            {
                var vmm = new DeviceGroupViewModel() {  Document = this.Document, Model = vgd };
                this.Children.Add(vmm);
                vmm.IsSelected = true;
                vmm.IsEdit = true;
            }
            this.IsExpanded = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Add()
        {
            string sname = DeviceDocument.Manager.GetAvaiableName("Device");
            var vd = new DeviceDevelop() { Data = new DeviceData() ,Group = this.FullName};
            vd.Name = sname;
            if (mDocument.AddDevice(vd))
            {
                var vmm = new DeviceTreeViewModel() { Document=this.Document, Model = vd };
                this.Children.Add(vmm);
                vmm.IsSelected = true;
                vmm.IsEdit = true;
            }
            this.IsExpanded = true;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class DeviceRootViewModel: DeviceGroupViewModel
    {



        /// <summary>
        /// 
        /// </summary>
        public override string Name { get => Res.Get("Device"); set => base.Name = value; }

        /// <summary>
        /// 
        /// </summary>
        public override string FullName => string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public override DeviceDocument Document { get { return base.Document; } set{ base.Document = value; base.Init(); } }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddChild()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddGroup()
        {
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanRemove()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanCopy()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanReName()
        {
            return false;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class DeviceTreeViewModel : TreeItemViewModel
    {

        #region ... Variables  ...
       
        private IDeviceDevelop mModel;
        private DeviceDocument mDocument;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        public virtual DeviceDocument Document
        {
            get
            {
                return mDocument;
            }
            set
            {
                mDocument = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IDeviceDevelop Model
        {
            get
            {
                return mModel;
            }
            set
            {
                if (mModel != value)
                {
                    mModel = value;
                    mName = value.Name;
                    OnPropertyChanged("Model");
                }
            }
        }



        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override ViewModelBase GetModel(ViewModelBase mode)
        {
            if (mode is DeviceDetailViewModel)
            {
                (mode as DeviceDetailViewModel).Model = this.Model;
                return mode;
            }
            else
            {
                return new DeviceDetailViewModel() { Model = this.Model };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool OnRename(string oldName, string newName)
        {
            return mDocument.ReName(this.Model, newName);
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class APITreeViewModel : TreeItemViewModel
    {

        #region ... Variables  ...
        private IApiDevelop mModel;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public IApiDevelop Model
        {
            get
            {
                return mModel;
            }
            set
            {
                if (mModel != value)
                {
                    mModel = value;
                    OnPropertyChanged("Model");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Name { get => Res.Get("API"); set => base.Name = value; }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddChild()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanCopy()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanPaste()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanReName()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanRemove()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override ViewModelBase GetModel(ViewModelBase mode)
        {
            if (mode is APIDetailViewModel)
            {
                (mode as APIDetailViewModel).Model = this.Model;
                return mode;
            }
            else
            {
                return new APIDetailViewModel() { Model = this.Model };
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

   

}
