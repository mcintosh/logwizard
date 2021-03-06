﻿/* 
 * Copyright (C) 2014-2015 John Torjo
 *
 * This file is part of LogWizard
 *
 * LogWizard is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LogWizard is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact john.code@torjo.com
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LogWizard {

    class win32 {
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x;
            public int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point p_point);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static Point GetMousePos()
        {
            Point pt = new Point();
            GetCursorPos(ref pt);
            return pt;
        }

        public static void SetMousePos(Point p) {
            SetMousePos(p.x, p.y);
        }
        public static bool SetMousePos(int x, int y) {
            return SetCursorPos(x, y);
        }

        [DllImport("user32.dll", SetLastError = true)] 
        public static extern bool BringWindowToTop(IntPtr hWnd); 
        [DllImport("user32")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        const UInt32 SWP_NOSIZE = 0x0001;

        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        public static void MakeTopMost(Form form) {
            IntPtr hWnd = form.Handle;

            // backup - hopefully we don't need to do this, but just in case
            BringWindowToTop(hWnd);
            const int SW_SHOW = 5;
            ShowWindow(hWnd, SW_SHOW);

            SetForegroundWindow(hWnd);
            SetForegroundWindow(hWnd);

            SetWindowPos(form.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

    }
}
