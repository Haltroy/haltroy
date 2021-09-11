# HTUPDATE Architectures
HTUPDATE suuports multi-architecture projects and allows developers (like you) to 
target multiple architectures.

A HTUPDATE Arch is schemed like this:

        [Operating System]-[Version]-[Processor Architecture]

Except `[Processor Architecture]`, other parts are not required.

Examples: 
 - `win-7-x86` -> Only Microsoft Windows 7 supported  32-bit systems.
 - `win-x86` -> Only Microsoft Windows supported 32-bit systems.
 - `x86` ->  Only 32-bit supported systems.
 - `noarch` ->  Every system.
 - `fedora-34-noarch` -> Only Fedora GNU/Linux ver. 34 supported systems.
 - `arch-x64` -> Only Arch linux GNU/Linux supported 64-bit systems.

# [Processor Architecture]
This section contains a list of processor architectures that HTUPDATE can detect.

| Name | Short Name | Description |
|------|------------|-------------|
| No processor architectures  | noarch | All machines. |
| x86  | x86 | The 32-bit Intel/AMD desktop processor architecture. |
| x64  | x64 | The 64-bit Intel/AMD desktop processor architecture. |
| ARM  | arm | The 32-bit ARM desktop processor architecture. |
| ARM 64  | arm64 | The 64-bit ARM desktop processor architecture. |

# [Operating System] and [Version]
The [OS] part allows HTUPDATe to download packages different for each operating systems. The [V] part allows HTUPDATE to download the package that only targets that specific version or any other version that supports it.
The [OS] part must be included if you want to include the [V] part.

Here's a list of operating systems:

## Microsoft Windows

| Name | Short Name & Version | Description |
|------|------------|-------------|
| No Version  | win | All Windows machines. |
| 7  | win7 | Windows 7 and newer. |
| 7 Service Pack 1 | win7sp1 | Windows 7 Service Pack 1 (SP1) or newer. |
| 8  | win8 | Windows 8 or newer. |
| 8.1  | win8.1 | Windows 8.1 or newer. |
| 10  | win10 | Windows 10 or newer. |
| 10 [Any update] | win10-[Update short name] | Windows 10 versions after the update. Example: `win10-1703` -> Windows 10 1703 or newer. |
| 11 | win11 | Windows 11 or newer. |
| Server | winserver-[Update short name] | Windows Server versions after the update. Example: `winserver-1909` -> Windows Server 1909 or newer.. |

## Unix
`NOTE: Later versions of each of these operating systems can still install the old versions if their Archs are missing. 
For example: An OS ver. 5 user still can install a OS ver. 4 package if OS ver. 5 package is not listed, but an OS ver. 
4 user cannot install an OS ver. 5 package.`


| Name | Short Name | Description |
|------|------------|-------------|
| macOS | mac | All macOS machines. Derives from `unix`. |
| Unix | unix | Can be used as a base for almost all of the operating systems in this list. |
| Linux | linux | Derives from `unix`. Can be used as a base for almost all of the operating systems in this list. |
| BSD | bsd | Derives from `unix`. |
| FreeBSD | freebsd | Derives from `bsd`. |

### GNU/Linux Distributions
Here's a list of all of them:

| Name | Short Name | Notes |
|------|------------|-------|
| Alpine | alpine | Derives from `linux`. |
| Android | android | Derives from `linux`. |
| Arch Linux | arch | Derives from `linux`. |
| CentOS | centos | Derives from `linux`. |
| Debian | debian | Derives from `linux`. |
| Exherbo | exherbo | Derives from `linux`. |
| Fedora | fedora | Derives from `linux`. |
| Gentoo | gentoo | Derives from `linux`. |
| Linux Mint | linuxmint | Derives from `linux`. |
| Oracle Linux | ol | Derives from `linux`. |
| OpenSUSE | opensuse | Derives from `linux`. |
| Red Hat Linux Enterprise | rhel | Derives from `linux`. |
| SLES | sles | Derives from `linux`. |
| Solaris | solaris | Derives from `linux`. |
| Tizen | tizen | Derives from `linux`. |
| Void | void | Derives from `linux`. |
| Ubuntu | ubuntu | Derives from `linux`. |