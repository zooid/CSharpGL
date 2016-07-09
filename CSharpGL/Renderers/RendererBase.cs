﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;


namespace CSharpGL
{
    /// <summary>
    /// 用OpenGL初始化和渲染一个元素。
    /// 只做初始化和渲染这两件事。
    /// <para>Initialize and render something.</para>
    /// </summary>
    [Editor(typeof(PropertyGridEditor), typeof(UITypeEditor))]
    public abstract class RendererBase : IRenderable, IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 为便于调试而设置的ID值，没有应用意义。
        /// <para>Only for debugging.</para>
        /// </summary>
        public int ID { get; private set; }

        private static int idCounter = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0}]: [{1}]", this.ID, this.Name);
        }

        /// <summary>
        /// 用OpenGL初始化和渲染一个元素。
        /// 只做初始化和渲染这两件事。
        /// <para>Initialize and render something.</para>
        /// </summary>
        public RendererBase()
        {
            this.ID = idCounter++;
        }

        private bool initialized = false;

        /// <summary>
        /// 
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// 初始化此Element，此方法应且只应执行1次。
        /// </summary>
        public void Initialize()
        {
            if (!initialized)
            {
                DoInitialize();

                initialized = true;
            }
        }

        /// <summary>
        /// 初始化此Element，此方法应且只应执行1次。
        /// </summary>
        protected abstract void DoInitialize();

        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="arg"></param>
        public void Render(RenderEventArg arg)
        {
            if (!initialized) { Initialize(); }

            DoRender(arg);
        }

        /// <summary>
        /// 执行渲染操作
        /// </summary>
        /// <param name="arg"></param>
        protected abstract void DoRender(RenderEventArg arg);


        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        } // end sub

        /// <summary>
        /// Destruct instance of the class.
        /// </summary>
        ~RendererBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Backing field to track whether Dispose has been called.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Dispose managed and unmanaged resources of this instance.
        /// </summary>
        /// <param name="disposing">If disposing equals true, managed and unmanaged resources can be disposed. If disposing equals false, only unmanaged resources can be disposed. </param>
        private void Dispose(bool disposing)
        {

            if (this.disposedValue == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    DisposeManagedResources();
                } // end if

                // Dispose unmanaged resources.
                DisposeUnmanagedResources();
            } // end if

            this.disposedValue = true;
        } // end sub

        #endregion

        /// <summary>
        /// 释放.net托管资源。
        /// </summary>
        protected virtual void DisposeManagedResources() { }

        /// <summary>
        /// 释放.net非托管资源，例如释放OpenGL相关的资源。
        /// 此类型用于OpenGL渲染，因此必有要释放的OpenGL资源。因此设置为abstract。
        /// </summary>
        protected abstract void DisposeUnmanagedResources();

    }

}
