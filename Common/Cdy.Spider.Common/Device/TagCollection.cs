﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/4 15:36:55.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Cdy.Spider
{
    /// <summary>
    /// 
    /// </summary>
    public class TagCollection:SortedDictionary<int,Tagbae>
    {

        #region ... Variables  ...

        private Dictionary<string, Tagbae> mNamedTags = new Dictionary<string, Tagbae>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public int MaxId { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> TagNames { get { return mNamedTags.Keys; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool UpdateOrAdd(Tagbae tag)
        {
            if(ContainsKey(tag.Id))
            {
                this[tag.Id] = tag;
            }
            else if(tag.Id>-1)
            {
                this.Add(tag.Id, tag);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public bool AddTag(Tagbae tag)
        {
            if(!this.ContainsKey(tag.Id) && !mNamedTags.ContainsKey(tag.Name))
            {
                Add(tag.Id, tag);
                mNamedTags.Add(tag.Name, tag);
                MaxId = Math.Max(tag.Id, MaxId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public bool AppendTag(Tagbae tag)
        {
            if(!mNamedTags.ContainsKey(tag.Name))
            {
                tag.Id = MaxId++;
                Add(tag.Id, tag);
                mNamedTags.Add(tag.Name, tag);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public bool RemoveTag(Tagbae tag)
        {
            if(this.ContainsKey(tag.Id))
            {
                this.Remove(tag.Id);
            }
            if(mNamedTags.ContainsKey(tag.Name))
            {
                mNamedTags.Remove(tag.Name);
            }
            return true;
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
