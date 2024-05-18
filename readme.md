# Monitorcontrol

I was tired of using the mouse or some not very reliable keyboard shortcuts to
switch the input adapters of my huge dell monitor. 

While looking for a command line tool to do the switching I found various
implementation in node and python. Looking at those implementation I realized I
could write it myself with some help from Chat GPT. 

This utility is a combination of some c# code that calls the native Windows APIs
and a PowerShell script that does the command line handling.

## Configuration
You can set the mapping of input source names to the numerical values in
`config.json`. This file will be read my the powershell function to provide the
parameter suggestions.
