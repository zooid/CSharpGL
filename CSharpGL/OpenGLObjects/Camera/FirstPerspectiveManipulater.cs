﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpGL
{
    /// <summary>
    /// 用鼠标旋转模型。
    /// </summary>
    public class FirstPerspectiveManipulater : CameraManipulater
    {
        private ICamera camera;
        private Control canvas;

        private char upcaseFrontKey;
        private char upcaseBackKey;
        private char upcaseLeftKey;
        private char upcaseRightKey;
        private char frontKey;
        private char backKey;
        private char leftKey;
        private char rightKey;

        public char FrontKey
        {
            get { return frontKey; }
            set
            {
                frontKey = value.ToString().ToLower()[0];
                upcaseFrontKey = value.ToString().ToUpper()[0];
            }
        }

        public char BackKey
        {
            get { return backKey; }
            set
            {
                backKey = value.ToString().ToLower()[0];
                upcaseBackKey = value.ToString().ToUpper()[0];
            }
        }

        public char LeftKey
        {
            get { return leftKey; }
            set
            {
                leftKey = value.ToString().ToLower()[0];
                upcaseLeftKey = value.ToString().ToUpper()[0];
            }
        }

        public char RightKey
        {
            get { return rightKey; }
            set
            {
                rightKey = value.ToString().ToLower()[0];
                upcaseRightKey = value.ToString().ToUpper()[0];
            }
        }

        public float StepLength { get; set; }

        private KeyPressEventHandler keyPressEvent;
        private MouseEventHandler mouseDownEvent;
        private MouseEventHandler mouseMoveEvent;
        private MouseEventHandler mouseUpEvent;

        public FirstPerspectiveManipulater()
        {
            this.FrontKey = 'w';
            this.BackKey = 's';
            this.LeftKey = 'a';
            this.RightKey = 'd';

            this.StepLength = 0.1f;

            this.keyPressEvent = new KeyPressEventHandler(this.canvas_KeyPress);
            this.mouseDownEvent = new MouseEventHandler(this.canvas_MouseDown);
            this.mouseMoveEvent = new MouseEventHandler(this.canvas_MouseMove);
            this.mouseUpEvent = new MouseEventHandler(this.canvas_MouseUp);
        }

        public override void Bind(ICamera camera, System.Windows.Forms.Control canvas)
        {
            if (camera == null || canvas == null) { throw new ArgumentNullException(); }

            this.camera = camera;
            this.canvas = canvas;

            canvas.KeyPress += this.keyPressEvent;
            canvas.MouseDown += this.mouseDownEvent;
            canvas.MouseMove += this.mouseMoveEvent;
            canvas.MouseUp += this.mouseUpEvent;
        }

        public override void Unbind()
        {
            this.camera = null;
            if (this.canvas != null && (!this.canvas.IsDisposed))
            {
                this.canvas.KeyPress -= this.keyPressEvent;
                this.canvas.MouseDown -= this.mouseDownEvent;
                this.canvas.MouseMove -= this.mouseMoveEvent;
                this.canvas.MouseUp -= this.mouseUpEvent;
            }
        }

        private bool mouseDownFlag = false;
        private int lastLocationX;
        private int lastLocationY;

        void canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mouseDownFlag = false;
        }

        void canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.mouseDownFlag)
            {

            }
        }

        void canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mouseDownFlag = true;
            this.lastLocationX = e.X;
            this.lastLocationY = e.Y;
        }

        void canvas_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == frontKey || e.KeyChar == upcaseFrontKey)
            {
                vec3 right = this.camera.GetRight();
                vec3 standardFront = this.camera.UpVector.cross(right).normalize();
                this.camera.Position += standardFront * this.StepLength;
                this.camera.Target += standardFront * this.StepLength;
            }
            else if (e.KeyChar == backKey || e.KeyChar == upcaseBackKey)
            {
                vec3 right = this.camera.GetRight();
                vec3 standardBack = right.cross(this.camera.UpVector).normalize();
                this.camera.Position += standardBack * this.StepLength;
                this.camera.Target += standardBack * this.StepLength;
            }
            else if (e.KeyChar == leftKey || e.KeyChar == upcaseLeftKey)
            {
                vec3 right = this.camera.GetRight();
                vec3 left = (-right).normalize();
                this.camera.Position += left * this.StepLength;
                this.camera.Target += left * this.StepLength;
            }
            else if (e.KeyChar == rightKey || e.KeyChar == upcaseRightKey)
            {
                vec3 right = this.camera.GetRight().normalize();
                this.camera.Position += right * this.StepLength;
                this.camera.Target += right * this.StepLength;
            }
        }

    }
}
