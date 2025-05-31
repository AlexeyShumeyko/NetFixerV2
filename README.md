# NetFix Tool - Network Diagnostics and Repair Utility

Automated tool for diagnosing and fixing common Windows network issues with intelligent optimization.

## 🛠️ Core Features

### 🌐 DNS Optimization Module
- Flushes DNS cache (`ipconfig /flushdns`)
- Benchmarks Google (8.8.8.8) vs Cloudflare (1.1.1.1) DNS servers
- Automatically configures fastest DNS resolver
- Verification via `nslookup`

### 🔌 Network Repair Module
- Ping and traceroute diagnostics
- Winsock stack reset (`netsh winsock reset`)
- MTU optimization:
  - Discovers maximum unfragmented packet size
  - Calculates optimal MTU (packet size + 28 bytes)
  - Applies to all physical interfaces

### ⚡ Connection Troubleshooter
- Proxy configuration analysis
- Critical endpoint availability testing
- Hosts file inspection
- DHCP refresh cycle:
  - Automatic DHCP interface detection
  - Executes `ipconfig /release` + `ipconfig /renew`

 ## 🚀 Installation

### Prerequisites
- Windows 10/11 (64-bit)
- PowerShell 5.1 or later
- Administrator privileges

### One-line Install
```powershell```
# Recommended method (auto-updates)
powershell -Command "iwr -useb https://raw.githubusercontent.com/AlexeyShumeyko/netfix/main/install.ps1 | iex"

### Manual Installation

<!-- Подробные шаги для ручной установки -->
1. **Download the release**:
   - Visit the [Releases page](https://github.com/AlexeyShumeyko/netfix/releases)
   - Download the latest `NetFixTool-[version].zip` package

2. **Extract files**:
   ```powershell```
   Expand-Archive -Path .\NetFixTool-[version].zip -DestinationPath C:\NetFixTool

## 🛠️ Core Features

| Module       | Functionality                                                                 |
|--------------|-------------------------------------------------------------------------------|
| **DNS**      | • Flush DNS cache<br>• Benchmark DNS servers<br>• Automatically configure fastest resolver |
| **Network**  | • Reset Winsock stack<br>• Optimize MTU size<br>• Diagnose connection issues  |
| **Connect**  | • Check proxy settings<br>• Test endpoint connectivity<br>• Analyze hosts file |

## ⚠️ Disclaimer 

### Safety Measures
- 🔄 **Automatically creates system restore points** before making changes
- 📋 **Logs all actions** in `%ProgramData%\NetFixTool\logs\`
- 🔍 **Provides dry-run mode** (`--diagnose`) to preview changes

### Limitations
- ❌ **No warranty** of any kind provided
- ⚠️ **Not responsible** for any system instability
- 💻 **Tested only on** Windows 10/11 x64 systems

### User Agreement
By using this tool, you agree:
1. You have **administrator privileges**
2. You understand the **risks of network configuration changes**
3. You **backed up important data** before execution

