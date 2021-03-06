﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogWizard {
    // application settings
    class app {
        private static app inst_= new app();

        public static app inst {
            get { return inst_; }
        }

        // ... for file-to-file settings
        private string selected_log_file_name_ = "";

        // if true, we show how many lines each view has
        public bool show_view_line_count = true;
        // if true, we show the selected line index of each view
        public bool show_view_selected_index = true;
        // if true, we show the selected line of each view (the real line in the log)
        public bool show_view_selected_line = true;

        // if true, synchronize all views with existing view
        // (that is, when we selected line changes, the other views should go to the closest line to the selected line in this view)
        //
        // 1.0.56+ - don't have it by default, for large files (20+Mb) it's slow
        public bool sync_all_views = false;
        // if true, synchronize Full Log with existing view (that is, the selected line)
        public bool sync_full_log_view = true;

        // if true, I instantly refresh all views all the time
        // if false, I refresh only the current view
        public bool instant_refresh_all_views = true;

        // if true, we show the Topmost button (top-left)
        public bool show_topmost_toggle = false;

        // file-by-file
        public bool bring_to_top_on_restart = false;
        public bool make_topmost_on_restart = false;

        public void set_log_file(string file) {
            if (selected_log_file_name_ != "") 
                // save old settings
                load_save_file_by_file(false);

            selected_log_file_name_ = file;
            load_save_file_by_file(true);
        }

        private void load_save_file_by_file(bool load) {
            var sett = Program.sett;
            if (load) {
                string[] words = sett.get("settings_by_file." + selected_log_file_name_).Split(',');
                bring_to_top_on_restart = false;
                make_topmost_on_restart = true; // ... default
                foreach (string word in words)
                    switch (word) {
                    case "bring_to_top_on_restart":
                        bring_to_top_on_restart = true;
                        break;
                    case "not_make_topmost_on_restart":
                        make_topmost_on_restart = false;
                        break;
                    }
            } else {
                string words = "";
                if (bring_to_top_on_restart)
                    words += "bring_to_top_on_restart,";
                if (!make_topmost_on_restart)
                    words += "not_make_topmost_on_restart,";
                sett.set("settings_by_file." + selected_log_file_name_, words);
                sett.save();
            }
        }

        private void load_save(bool load, ref bool prop, string name, bool default_ = false) {
            var sett = Program.sett;
            if (load)
                prop = sett.get(name, default_ ? "1" : "0") != "0";
            else 
                sett.set(name, prop ? "1" : "0");
        }

        private void load_save(bool load) {
            load_save(load, ref show_view_line_count, "show_view_line_count", true);
            load_save(load, ref show_view_selected_line, "show_view_selected_line", true);
            load_save(load, ref show_view_selected_index, "show_view_selected_index", true);
            load_save(load, ref sync_all_views, "sync_all_views");
            load_save(load, ref sync_full_log_view, "sync_full_log_view", true);

            load_save(load, ref show_topmost_toggle, "show_topmost_toggle");
        }

        public void load() {
            load_save(true);
        }

        public void save() {
            load_save(false);
            if ( selected_log_file_name_ != "")
                load_save_file_by_file(false);
            var sett = Program.sett;
            sett.save();            
        }


    }

}
