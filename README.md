
# Bixolon Printer Integration in .NET MAUI

This project demonstrates how to integrate **Bixolon Bluetooth printers** into a **.NET 8 MAUI application**, addressing the lack of official documentation for this integration.  

We used the `BixolonPrinter.jar` file from the [fewlaps/bixolon-printer-example](https://github.com/fewlaps/bixolon-printer-example) repository because the official versions from Bixolon's website **did not work correctly** in this context.

## Features

- **Automatic Bluetooth Connection**: The app automatically connects to the paired Bixolon printer, simplifying the user experience.
- **Text Printing**: Supports direct text printing from the application.
- **Permission Management**: Handles required **Bluetooth and location permissions** at runtime.

## Project Structure

The project consists of the following main components:

- **BixolonPrinter.Maui**: The **main .NET 8 MAUI application**.
- **BixolonPrinter.Binding**: A **binding library** that includes the `BixolonPrinter.jar` file for printer integration.

## Project Setup

### Metadata File (`Transforms/Metadata.xml`)

Ensures that the **necessary classes** are public and removes **unnecessary classes**:

```xml
<metadata>
    <!-- Ensure that the necessary classes are public -->
    <attr
        path="/api/package[@name='com.bixolon.printer']/class[@name='BixolonPrinter']"
        name="visibility">public</attr>

    <!-- Remove unnecessary classes -->
    <remove-node path="/api/package[not(@name='com.bixolon.printer')]" />
</metadata>
```

### Binding Project (`BixolonPrinter.Binding.csproj`)

Includes the **Bixolon JAR file** and applies the **metadata transformation**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-android</TargetFramework>
    <SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <TransformFile Include="Transforms\Metadata.xml" />
    <EmbeddedJar Include="Jars\BixolonPrinter.jar" />
  </ItemGroup>
</Project>
```

---

## Implementation

### **Android Printer Service** (`AndroidPrinterService.cs`)

This service **handles Bluetooth connections** and **text printing** for Android devices in .NET 8 MAUI:

- **Checks Bluetooth availability**
- **Requests necessary permissions**
- **Connects to the printer via Bluetooth**
- **Sends text for printing**
- **Manages disconnections**

---

### Summary

- This project **enables Bluetooth printing** using **Bixolon printers** in **.NET 8 MAUI**.
- **Metadata adjustments** were made to the binding project to ensure proper integration.
- **Permissions handling** and **automatic Bluetooth connection** were implemented.

This project provides a **fully functional MAUI solution** for integrating **Bixolon Bluetooth printers** in **.NET 8** applications.

---

Let me know if you need further modifications! 🚀