/* 
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LogWizard {
    class filter_line {
        protected bool Equals(filter_line other) {
            return part == other.part && comparison == other.comparison && string.Equals(text, other.text);// && Equals(fi, other.fi);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((filter_line) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) part;
                hashCode = (hashCode * 397) ^ (int) comparison;
                hashCode = (hashCode * 397) ^ (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fi != null ? fi.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(filter_line left, filter_line right) {
            return Equals(left, right);
        }

        public static bool operator !=(filter_line left, filter_line right) {
            return !Equals(left, right);
        }

        // ctxX -> context about the message (other than file/func/class)
        public enum part_type {
            date, time, level, message, file, func, ctx1, ctx2, ctx3, class_, font, case_sensitive_info,
            // not implemented yet
            thread
        }

        public enum comparison_type {
            equal, not_equal, starts_with, does_not_start_with, contains, does_not_contain,
            // 1.0.38+
            contains_any, contains_none
        }
        public part_type part;
        public comparison_type comparison;
        public string text = "";

        // in case we're looking for ANY/NONE
        public string[] words = null;

        // used only for case-insensitive compare
        private string lo_text = "";
        private string[] lo_words = null;

        public bool case_sensitive = true;

        // font -> contains details about the font; for now, the colors
        public class font_info {
            protected bool Equals(font_info other) {
                return fg.Equals(other.fg) && bg.Equals(other.bg);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((font_info) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return (fg.GetHashCode() * 397) ^ bg.GetHashCode();
                }
            }

            public static bool operator ==(font_info left, font_info right) {
                return Equals(left, right);
            }

            public static bool operator !=(font_info left, font_info right) {
                return !Equals(left, right);
            }

            public void copy_from(font_info other) {
                fg = other.fg;
                bg = other.bg;
            }

            public Color fg = util.transparent, bg = util.transparent;
            // FIXME in the future, allow other font info, such as "bold", font name, etc
            // FIXME for later - allow matching a text within the line - the text will be dumped in a different color (and font, maybe -> like bold?)

            // basically this is what "Used" means - dim this filter_row compared to the rest
            public static font_info dimmed = new font_info { bg = Color.White, fg = Color.LightGray };
        }
        public font_info fi = new font_info();


        public static filter_line parse(string line) {
            try {
                return parse_impl(line);
            } catch (Exception) {
                return null;
            }
        }

        // note: in the future, we might allow for more "font" data - at that point, I'll think about the syntax
        //
        // at this point, we allow a simple "color" line:
        // color fg [bg]
        private static filter_line parse_font(string line) {
            Debug.Assert(line.StartsWith("color"));
            string[] colors = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            filter_line fi = new filter_line { part = part_type.font };
            if (colors.Length >= 2)
                fi.fi.fg = util.str_to_color(colors[1]);
            if (colors.Length >= 3)
                fi.fi.bg = util.str_to_color(colors[2]);
            return fi;
        }

        // tries to parse a line - if it fails, it will return null
        private static filter_line parse_impl(string line) {
            line = line.Trim();
            if (line.StartsWith("#"))
                // allow comments
                return null;

            if (line.StartsWith("font") || line.StartsWith("color"))
                return parse_font(line);

            if (line == "case-insensitive")
                return new filter_line {part = part_type.case_sensitive_info, case_sensitive = false };

            if ( !line.StartsWith("$"))
                return null;

            string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            
            // Syntax:
            // $ColumnName Comparison Text
            if (words.Length < 3)
                // we need at least $c compare word(s)
                return null;

            filter_line fi = new filter_line();
            switch ( words[0]) {
            case "$date": fi.part = part_type.date; break;
            case "$time": fi.part = part_type.time; break;
            case "$level": fi.part = part_type.level; break;
            case "$file": fi.part = part_type.file; break;
            case "$func": fi.part = part_type.func; break;
            case "$msg": fi.part = part_type.message; break;
            case "$class": fi.part = part_type.class_; break;

            case "$ctx1": fi.part = part_type.ctx1; break;
            case "$ctx2": fi.part = part_type.ctx2; break;
            case "$ctx3": fi.part = part_type.ctx3; break;

            case "$thread": fi.part = part_type.thread; break;
            default:
                return null;
            }

            switch ( words[1].ToLower() ) {
            case "!=": fi.comparison = comparison_type.not_equal; break;

            case "==": 
            case "=": 
                fi.comparison = comparison_type.equal; break;

            case "+": 
            case "startswith":
                fi.comparison = comparison_type.starts_with; break;

            case "-": 
            case "!startswith":
                fi.comparison = comparison_type.does_not_start_with; break;

            case "++": 
            case "contains":
                fi.comparison = comparison_type.contains; break;

            case "--": 
            case "!contains":
                fi.comparison = comparison_type.does_not_contain; break;

            case "containsany":
            case "any":
                fi.comparison = comparison_type.contains_any; break;

            case "containsnone":
            case "none":
                fi.comparison = comparison_type.contains_none;
                break;
            default:
                return null;
            }

            // take the rest of the text
            int compare_idx = line.IndexOf(words[1]);
            line = line.Substring(compare_idx + words[1].Length).Trim();
            if (fi.comparison == comparison_type.contains_any || fi.comparison == comparison_type.contains_none) {
                fi.words = line.Split('|');
                fi.lo_words = fi.words.Select(w => w.ToLower()).ToArray();
            }
            fi.text = line;
            fi.lo_text = line.ToLower();

            return fi;
        }

        private string line_part(line l) {
            string sub = "";
            switch (part) {
            case part_type.date:
                sub = l.part(info_type.date);
                break;
            case part_type.time:
                sub = l.part(info_type.time);
                break;
            case part_type.level:
                sub = l.part(info_type.level);
                break;
            case part_type.message:
                sub = l.part(info_type.msg);
                break;
            case part_type.file:
                sub = l.part(info_type.file);
                break;
            case part_type.func:
                sub = l.part(info_type.func);
                break;
            case part_type.class_:
                sub = l.part(info_type.class_);
                break;

            case part_type.ctx1:
                sub = l.part(info_type.ctx1);
                break;
            case part_type.ctx2:
                sub = l.part(info_type.ctx2);
                break;
            case part_type.ctx3:
                sub = l.part(info_type.ctx3);
                break;
            case part_type.thread:
                sub = l.part(info_type.thread);
                break;

            default:
                Debug.Assert(false);
                break;
            }
            return sub;
        }

        private bool compare(string line_part, string text, string[] words) {
            bool result = true;
            switch (comparison) {
            case comparison_type.equal:
                result = line_part == text;
                break;
            case comparison_type.not_equal:
                result = line_part != text;
                break;
            case comparison_type.starts_with:
                result = line_part.StartsWith(text);
                break;
            case comparison_type.does_not_start_with:
                result = !line_part.StartsWith(text);
                break;
            case comparison_type.contains:
                result = line_part.Contains(text);
                break;
            case comparison_type.does_not_contain:
                result = !line_part.Contains(text);
                break;

            case comparison_type.contains_any:
                result = words.Any(line_part.Contains);
                break;
            case comparison_type.contains_none:
                if (words.Any(line_part.Contains)) 
                    result = false;                
                break;
            default:
                Debug.Assert(false);
                break;
            }

            return result;            
        }

        private bool matches_case_sensitive(line l) {
            Debug.Assert( part != part_type.font );
            string line_part = this.line_part(l);
            return compare(line_part, text, words);
        }

        private bool matches_case_insensitive(line l) {
            Debug.Assert( part != part_type.font );
            string line_part = this.line_part(l).ToLower();
            return compare(line_part, lo_text, lo_words);
        }

        public bool matches(line l) {
            return case_sensitive ? matches_case_sensitive(l) : matches_case_insensitive(l);
        }

    }
}