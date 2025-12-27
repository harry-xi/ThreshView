# ThreshView

ThreshView is a cross-platform grayscale image thresholding and overlay color visualization tool based on Avalonia UI. It is designed for image analysis, binary preprocessing, and similar scenarios. Users can load local grayscale images, adjust threshold and overlay color in real time, and preview the processed result instantly.

## Features
- Supports loading and thumbnail preview of various image formats (with a focus on grayscale images)
- Real-time threshold adjustment for convenient binarization of grayscale images
- Customizable overlay color (RGBA) to highlight thresholded regions
- Multi-image tab management for batch processing
## Installation & Running

### Requirements
- .NET 10.0 or later
- Avalonia UI

### Build & Run
1. Clone this repository:
   ```shell
   git clone https://github.com/harry-xi/ThreshView.git
   ```
2. Enter the project directory and restore dependencies:
   ```shell
   cd ThreshView
   dotnet restore
   ```
3. Build and run:
   ```shell
   dotnet run
   ```

