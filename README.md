# Carubbi Audio Converter API

Carubbi Audio Converter API is a modular and secure API built with ASP.NET Core to convert audio files between popular formats such as MP3, WAV, and OGG. The API ensures file validation, efficient processing, and extensibility for future formats.

## Features

- **Audio Format Conversion**:
  - Supported conversions:
    - MP3 ↔ WAV
    - WAV ↔ OGG
    - MP3 ↔ OGG (via pipeline)
- **File Validation**:
  - Ensures valid file extensions (`.mp3`, `.wav`, `.ogg`).
  - Validates file size limits.
  - Uses file signatures (magic numbers) to confirm file authenticity.
- **Extensibility**:
  - Easily add new audio formats by implementing the `IConverter` interface.
- **Integration with External Tools**:
  - Utilizes `opusenc` and `opusdec` for OGG encoding/decoding.
  - Leverages `NAudio` and `NAudio.Lame` for MP3 and WAV processing.

---

## Requirements

- .NET 7.0
- `opusenc.exe` and `opusdec.exe` available in the environment's PATH (for OGG conversions).
- NAudio and NAudio.Lame libraries installed.

---

## Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/rcarubbi/Carubbi-AudioConverter-Api.git
   cd Carubbi-AudioConverter-Api
   ```

2. **Install dependencies**:
   - Ensure that the `NAudio` and `NAudio.Lame` packages are installed:
     ```bash
     dotnet add package NAudio
     dotnet add package NAudio.Lame
     ```
   - Add the `opusenc.exe` and `opusdec.exe` binaries to your PATH environment variable.

3. **Run the API**:
   ```bash
   dotnet run
   ```

4. **Access the Swagger UI**:
   - Navigate to `https://localhost:<port>/swagger` to test the API endpoints interactively.

---

## API Endpoints

### POST /Conversion

Converts an uploaded audio file to the desired format.

#### Request
- **Query Parameter**:
  - `to`: The desired output format (`mp3`, `wav`, or `ogg`).
- **Form Data**:
  - `source`: The audio file to convert.

#### Response
- **Success**: Returns the converted audio file as a binary stream.
- **Error**: Provides a detailed error message if validation or conversion fails.

#### Example
```bash
curl -X POST "https://localhost:<port>/Conversion?to=ogg" \
    -H "Content-Type: multipart/form-data" \
    -F "source=@example.mp3" \
    --output result.ogg
```

---

## How It Works

### File Validation
Each uploaded file goes through a validation process:
1. **File Size**: Ensures the file does not exceed the configured size limit.
2. **Extension Check**: Validates if the file extension is supported.
3. **File Signature**: Confirms the authenticity of the file using magic numbers.

### Conversion Pipeline
The API uses a modular pipeline approach for audio conversion. It determines the necessary converters based on input and output formats and chains them together when intermediate steps are needed (e.g., MP3 → WAV → OGG).

- **Example**: Conversion from MP3 to OGG:
  1. MP3 → WAV (using `NAudio.Wave`).
  2. WAV → OGG (using `opusenc`).

### Integration with External Tools
- **Opusenc/Opusdec**: Handles OGG encoding/decoding using process execution.
- **NAudio**: Manages MP3 and WAV processing directly in memory.

---

## Adding New Formats

To add a new format:
1. Implement the `IConverter` interface:
   ```csharp
   public class NewFormatConverter : IConverter
   {
       public string From => "existingFormat";
       public string To => "newFormat";

       public async Task<byte[]> ConvertAsync(byte[] content)
       {
           // Conversion logic here
       }
   }
   ```

2. Register the converter in `Startup.cs`:
   ```csharp
   services.AddTransient<IConverter, NewFormatConverter>();
   ```

---

## Tools and Libraries Used

- **ASP.NET Core**: For API implementation.
- **NAudio**: For MP3 and WAV processing.
- **NAudio.Lame**: For MP3 encoding.
- **Opusenc/Opusdec**: For OGG encoding and decoding.

---

## Contributing

Contributions are welcome! Please fork the repository, create a new branch, and submit a pull request.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

---

## Project Link

Check out the repository: [Carubbi Audio Converter API](https://github.com/rcarubbi/Carubbi-AudioConverter-Api)
