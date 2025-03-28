# xzknshot

xzknshot is a **multi-monitor** screen capture application for Windows that uses **Tesseract OCR** to recognize text or math expressions. Easily copy selected regions to your clipboard and share them with ChatGPT or other tools.

## Features
- **Multi-monitor support**: Captures the entire virtual screen area, so you can select regions on any monitor.
- **Resizable selection**: Draw a rectangle and adjust its size with a resize thumb (corner handle).
- **OCR** powered by Tesseract:
  - Extract recognized text to your clipboard.
  - Convert math expressions using Regex or Sympy for better ChatGPT interpretation.
- A **menu panel** with quick actions (SS, Text, Math).
- Press **ESC** at any time to exit the application.

## Installation
1. Install **Tesseract** (for Windows, the [UB-Mannheim build](https://github.com/UB-Mannheim/tesseract/wiki) is recommended).
2. Clone or download this repository.
3. Open the project in Visual Studio (or any .NET-compatible IDE).
4. Make sure you have a `tessdata` folder containing at least `eng.traineddata`.
5. Build the project (e.g., “Build → Rebuild Solution”).

## How It Works
1. When you run xzknshot, it opens a transparent window covering all monitors.
2. Click and drag the left mouse button to draw a selection rectangle on any screen.
3. When you release the mouse button, a **menu panel** appears near the selection:
   - **SS**: Copies the screenshot of your selection to the clipboard, then exits.
   - **Text**: Performs OCR on the selected region, copies recognized text, then exits.
   - **Math**: Performs OCR, optionally applies Regex or Sympy transformations to create a ChatGPT-friendly math expression, then copies it and exits.
4. Press **ESC** to close the application immediately.

## Example Screenshot
*(Add a GIF or screenshot demonstrating usage if you like.)*

## Development
- Look at `MainWindow.xaml.cs` for methods such as `CaptureScreen()`, `PerformOcr()`, and `ConvertMathExpressionForChatGPT()`.
- Uses `SystemParameters.VirtualScreenWidth` (and related properties) for multi-monitor capturing.
- The **resize** feature uses a WPF `Thumb` control’s `DragDelta` event for resizing the selection rectangle.
- OCR integration relies on the **Tesseract** NuGet package.

## Contributing
- Open to pull requests and issues. Suggestions for adding new language models or advanced OCR features are welcome.
