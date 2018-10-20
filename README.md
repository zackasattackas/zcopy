# zcopy
.NET command-line file copy application.

### Examples
(see below for all command-line options)

This example recursively copies all files not matching the `*.png` or `*.mp3` wildcard patterns. The copy speed is displayed in `MB`/s.
```
C:\> zcopy \\dc01\Company F:\CompanyData -r -xf=*.png,*.mp3 -uom=MB
```

This example copies all ShadowProtect files. The MD5 checksum of the source and target files are calculated a compared. The checksum of the source is calculated during the copy operation. Once complete, the target file is hashed.
```
C:\> zcopy \\nas01\backups F:\backupImages *b00?*.* -md5
```

This example recursively copies all files from the source directory. Nested directories with the `Hidden` or `ReparsePoint` file attributes will not be copied/scanned. Files are copied simultaneously in multiple threads. Note that the output changes when `-mt` is specified.
```
C:\> zcopy C:\users\zack D:\MyProfileData -r -mt -da=HR!
```

### Command-line Syntax
```
C:\> zcopy --help
zcopy 0.0.227.230

File copy with progress!

Usage: zcopy [arguments] [options]

Arguments:
  source                      The source directory. Use '.' for the current directory
  destination                 The destination directory. Use '.' for the current directory
  files*                      The files to be copied
  directories*                The subdirectories to be copied if '--recurse' is specified

Options:
  --version                   Show version information
  -u|--username               Username to access the source and/or destination
  -p|--password               Password to access the source and/or destination
  -m|--move                   Delete the source files once copied
  -r|--recurse                Recusively scan the source directory for files to copy
  -md5                        Compare the MD5 checksum of the copied file to the source
  -mt|--multi-thread=<count>  Multithreaded operation. The default thread count is 8. (implies '-bo')
  -i|--info=<TAS>             File info to retain. Allowed values are 'T=timestamps', 'A=attributes', 'S=security'
  --regex                     Interpret search patterns as regular expressions*
  -xf=<files*>                Files to be excluded
  -xd=<directories*>          Subdirectories to be excluded
  -fa=<ACEHORS>               File attributes to include/exclude. Append '!' to exclude the specified attributes
  -da=<ACEHORS>               Directory attributes to include/exclude. Append '!' to exclude the specified attributes
  --max-age=<datetime>        Exclude files older than 'MM-dd-yyyy HH:mm'
  --min-age=<datetime>        Exclude files newer than 'MM-dd-yyyy HH:mm'
  --max-size=<bytes>          Exclude files larger than n bytes
  --min-size=<bytes>          Exclude files smaller than n bytes
  -no|--no-output             Do not write progress messages to the console
  -bo|--basic-output          Display basic output for improved performance
  -cr=<ms>                    The interval in milliseconds to refresh progress information at the console
  -uom=<uom>                  Display copy speed in uom/s
  -nh|--no-header             Do not print the header information
  -nf|--no-footer             Do not print the footer information
  -utc                        Interpret and display date/time values in coordinated universal time
  -?|-h|--help                Show help information

*Search filters support wildcards by default. Use '--regex' to enable regular expression pattern matching
```
