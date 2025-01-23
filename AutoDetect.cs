using Newtonsoft.Json;
using System.Diagnostics;

namespace PosterOverlay
{
    public static class AutoDetect
    {
        public static DetectionResult AutoDetectProperties(string inputFile)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v quiet -print_format json -show_format -show_streams \"{inputFile}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start ffprobe process. Make sure ffprobe is in the same directory as this executable or in the system PATH.");
                }

                using (StreamReader reader = process.StandardOutput)
                {
                    process.WaitForExit();

                    string output = reader.ReadToEnd();
                    dynamic? jsonOutput = JsonConvert.DeserializeObject(output);

                    if (jsonOutput == null)
                    {
                        throw new InvalidOperationException("Failed to parse ffprobe JSON output.");
                    }

                    return new DetectionResult
                    {
                        Resolution = FindResolution(jsonOutput),
                        IsIMAX = DetectIMAX(jsonOutput),
                        IsDV = DetectDV(jsonOutput),
                        IsHDR = DetectHDR(jsonOutput),
                        IsHDR10Plus = DetectHDR10Plus(jsonOutput),
                        IsAtmos = DetectAtmos(jsonOutput),
                        IsDolbyTheatre = DetectDolbyTheatre(jsonOutput),
                        VideoCodec = DetectVideoCodec(jsonOutput),
                        AudioCodec = DetectAudioCodec(jsonOutput),
                        Is3D = Detect3D(jsonOutput)
                    };
                }
            }
        }

        private static string FindResolution(dynamic jsonOutput)
        {
            foreach (var stream in jsonOutput.streams)
            {
                if (stream.codec_type == "video")
                {
                    int width = stream.width;
                    int height = stream.height;

                    if (width >= 3840 && height >= 2160)
                        return "4K";
                    else if (width >= 1920 && height >= 1080)
                        return "1080p";
                    else if (width >= 1280 && height >= 720)
                        return "720p";
                }
            }

            return "Unknown";
        }

        private static bool DetectIMAX(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("IMAX") == true;
        private static bool DetectDV(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("dolby vision") == true;
        private static bool DetectHDR(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("HDR") == true;
        private static bool DetectHDR10Plus(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("HDR10+") == true;
        private static bool DetectAtmos(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("dolby atmos") == true;
        private static bool DetectDolbyTheatre(dynamic jsonOutput) => jsonOutput.format.tags?.title?.Contains("Dolby Theatre") == true;

        private static string DetectVideoCodec(dynamic jsonOutput)
        {
            foreach (var stream in jsonOutput.streams)
            {
                if (stream.codec_type == "video")
                {
                    return stream.codec_name;
                }
            }

            return "Unknown";
        }

        private static string DetectAudioCodec(dynamic jsonOutput)
        {
            foreach (var stream in jsonOutput.streams)
            {
                if (stream.codec_type == "audio")
                {
                    return stream.codec_name;
                }
            }

            return "Unknown";
        }

        private static bool Detect3D(dynamic jsonOutput)
        {
            string[] indicators = { "stereo3d", "3D", "side-by-side", "top-and-bottom", "frame packing", "stereoscopic", "MVC", "anaglyph", "depth map", "3D SBS", "3D TAB" };

            foreach (var stream in jsonOutput.streams)
            {
                foreach (string indicator in indicators)
                {
                    if (stream.tags?.stereo_mode?.Contains(indicator, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class DetectionResult
    {
        public string? Resolution { get; set; }
        public bool IsIMAX { get; set; }
        public bool IsDV { get; set; }
        public bool IsHDR { get; set; }
        public bool IsHDR10Plus { get; set; }
        public bool IsAtmos { get; set; }
        public bool IsDolbyTheatre { get; set; }
        public string? VideoCodec { get; set; }
        public string? AudioCodec { get; set; }
        public bool Is3D { get; set; }
    }    
}
