define("ace/theme/tomorrow_night_purple",["require","exports","module","ace/lib/dom"], function(require, exports, module) {

exports.isDark = true;
exports.cssClass = "ace-tomorrow-night-purple";
exports.cssText = ".ace-tomorrow-night-purple .ace_gutter {\
background: #20103b;\
color: #7388b5\
}\
.ace-tomorrow-night-purple .ace_print-margin {\
width: 1px;\
background: #10052b\
}\
.ace-tomorrow-night-purple {\
background-color: #1c1831;\
color: #c0c0c0\
}\
.ace-tomorrow-night-purple .ace_constant.ace_other,\
.ace-tomorrow-night-purple .ace_cursor {\
color: #c0c0c0\
}\
.ace-tomorrow-night-purple .ace_marker-layer .ace_selection {\
background: #3F1F8E\
}\
.ace-tomorrow-night-purple.ace_multiselect .ace_selection.ace_start {\
box-shadow: 0 0 3px 0px #241c51;\
border-radius: 2px\
}\
.ace-tomorrow-night-purple .ace_marker-layer .ace_step {\
background: rgb(127, 111, 19)\
}\
.ace-tomorrow-night-purple .ace_marker-layer .ace_bracket {\
margin: -1px 0 0 -1px;\
border: 1px solid #4F407D\
}\
.ace-tomorrow-night-purple .ace_marker-layer .ace_active-line {\
background: #34046E\
}\
.ace-tomorrow-night-purple .ace_gutter-active-line {\
background-color: #200240\
}\
.ace-tomorrow-night-purple .ace_marker-layer .ace_selected-word {\
border: 1px solid #3F0F8E\
}\
.ace-tomorrow-night-purple .ace_invisible {\
color: #4F407D\
}\
.ace-tomorrow-night-purple .ace_keyword,\
.ace-tomorrow-night-purple .ace_meta,\
.ace-tomorrow-night-purple .ace_storage,\
.ace-tomorrow-night-purple .ace_storage.ace_type,\
.ace-tomorrow-night-purple .ace_support.ace_type {\
color: #EBBBFF\
}\
.ace-tomorrow-night-purple .ace_keyword.ace_operator {\
color: #99FFFF\
}\
.ace-tomorrow-night-purple .ace_constant.ace_character,\
.ace-tomorrow-night-purple .ace_constant.ace_language,\
.ace-tomorrow-night-purple .ace_constant.ace_numeric,\
.ace-tomorrow-night-purple .ace_keyword.ace_other.ace_unit,\
.ace-tomorrow-night-purple .ace_support.ace_constant,\
.ace-tomorrow-night-purple .ace_variable.ace_parameter {\
color: #FFC58F\
}\
.ace-tomorrow-night-purple .ace_invalid {\
color: #FFFFFF;\
background-color: #F99DA5\
}\
.ace-tomorrow-night-purple .ace_invalid.ace_deprecated {\
color: #FFFFFF;\
background-color: #EBBBFF\
}\
.ace-tomorrow-night-purple .ace_fold {\
background-color: #BBDAFF;\
border-color: #FFFFFF\
}\
.ace-tomorrow-night-purple .ace_entity.ace_name.ace_function,\
.ace-tomorrow-night-purple .ace_support.ace_function,\
.ace-tomorrow-night-purple .ace_variable {\
color: #BBDAFF\
}\
.ace-tomorrow-night-purple .ace_support.ace_class,\
.ace-tomorrow-night-purple .ace_support.ace_type {\
color: #FFEEAD\
}\
.ace-tomorrow-night-purple .ace_heading,\
.ace-tomorrow-night-purple .ace_markup.ace_heading,\
.ace-tomorrow-night-purple .ace_string {\
color: #D1F1A9\
}\
.ace-tomorrow-night-purple .ace_entity.ace_name.ace_tag,\
.ace-tomorrow-night-purple .ace_entity.ace_other.ace_attribute-name,\
.ace-tomorrow-night-purple .ace_meta.ace_tag,\
.ace-tomorrow-night-purple .ace_string.ace_regexp,\
.ace-tomorrow-night-purple .ace_variable {\
color: #FF9DA4\
}\
.ace-tomorrow-night-purple .ace_comment {\
color: #7285B7\
}\
.ace-tomorrow-night-purple .ace_indent-guide {\
background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWNgYGBgYJDzqfwPAANXAeNsiA+ZAAAAAElFTkSuQmCC) right repeat-y\
}";

var dom = require("../lib/dom");
dom.importCssString(exports.cssText, exports.cssClass);
});
