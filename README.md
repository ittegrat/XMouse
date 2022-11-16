# XMouse
Command line utility to configure *focus follows mouse* behavior (aka *active window tracking*) on Windows.

```
Usage: xmouse [option]...
If no options are specified, displays the current configuration.

Options (add 'x' to disable):
  -e[x]: enable xmouse behavior (tracking on, no raise, 200 msec delay)
  -t[x]: enable active window tracking
  -z[x]: enable active window tracking zorder
  -d MSEC: set active window tracking delay
  -h, --help: print this help
```

For a GUI utility, please see Joel Purra's [X-Mouse Controls](https://joelpurra.com/projects/X-Mouse_Controls/).
